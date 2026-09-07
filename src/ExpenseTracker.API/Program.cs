using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using ExpenseTracker.API.Filters;
using ExpenseTracker.API.Middleware;
using ExpenseTracker.API.Swagger;
using ExpenseTracker.Application;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Infrastructure;
using ExpenseTracker.Infrastructure.Authentication;
using ExpenseTracker.Infrastructure.Security.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings is missing.");

var rateLimitSettings = builder.Configuration
    .GetSection("RateLimitSettings")
    .Get<RateLimitSettings>()
    ?? throw new InvalidOperationException("RateLimitSettings is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = JwtRegisteredClaimNames.UniqueName
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var tokenId = context.Principal?
                    .FindFirstValue(JwtRegisteredClaimNames.Jti);

                if (string.IsNullOrWhiteSpace(tokenId))
                {
                    context.Fail("JWT does not contain a jti claim.");
                    return Task.CompletedTask;
                }

                var blacklistService = context.HttpContext.RequestServices
                    .GetRequiredService<ITokenBlacklistService>();

                if (blacklistService.IsRevoked(tokenId))
                    context.Fail("Access token has been revoked.");

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AddProblemDetails();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(
                MetadataName.RetryAfter,
                out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds)
                .ToString(CultureInfo.InvariantCulture);
        }

        context.HttpContext.Response.ContentType =
            "application/problem+json";

        await context.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                status = StatusCodes.Status429TooManyRequests,
                title = "Too many requests.",
                detail = "Rate limit exceeded. Please try again later."
            },
            cancellationToken);
    };

    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(
            httpContext =>
            {
                var userId = httpContext.User
                    .FindFirstValue(JwtRegisteredClaimNames.Sub);

                var partitionKey = !string.IsNullOrWhiteSpace(userId)
                    ? $"user:{userId}"
                    : $"ip:{httpContext.Connection.RemoteIpAddress}";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit =
                            rateLimitSettings.GlobalPermitLimit,
                        Window =
                            TimeSpan.FromMinutes(
                                rateLimitSettings.GlobalWindowMinutes),
                        QueueLimit = 0
                    });
            });

    options.AddFixedWindowLimiter(
        "auth",
        limiterOptions =>
        {
            limiterOptions.AutoReplenishment = true;
            limiterOptions.PermitLimit =
                rateLimitSettings.AuthPermitLimit;
            limiterOptions.Window =
                TimeSpan.FromMinutes(
                    rateLimitSettings.AuthWindowMinutes);
            limiterOptions.QueueLimit = 0;
        });
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ExpenseTracker API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter a valid JWT access token."
    });

    options.OperationFilter<AuthorizeOperationFilter>();
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

//app.UseRouting();

app.UseAuthentication();
app.UseRateLimiter();
app.UseMiddleware<SessionActivityMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();