FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ExpenseTracker.API/ExpenseTracker.API.csproj ExpenseTracker.API/
COPY ExpenseTracker.Application/ExpenseTracker.Application.csproj ExpenseTracker.Application/
COPY ExpenseTracker.Domain/ExpenseTracker.Domain.csproj ExpenseTracker.Domain/
COPY ExpenseTracker.Infrastructure/ExpenseTracker.Infrastructure.csproj ExpenseTracker.Infrastructure/

RUN dotnet restore ExpenseTracker.API/ExpenseTracker.API.csproj

COPY . .

RUN dotnet publish ExpenseTracker.API/ExpenseTracker.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

CMD ["sh", "-c", "dotnet ExpenseTracker.API.dll --urls http://0.0.0.0:${PORT:-10000}"]