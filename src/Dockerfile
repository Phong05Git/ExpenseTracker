FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/ExpenseTracker.API/ExpenseTracker.API.csproj src/ExpenseTracker.API/
COPY src/ExpenseTracker.Application/ExpenseTracker.Application.csproj src/ExpenseTracker.Application/
COPY src/ExpenseTracker.Domain/ExpenseTracker.Domain.csproj src/ExpenseTracker.Domain/
COPY src/ExpenseTracker.Infrastructure/ExpenseTracker.Infrastructure.csproj src/ExpenseTracker.Infrastructure/

RUN dotnet restore src/ExpenseTracker.API/ExpenseTracker.API.csproj

COPY . .

RUN dotnet publish src/ExpenseTracker.API/ExpenseTracker.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

USER $APP_UID

CMD ["sh", "-c", "dotnet ExpenseTracker.API.dll --urls [http://0.0.0.0:${PORT:-10000](http://0.0.0.0:${PORT:-10000)}"]
