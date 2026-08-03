import os

file_path = "/home/zeyad/Downloads/Schedulas/backend/tests/Schedulas.Application.Tests/Features/People/PeopleManagementTests.cs"
with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the whole constructor block
old_constructor = """    public PeopleManagementTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<SchedulasDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(new SoftDeleteInterceptor())
            .Options;

        _tenantServiceMock = new Mock<ITenantService>();
        _tenantServiceMock.Setup(t => t.IsTenantEnforced).Returns(false); // Platform admin by default for setup

        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(c => c.Role).Returns(UserRole.PlatformAdmin);

        using var context = new SchedulasDbContext(_options, _tenantServiceMock.Object);
        context.Database.EnsureCreated();
    }"""

new_constructor = """    public PeopleManagementTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _tenantServiceMock = new Mock<ITenantService>();
        _tenantServiceMock.Setup(t => t.IsTenantEnforced).Returns(false); // Platform admin by default for setup

        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(c => c.Role).Returns(UserRole.PlatformAdmin);

        _options = new DbContextOptionsBuilder<SchedulasDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(new AuditableSaveChangesInterceptor(_currentUserMock.Object))
            .Options;

        using var context = new SchedulasDbContext(_options, _tenantServiceMock.Object);
        context.Database.EnsureCreated();
    }"""

content = content.replace(old_constructor, new_constructor)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)

print("Fixed constructor")
