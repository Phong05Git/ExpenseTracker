using ExpenseTracker.Application.DTOs.Auth;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;

namespace ExpenseTracker.UnitTests;

public class AuthServiceLoginTests
{
    [Fact]
    public async Task LoginAsync_InvalidPasswordDoesNotLookUpAccountOrWrite()
    {
        var fixture = new LoginFixture(priorFailures: 4, lockUntil: DateTime.UtcNow.AddMinutes(10));
        var before = (fixture.User.FailedAttempts, fixture.User.LockUntil, fixture.User.UpdatedAt);
        string?[] rejected = [null, "", new string('a', 73), new string('é', 37), "fixture\0", "fixture\uD800"];

        foreach (var password in rejected)
        {
            var result = await fixture.Service.LoginAsync(fixture.Request(password!));
            Assert.False(result.IsSuccess);
            Assert.Equal("Invalid username or password.", result.Error);
        }

        Assert.Equal(before, (fixture.User.FailedAttempts, fixture.User.LockUntil, fixture.User.UpdatedAt));
        Assert.Empty(fixture.UserLookups);
        Assert.Empty(fixture.PasswordChecks);
        Assert.Empty(fixture.Saves);
        fixture.AssertNoSessionOrTokens();
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_PersistsSessionBeforeIssuingAccessToken()
    {
        // BE-UT.01-01
        var fixture = new LoginFixture();
        using var cancellation = new CancellationTokenSource();
        var before = DateTime.UtcNow;

        var result = await fixture.Service.LoginAsync(
            new LoginRequestDto { Username = "  login-fixture  ", Password = LoginFixture.ValidPassword },
            cancellation.Token);

        var after = DateTime.UtcNow;
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        var response = Assert.IsType<AuthResponseDto>(result.Data);
        Assert.Equal((fixture.User.Username, cancellation.Token), Assert.Single(fixture.UserLookups));
        Assert.Equal((LoginFixture.ValidPassword, fixture.User.PasswordHash), Assert.Single(fixture.PasswordChecks));

        var refresh = Assert.Single(fixture.RefreshRequests);
        var session = Assert.Single(fixture.AddedSessions);
        var access = Assert.Single(fixture.AccessRequests);
        Assert.Equal(fixture.User.Id, refresh.UserId);
        Assert.Equal(fixture.User.Id, session.UserId);
        Assert.InRange(refresh.ExpiresAt, before.AddDays(fixture.RefreshTokenDays), after.AddDays(fixture.RefreshTokenDays));
        Assert.InRange(access.ExpiresAt, before.AddMinutes(fixture.AccessTokenMinutes), after.AddMinutes(fixture.AccessTokenMinutes));
        Assert.InRange(session.LastActivityAt, before, after);
        Assert.Equal(refresh.ExpiresAt, session.ExpiresAt);
        Assert.Null(session.RevokedAt);
        Assert.Same(fixture.User, access.User);
        Assert.Equal(LoginFixture.SavedSessionId, session.Id);
        Assert.Equal(session.Id, access.SessionId);
        Assert.Equal(LoginFixture.AccessToken, response.AccessToken);
        Assert.Equal(session.Token, response.RefreshToken);
        Assert.Equal(access.ExpiresAt, response.AccessTokenExpiresAt);
        Assert.Equal(session.ExpiresAt, response.RefreshTokenExpiresAt);
        Assert.Equal(cancellation.Token, Assert.Single(fixture.SessionAddTokens));
        Assert.Collection(fixture.Saves,
            first => Assert.Equal(new SaveSnapshot(0, null, null, cancellation.Token), first),
            second => Assert.Equal(new SaveSnapshot(0, null, session.Id, cancellation.Token), second));
    }

    [Fact]
    public async Task LoginAsync_UnknownUsername_ReturnsGenericFailureWithoutWriting()
    {
        // BE-UT.01-02
        var fixture = new LoginFixture();

        var result = await fixture.Service.LoginAsync(
            new LoginRequestDto { Username = "missing-fixture", Password = LoginFixture.ValidPassword });

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal("Invalid username or password.", result.Error);
        Assert.Equal("missing-fixture", Assert.Single(fixture.UserLookups).Username);
        Assert.Empty(fixture.Saves);
        fixture.AssertNoSessionOrTokens();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public async Task LoginAsync_WrongPasswordBelowThreshold_PersistsExactlyOneFailure(int priorFailures)
    {
        // BE-UT.01-03
        var fixture = new LoginFixture(priorFailures);

        var result = await fixture.Service.LoginAsync(fixture.Request(LoginFixture.WrongPassword));

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal("Invalid username or password.", result.Error);
        Assert.Equal((LoginFixture.WrongPassword, fixture.User.PasswordHash), Assert.Single(fixture.PasswordChecks));
        Assert.Equal(priorFailures + 1, fixture.User.FailedAttempts);
        Assert.Null(fixture.User.LockUntil);
        Assert.Equal(new SaveSnapshot(priorFailures + 1, null, null, default), Assert.Single(fixture.Saves));
        fixture.AssertNoSessionOrTokens();
    }

    [Fact]
    public async Task LoginAsync_FifthFailure_PersistsFifteenMinuteLock()
    {
        // BE-UT.01-04
        var fixture = new LoginFixture(priorFailures: 4);
        var before = DateTime.UtcNow;

        var result = await fixture.Service.LoginAsync(fixture.Request(LoginFixture.WrongPassword));

        var after = DateTime.UtcNow;
        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(5, fixture.User.FailedAttempts);
        var lockUntil = Assert.IsType<DateTime>(fixture.User.LockUntil);
        Assert.InRange(lockUntil, before.AddMinutes(15), after.AddMinutes(15));
        Assert.Equal(new SaveSnapshot(5, lockUntil, null, default), Assert.Single(fixture.Saves));
        fixture.AssertNoSessionOrTokens();
    }

    [Fact]
    public async Task LoginAsync_ActiveLock_RejectsEvenCorrectPasswordWithoutChangingState()
    {
        // BE-UT.01-05
        var lockUntil = DateTime.UtcNow.AddMinutes(10);
        var fixture = new LoginFixture(priorFailures: 5, lockUntil: lockUntil);

        var result = await fixture.Service.LoginAsync(fixture.Request(LoginFixture.ValidPassword));

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(5, fixture.User.FailedAttempts);
        Assert.Equal(lockUntil, fixture.User.LockUntil);
        Assert.Empty(fixture.Saves);
        fixture.AssertNoSessionOrTokens();
    }

    [Fact]
    public async Task LoginAsync_ExpiredLockAndCorrectPassword_ClearsLockAndCreatesSession()
    {
        // BE-UT.01-06
        var fixture = new LoginFixture(priorFailures: 5, lockUntil: DateTime.UtcNow.AddMinutes(-10));

        var result = await fixture.Service.LoginAsync(fixture.Request(LoginFixture.ValidPassword));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(0, fixture.User.FailedAttempts);
        Assert.Null(fixture.User.LockUntil);
        var session = Assert.Single(fixture.AddedSessions);
        Assert.Collection(fixture.Saves,
            first => Assert.Equal(new SaveSnapshot(0, null, null, default), first),
            second => Assert.Equal(new SaveSnapshot(0, null, session.Id, default), second));
        Assert.Equal(session.Id, Assert.Single(fixture.AccessRequests).SessionId);
    }

    [Fact]
    public async Task LoginAsync_CorrectPasswordAfterFailures_PersistsReset()
    {
        // BE-UT.01-07
        var fixture = new LoginFixture(priorFailures: 4);

        var result = await fixture.Service.LoginAsync(fixture.Request(LoginFixture.ValidPassword));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(0, fixture.User.FailedAttempts);
        Assert.Null(fixture.User.LockUntil);
        var session = Assert.Single(fixture.AddedSessions);
        Assert.Collection(fixture.Saves,
            first => Assert.Equal(new SaveSnapshot(0, null, null, default), first),
            second => Assert.Equal(new SaveSnapshot(0, null, session.Id, default), second));
    }

    [Fact]
    public async Task LoginAsync_AccountSaveFails_DoesNotCreateSessionOrTokens()
    {
        // BE-UT.01-08
        var fixture = new LoginFixture(priorFailures: 1) { FailOnSave = 1 };

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => fixture.Service.LoginAsync(fixture.Request(LoginFixture.ValidPassword)));

        Assert.Same(fixture.SaveFailure, error);
        Assert.Equal(new SaveSnapshot(0, null, null, default), Assert.Single(fixture.Saves));
        fixture.AssertNoSessionOrTokens();
    }

    [Fact]
    public async Task LoginAsync_SessionSaveFails_DoesNotIssueAccessToken()
    {
        // BE-UT.01-09
        var fixture = new LoginFixture { FailOnSave = 2 };

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => fixture.Service.LoginAsync(fixture.Request(LoginFixture.ValidPassword)));

        Assert.Same(fixture.SaveFailure, error);
        Assert.Single(fixture.RefreshRequests);
        Assert.Equal(0, Assert.Single(fixture.AddedSessions).Id);
        Assert.Collection(fixture.Saves,
            first => Assert.Equal(new SaveSnapshot(0, null, null, default), first),
            second => Assert.Equal(new SaveSnapshot(0, null, null, default), second));
        Assert.Empty(fixture.AccessRequests);
    }

    private record SaveSnapshot(int FailedAttempts, DateTime? LockUntil, int? SavedSessionId,
        CancellationToken CancellationToken);

    private sealed class LoginFixture : IUserRepository, IRefreshTokenRepository, IUnitOfWork,
        IPasswordHasher, ITokenService, ITokenBlacklistService, IJwtTokenSettings
    {
        public const string ValidPassword = "synthetic-valid-input";
        public const string WrongPassword = "synthetic-invalid-input";
        public const string AccessToken = "synthetic-access-output";
        public const int SavedSessionId = 73;
        private const string PasswordHash = "synthetic-password-hash";

        public User User { get; } = new("login-fixture", PasswordHash, "Fixture User", "fixture@example.invalid") { Id = 42 };
        public AuthService Service { get; }
        public int AccessTokenMinutes => 7;
        public int RefreshTokenDays => 3;
        public int? FailOnSave { get; init; }
        public InvalidOperationException SaveFailure { get; } = new("Synthetic persistence failure.");
        public List<(string Username, CancellationToken Token)> UserLookups { get; } = [];
        public List<(string Password, string Hash)> PasswordChecks { get; } = [];
        public List<(int UserId, DateTime ExpiresAt)> RefreshRequests { get; } = [];
        public List<(User User, int SessionId, DateTime ExpiresAt)> AccessRequests { get; } = [];
        public List<RefreshToken> AddedSessions { get; } = [];
        public List<CancellationToken> SessionAddTokens { get; } = [];
        public List<SaveSnapshot> Saves { get; } = [];

        public LoginFixture(int priorFailures = 0, DateTime? lockUntil = null)
        {
            for (var i = 0; i < priorFailures; i++)
                User.IncreaseFailedAttempts();
            if (lockUntil is DateTime expiry)
                User.Lock(expiry);

            Service = new AuthService(this, this, this, this, this, this, this);
        }

        public LoginRequestDto Request(string password) => new() { Username = User.Username, Password = password };

        public void AssertNoSessionOrTokens()
        {
            Assert.Empty(RefreshRequests);
            Assert.Empty(AddedSessions);
            Assert.Empty(AccessRequests);
        }

        Task<User?> IUserRepository.GetByUsernameAsync(string username, CancellationToken cancellationToken)
        {
            UserLookups.Add((username, cancellationToken));
            return Task.FromResult<User?>(username == User.Username ? User : null);
        }

        bool IPasswordHasher.Verify(string password, string hashedPassword)
        {
            PasswordChecks.Add((password, hashedPassword));
            return password == ValidPassword && hashedPassword == PasswordHash;
        }

        RefreshToken ITokenService.GenerateRefreshToken(int userId, DateTime expiresAt)
        {
            RefreshRequests.Add((userId, expiresAt));
            return new RefreshToken(userId, "synthetic-refresh-output", expiresAt);
        }

        Task IRepository<RefreshToken>.AddAsync(RefreshToken entity, CancellationToken cancellationToken)
        {
            AddedSessions.Add(entity);
            SessionAddTokens.Add(cancellationToken);
            return Task.CompletedTask;
        }

        string ITokenService.GenerateAccessToken(User user, int sessionId, DateTime expiresAt)
        {
            AccessRequests.Add((user, sessionId, expiresAt));
            return AccessToken;
        }

        Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
        {
            var fail = FailOnSave == Saves.Count + 1;
            var session = AddedSessions.LastOrDefault();
            if (!fail && session is not null)
                session.Id = SavedSessionId;

            Saves.Add(new SaveSnapshot(User.FailedAttempts, User.LockUntil,
                session is { Id: > 0 } ? session.Id : null, cancellationToken));
            return fail ? Task.FromException<int>(SaveFailure) : Task.FromResult(1);
        }

        Task<User?> IRepository<User>.GetByIdAsync(int id, CancellationToken cancellationToken) => throw new NotSupportedException();
        Task<User?> IUserRepository.GetByEmailAsync(string email, CancellationToken cancellationToken) => throw new NotSupportedException();
        Task IRepository<User>.AddAsync(User entity, CancellationToken cancellationToken) => throw new NotSupportedException();
        void IRepository<User>.Update(User entity) => throw new NotSupportedException();
        void IRepository<User>.Remove(User entity) => throw new NotSupportedException();
        Task<RefreshToken?> IRepository<RefreshToken>.GetByIdAsync(int id, CancellationToken cancellationToken) => throw new NotSupportedException();
        Task<RefreshToken?> IRefreshTokenRepository.GetByTokenAsync(string token, CancellationToken cancellationToken) => throw new NotSupportedException();
        Task<IReadOnlyList<RefreshToken>> IRefreshTokenRepository.GetActiveByUserIdAsync(int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        Task IRefreshTokenRepository.RevokeAllByUserIdAsync(int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        void IRepository<RefreshToken>.Update(RefreshToken entity) => throw new NotSupportedException();
        void IRepository<RefreshToken>.Remove(RefreshToken entity) => throw new NotSupportedException();
        string IPasswordHasher.Hash(string password) => throw new NotSupportedException();
        string? ITokenService.GetTokenId(string accessToken) => throw new NotSupportedException();
        DateTime? ITokenService.GetTokenExpiration(string accessToken) => throw new NotSupportedException();
        void ITokenBlacklistService.Revoke(string tokenId, DateTime expiresAt) => throw new NotSupportedException();
        bool ITokenBlacklistService.IsRevoked(string tokenId) => throw new NotSupportedException();
    }
}
