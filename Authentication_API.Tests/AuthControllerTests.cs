using System.Net;
using System.Net.Http.Json;
using Authentication_API;
using Authentication_API.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/* Please read the explanation below
 
 * These tests start the WHOLE API in memory and send real HTTP requests to it,
 just like Swagger or the MVC app would. That means the real cookie is created on login,
 * and the [Authorize(Roles = "...")] attributes are actually checked.
 
 * FLOW OF EVERY TEST
 * 1. Setup() starts the API with an in-memory database instead of SQL Server.
 * 2. The test registers + logs in a user (the HttpClient keeps the login cookie automatically).
 * 3. The test calls a protected endpoint and checks the status code: 401, 403 or 200.
 */
[TestClass]
public class RoleAccessTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [TestInitialize]
    public void Setup()
    {
        var dbName = Guid.NewGuid().ToString(); // one new empty database per test

        // Start the real API in memory for testing
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // Replace the SQL Server database with an in-memory one (fake db)
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<DbContextOptions<AppDbContext>>();
                    services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
                    services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName));
                });
            });

        // HttpClient that talks to the in-memory API. It stores cookies like a browser does.
        _client = _factory.CreateClient();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _factory.Dispose();
    }


    [TestMethod]
    public async Task StudentOnly_NotLoggedIn_Returns401()
    {
        // Arrange: nobody logs in

        // Act: call a protected endpoint
        var response = await _client.GetAsync("api/student/student-only");

        // Assert [1]: 401 Unauthorized
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task TeacherOnly_AsStudent_Returns403()
    {
        // Arrange: register a Student, then log in (_client keeps the login cookie)
        RegisterDto newUser = new RegisterDto { Email = "amy@school.com", Password = "Pass123!", Role = "Student" };
        LoginDto loginUser = new LoginDto { Email = "amy@school.com", Password = "Pass123!" };
        
        await _client.PostAsJsonAsync("api/auth/register", newUser);
        await _client.PostAsJsonAsync("api/auth/login", loginUser);

        // Act: student tries the teacher-only endpoint
        var response = await _client.GetAsync("api/teacher/teacher-only");

        // Assert: 403 Forbidden (logged in, but wrong role)
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task TeacherOnly_AsTeacher_Returns200()
    {
        // Arrange: register a TEACHER, then log in (_client keeps the login cookie)
        RegisterDto newUser = new RegisterDto { Email = "Yusra@school.com", Password = "Pass123!", Role = "Teacher" };
        LoginDto loginUser = new LoginDto { Email = "Yusra@school.com", Password = "Pass123!" };

        await _client.PostAsJsonAsync("api/auth/register", newUser);
        await _client.PostAsJsonAsync("api/auth/login", loginUser);

        // Act: teacher calls the teacher-only endpoint
        var response = await _client.GetAsync("api/teacher/teacher-only");

        // Assert: 200 OK
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}