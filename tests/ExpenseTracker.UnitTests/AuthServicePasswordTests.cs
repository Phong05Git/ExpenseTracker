using ExpenseTracker.Application.DTOs.Auth;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;

namespace ExpenseTracker.UnitTests;

public class AuthServicePasswordTests
{
    [Fact]
    public async Task ChangePasswordAsync_InvalidCurrentPasswordDoesNotLookUpAccountOrWrite()
    {
        var fixture = new PasswordFixture();
        var before = (fixture.User.FailedAttempts, fixture.User.LockUntil, fixture.User.UpdatedAt);
        string?[] rejected = [null, "", new string('a', 73), new string('é', 37), "fixture\0", "fixture\uD800"];

        foreach (var password in rejected)
        {
            var request = new ChangePasswordRequestDto { CurrentPassword = password!, NewPassword = NewPassword };
            var result = await fixture.Service.ChangePasswordAsync(fixture.User.Id, request, PasswordFixture.Bearer);
            Assert.False(result.IsSuccess);
            Assert.Equal("Current password is incorrect.", result.Error);
        }

        Assert.Equal(before, (fixture.User.FailedAttempts, fixture.User.LockUntil, fixture.User.UpdatedAt));
        Assert.Equal(OldHash, fixture.User.PasswordHash);
        Assert.True(fixture.CurrentSession.IsActive);
        Assert.True(fixture.OtherSession.IsActive);
        Assert.Empty(fixture.UserLookups);
        Assert.Empty(fixture.Verifications);
        Assert.Empty(fixture.HashInputs);
        Assert.Empty(fixture.Saves);
        Assert.Empty(fixture.Revocations);
        Assert.Empty(fixture.BlacklistWrites);
        fixture.AssertNoIssuedSession();
    }

    [Fact]
    public async Task RegisterAsync_NewPasswordOutsideByteLimit_DoesNotPersistOrIssueTokens()
    {
        var fixture = new PasswordFixture();
        var request = new RegisterRequestDto
        {
            Username = Registration.Username, FullName = Registration.FullName, Email = Registration.Email,
            Password = "Aa1!a" + new string('é', 34)
        };

        var result = await fixture.Service.RegisterAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Empty(fixture.UsernameLookups);
        Assert.Empty(fixture.EmailLookups);
        Assert.Empty(fixture.HashInputs);
        Assert.Null(fixture.AddedUser);
        Assert.Empty(fixture.Saves);
        fixture.AssertNoIssuedSession();
    }

    [Fact]
    public async Task ChangePasswordAsync_InvalidNewPassword_DoesNotChangeCredentialsOrRevokeSessions()
    {
        var fixture = new PasswordFixture();
        var request = new ChangePasswordRequestDto
        {
            CurrentPassword = CurrentPassword, NewPassword = "Aa1!fixture\0"
        };

        var result = await fixture.Service.ChangePasswordAsync(fixture.User.Id, request, PasswordFixture.Bearer);

        Assert.False(result.IsSuccess);
        Assert.Empty(fixture.UserLookups);
        Assert.Empty(fixture.Verifications);
        Assert.Empty(fixture.HashInputs);
        Assert.Equal(OldHash, fixture.User.PasswordHash);
        Assert.True(fixture.CurrentSession.IsActive);
        Assert.True(fixture.OtherSession.IsActive);
        Assert.Empty(fixture.Revocations);
        Assert.Empty(fixture.BlacklistWrites);
        Assert.Empty(fixture.Saves);
        fixture.AssertNoIssuedSession();
    }

    [Fact]
    public async Task RegisterAsync_NewUser_PersistsNormalizedUserAndHashedPassword()
    {
        // BE-UT.03-01
        var fixture = new PasswordFixture();
        using var cancellation = new CancellationTokenSource();

        var result = await fixture.Service.RegisterAsync(Registration, cancellation.Token);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        var response = Assert.IsType<AuthResponseDto>(result.Data);
        Assert.Equal(("new-user", cancellation.Token), Assert.Single(fixture.UsernameLookups));
        Assert.Equal(("new@example.invalid", cancellation.Token), Assert.Single(fixture.EmailLookups));
        Assert.Equal(NewPassword, Assert.Single(fixture.HashInputs));
        var user = Assert.IsType<User>(fixture.AddedUser);
        Assert.Equal("new-user", user.Username);
        Assert.Equal("new@example.invalid", user.Email);
        Assert.Equal("New User", user.FullName);
        Assert.Equal(NewHash, user.PasswordHash);
        Assert.NotEqual(Registration.Password, user.PasswordHash);
        Assert.Equal(0, user.FailedAttempts);
        Assert.Null(user.LockUntil);
        var session = Assert.IsType<RefreshToken>(fixture.AddedSession);
        Assert.Equal(user.Id, session.UserId);
        Assert.All(fixture.Saves, save => Assert.Equal(cancellation.Token, save.Token));
        Assert.Contains(fixture.Saves, save => save.AddedUserId == user.Id && save.PasswordHash == NewHash);
        Assert.Contains(fixture.Saves, save => save.SavedSessionId == session.Id && save.Token == cancellation.Token);
        Assert.Equal((user.Id, session.Id), Assert.Single(fixture.AccessRequests));
        Assert.Equal(session.Token, response.RefreshToken);
        Assert.Equal("synthetic-issued-access", response.AccessToken);
        Assert.Empty(fixture.BlacklistWrites);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RegisterAsync_DuplicateIdentity_RejectsWithoutPersistenceOrTokens(bool duplicateUsername)
    {
        // BE-UT.03-02, BE-UT.03-03
        var fixture = new PasswordFixture { DuplicateUsername = duplicateUsername, DuplicateEmail = !duplicateUsername };

        var result = await fixture.Service.RegisterAsync(Registration);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal("new-user", Assert.Single(fixture.UsernameLookups).Value);
        if (!duplicateUsername)
            Assert.Equal("new@example.invalid", Assert.Single(fixture.EmailLookups).Value);
        Assert.Null(fixture.AddedUser);
        Assert.Empty(fixture.Saves);
        fixture.AssertNoIssuedSession();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task RegisterAsync_SaveFails_DoesNotIssueAccessToken(int failedSave)
    {
        // BE-UT.03-04
        var fixture = new PasswordFixture { FailOnSave = failedSave };

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => fixture.Service.RegisterAsync(Registration));

        Assert.Same(fixture.PersistenceFailure, error);
        Assert.Equal(failedSave, fixture.Saves.Count);
        Assert.Empty(fixture.AccessRequests);
        Assert.Null(fixture.Saves[^1].SavedSessionId);
        if (failedSave == 1)
            fixture.AssertNoIssuedSession();
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidCurrentPassword_UpdatesHashAndRevokesOwnedSessions()
    {
        // BE-UT.03-05
        var fixture = new PasswordFixture();
        using var cancellation = new CancellationTokenSource();
        var priorRevocation = fixture.RevokedSession.RevokedAt;

        var result = await fixture.Service.ChangePasswordAsync(
            fixture.User.Id, ChangeRequest, PasswordFixture.Bearer, cancellation.Token);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Equal((fixture.User.Id, cancellation.Token), Assert.Single(fixture.UserLookups));
        Assert.Equal((CurrentPassword, OldHash), Assert.Single(fixture.Verifications));
        Assert.Equal(NewPassword, Assert.Single(fixture.HashInputs));
        Assert.Equal(NewHash, fixture.User.PasswordHash);
        Assert.NotEqual(NewPassword, fixture.User.PasswordHash);
        Assert.Equal((fixture.User.Id, cancellation.Token), Assert.Single(fixture.Revocations));
        Assert.False(fixture.CurrentSession.IsActive);
        Assert.False(fixture.OtherSession.IsActive);
        Assert.True(fixture.ForeignSession.IsActive);
        Assert.Null(fixture.ExpiredSession.RevokedAt);
        Assert.Equal(priorRevocation, fixture.RevokedSession.RevokedAt);
        var save = Assert.Single(fixture.Saves);
        Assert.Equal(NewHash, save.PasswordHash);
        Assert.Equal(cancellation.Token, save.Token);
        Assert.Equal(2, save.RevokedActiveSessions);
        Assert.Equal((PasswordFixture.BearerId, fixture.BearerExpiry), Assert.Single(fixture.BlacklistWrites));
        fixture.AssertNoIssuedSession();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ChangePasswordAsync_UserMissingOrCurrentPasswordWrong_LeavesStateUntouched(bool userExists)
    {
        // BE-UT.03-06, BE-UT.03-07
        var fixture = new PasswordFixture { UserExists = userExists };
        var request = new ChangePasswordRequestDto { CurrentPassword = "synthetic-wrong-input", NewPassword = NewPassword };

        var result = await fixture.Service.ChangePasswordAsync(fixture.User.Id, request, PasswordFixture.Bearer);

        Assert.False(result.IsSuccess);
        Assert.Equal(fixture.User.Id, Assert.Single(fixture.UserLookups).Id);
        if (userExists)
            Assert.Equal((request.CurrentPassword, OldHash), Assert.Single(fixture.Verifications));
        Assert.Equal(OldHash, fixture.User.PasswordHash);
        Assert.Empty(fixture.HashInputs);
        Assert.Empty(fixture.Revocations);
        Assert.Empty(fixture.Saves);
        Assert.Empty(fixture.BlacklistWrites);
        Assert.True(fixture.CurrentSession.IsActive);
        Assert.True(fixture.OtherSession.IsActive);
        fixture.AssertNoIssuedSession();
    }

    [Fact]
    public async Task ChangePasswordAsync_SaveFails_PropagatesPersistenceError()
    {
        // BE-UT.03-08
        var fixture = new PasswordFixture { FailOnSave = 1 };

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.Service.ChangePasswordAsync(fixture.User.Id, ChangeRequest, PasswordFixture.Bearer));

        Assert.Same(fixture.PersistenceFailure, error);
        var attemptedSave = Assert.Single(fixture.Saves);
        Assert.Equal(NewHash, attemptedSave.PasswordHash);
        Assert.Equal(2, attemptedSave.RevokedActiveSessions);
        fixture.AssertNoIssuedSession();
    }

    private const string CurrentPassword = "synthetic-current-input";
    private const string NewPassword = "Synthetic-new1!";
    private const string OldHash = "synthetic-old-hash";
    private const string NewHash = "synthetic-replacement-hash";

    private static RegisterRequestDto Registration => new()
    {
        Username = "  new-user  ", FullName = "  New User  ", Email = "  New@Example.Invalid  ", Password = NewPassword
    };

    private static ChangePasswordRequestDto ChangeRequest =>
        new() { CurrentPassword = CurrentPassword, NewPassword = NewPassword };

    private record SaveSnapshot(int? AddedUserId, int? SavedSessionId, string PasswordHash,
        int RevokedActiveSessions, CancellationToken Token);

    private sealed class PasswordFixture : IUserRepository, IRefreshTokenRepository, IUnitOfWork,
        IPasswordHasher, ITokenService, ITokenBlacklistService, IJwtTokenSettings
    {
        public const string Bearer = "synthetic-current-access";
        public const string BearerId = "synthetic-current-jti";
        public User User { get; } = new("existing-user", OldHash, "Existing User", "existing@example.invalid") { Id = 42 };
        public RefreshToken CurrentSession { get; } = MakeSession(42, "current");
        public RefreshToken OtherSession { get; } = MakeSession(42, "other");
        public RefreshToken ForeignSession { get; } = MakeSession(99, "foreign");
        public RefreshToken ExpiredSession { get; } = MakeSession(42, "expired", -1);
        public RefreshToken RevokedSession { get; } = MakeSession(42, "revoked");
        public DateTime BearerExpiry { get; } = DateTime.UtcNow.AddHours(1);
        public bool DuplicateUsername { get; init; }
        public bool DuplicateEmail { get; init; }
        public bool UserExists { get; init; } = true;
        public int? FailOnSave { get; init; }
        public int AccessTokenMinutes => 7;
        public int RefreshTokenDays => 3;
        public AuthService Service { get; }
        public InvalidOperationException PersistenceFailure { get; } = new("Synthetic persistence failure.");
        public User? AddedUser { get; private set; }
        public RefreshToken? AddedSession { get; private set; }
        public int RefreshRequests { get; private set; }
        public List<string> HashInputs { get; } = [];
        public List<(string Password, string Hash)> Verifications { get; } = [];
        public List<(string Value, CancellationToken Token)> UsernameLookups { get; } = [];
        public List<(string Value, CancellationToken Token)> EmailLookups { get; } = [];
        public List<(int Id, CancellationToken Token)> UserLookups { get; } = [];
        public List<(int Id, CancellationToken Token)> Revocations { get; } = [];
        public List<(int UserId, int SessionId)> AccessRequests { get; } = [];
        public List<(string TokenId, DateTime ExpiresAt)> BlacklistWrites { get; } = [];
        public List<SaveSnapshot> Saves { get; } = [];

        public PasswordFixture()
        {
            RevokedSession.Revoke();
            Service = new AuthService(this, this, this, this, this, this, this);
        }

        private static RefreshToken MakeSession(int owner, string label, int hours = 1) =>
            new(owner, $"synthetic-{label}-refresh", DateTime.UtcNow.AddHours(hours));

        public void AssertNoIssuedSession()
        {
            Assert.Equal(0, RefreshRequests);
            Assert.Null(AddedSession);
            Assert.Empty(AccessRequests);
        }

        Task<User?> IUserRepository.GetByUsernameAsync(string username, CancellationToken cancellationToken)
        {
            UsernameLookups.Add((username, cancellationToken));
            return Task.FromResult<User?>(DuplicateUsername && username == "new-user" ? User : null);
        }

        Task<User?> IUserRepository.GetByEmailAsync(string email, CancellationToken cancellationToken)
        {
            EmailLookups.Add((email, cancellationToken));
            return Task.FromResult<User?>(DuplicateEmail && email == "new@example.invalid" ? User : null);
        }

        Task<User?> IRepository<User>.GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            UserLookups.Add((id, cancellationToken));
            return Task.FromResult<User?>(UserExists && id == User.Id ? User : null);
        }

        Task IRepository<User>.AddAsync(User entity, CancellationToken cancellationToken)
        {
            AddedUser = entity;
            return Task.CompletedTask;
        }

        string IPasswordHasher.Hash(string password)
        {
            HashInputs.Add(password);
            return NewHash;
        }

        bool IPasswordHasher.Verify(string password, string hashedPassword)
        {
            Verifications.Add((password, hashedPassword));
            return password == CurrentPassword && hashedPassword == OldHash;
        }

        Task IRefreshTokenRepository.RevokeAllByUserIdAsync(int userId, CancellationToken cancellationToken)
        {
            Revocations.Add((userId, cancellationToken));
            var sessions = new[] { CurrentSession, OtherSession, ForeignSession, ExpiredSession, RevokedSession };
            foreach (var session in sessions.Where(session => session.UserId == userId && session.IsActive))
                session.Revoke();
            return Task.CompletedTask;
        }

        Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
        {
            var failed = FailOnSave == Saves.Count + 1;
            if (!failed && AddedUser is not null)
                AddedUser.Id = 57;
            if (!failed && AddedSession is not null)
                AddedSession.Id = 81;

            Saves.Add(new SaveSnapshot(
                AddedUser is { Id: > 0 } user ? user.Id : null,
                AddedSession is { Id: > 0 } session ? session.Id : null,
                AddedUser?.PasswordHash ?? User.PasswordHash,
                new[] { CurrentSession, OtherSession }.Count(session => session.RevokedAt.HasValue),
                cancellationToken));
            return failed ? Task.FromException<int>(PersistenceFailure) : Task.FromResult(1);
        }

        RefreshToken ITokenService.GenerateRefreshToken(int userId, DateTime expiresAt)
        {
            RefreshRequests++;
            return new RefreshToken(userId, "synthetic-new-refresh", expiresAt);
        }

        Task IRepository<RefreshToken>.AddAsync(RefreshToken entity, CancellationToken cancellationToken)
        {
            AddedSession = entity;
            return Task.CompletedTask;
        }

        string ITokenService.GenerateAccessToken(User user, int sessionId, DateTime expiresAt)
        {
            AccessRequests.Add((user.Id, sessionId));
            return "synthetic-issued-access";
        }

        string? ITokenService.GetTokenId(string accessToken) => accessToken == Bearer ? BearerId : null;
        DateTime? ITokenService.GetTokenExpiration(string accessToken) => accessToken == Bearer ? BearerExpiry : null;
        void ITokenBlacklistService.Revoke(string tokenId, DateTime expiresAt) => BlacklistWrites.Add((tokenId, expiresAt));
        bool ITokenBlacklistService.IsRevoked(string tokenId) => throw new NotSupportedException();
        void IRepository<User>.Update(User entity) => throw new NotSupportedException();
        void IRepository<User>.Remove(User entity) => throw new NotSupportedException();
        Task<RefreshToken?> IRepository<RefreshToken>.GetByIdAsync(int id, CancellationToken cancellationToken) => throw new NotSupportedException();
        Task<RefreshToken?> IRefreshTokenRepository.GetByTokenAsync(string token, CancellationToken cancellationToken) => throw new NotSupportedException();
        Task<IReadOnlyList<RefreshToken>> IRefreshTokenRepository.GetActiveByUserIdAsync(int userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        void IRepository<RefreshToken>.Update(RefreshToken entity) => throw new NotSupportedException();
        void IRepository<RefreshToken>.Remove(RefreshToken entity) => throw new NotSupportedException();
    }
}
