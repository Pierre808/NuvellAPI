using System.Net;
using System.Net.Http.Json;
using NuvellAPI.Models.DTO;

namespace NuvellApi.IntegrationTests;

public class AuthControllerTests(IntegrationTestWebApplicationFactory factory)
    : IClassFixture<IntegrationTestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    
    /*
    [Fact]
    public async Task Register_ReturnsBadRequest_WhenUserAlreadyExists()
    {
        // user test@test-mail.com exists already due to seeding
        var registerDto = new RegisterDto
        { 
            Email = "test@test-mail.com", 
            Password = "P@ssw0rd" 
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
         
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Email already exists", content);
    }
    
    [Fact]
    public async Task Register_ReturnsOk_WhenNewUser()
    {
        // user test2@test-mail.com is a new user
        var registerDto = new RegisterDto
        { 
            Email = "test2@test-mail.com", 
            Password = "P@ssw0rd" 
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
         
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("test2@test-mail.com", content);
    }
    */
    
    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenEmailDoesNotExist()
    {
        // user peter@test-mail.com does not exist
        var loginDto = new LoginDto
        { 
            Email = "peter@test-mail.com", 
            Password = "P@ssw0rd" 
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Log the actual response
        string responseBody = await response.Content.ReadAsStringAsync();
        Assert.Equal("", responseBody);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
         
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid email or password", content);
    }
    
    /*
    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenWrongPassword()
    {
        // password P@zzw0rd is wrong
        var loginDto = new LoginDto
        { 
            Email = "test@test-mail.com", 
            Password = "P@zzw0rd" 
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
         
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid email or password", content);
    }
    
    [Fact]
    public async Task Login_ReturnsOk_WhenValidCredentials()
    {
        var loginDto = new LoginDto
        { 
            Email = "test@test-mail.com", 
            Password = "P@ssw0rd" 
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
         
        var content = await response.Content.ReadAsStringAsync();
        
        var userResponse = System.Text.Json.JsonSerializer.Deserialize<UserResponseDto>(content);

        Assert.NotNull(userResponse);
        Assert.False(string.IsNullOrEmpty(userResponse.Email)); 
        Assert.False(string.IsNullOrEmpty(userResponse.AccessToken)); 
        Assert.False(string.IsNullOrEmpty(userResponse.RefreshToken));
    }
    
    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenRefreshingInvalidRefreshToken()
    {
        var loginDto = new LoginDto
        { 
            Email = "test@test-mail.com", 
            Password = "P@ssw0rd" 
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var userResponse = System.Text.Json.JsonSerializer.Deserialize<UserResponseDto>(content);

        Assert.NotNull(userResponse);
        var validAccessToken = userResponse.AccessToken;
        var validRefreshToken = userResponse.RefreshToken;
        
        // Step 2: attempt to refresh the token with an invalid refresh token
        await AssertInvalidTokenRefresh(validAccessToken, "invalid_refresh_token", "test@test-mail.com");

        // Step 3: attempt to refresh the token with an invalid e-mail address
        await AssertInvalidTokenRefresh(validAccessToken, validRefreshToken, "peter@test-mail.com");

        // Step 4: attempt to refresh the token with wrong access token
        await AssertInvalidTokenRefresh("eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJ0ZXN0MkBnbWFpbC5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjAxOTUyYTUyLWJlOWEtN2UxYS1iYzQ1LTQ1MWU0MWZlYTI3OSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlVzZXIiLCJleHAiOjE3NDAyNjEwMjksImlzcyI6ImV4YW1wbGUuY29tIiwiYXVkIjoiZXhhbXBsZS5jb20ifQ.FbrNRsqNqGn4dlchg3k18_3ZGWXRXKrIC8_FOlfSzB68DOltrEorqBugwlVLpdzmeXm0hiZqAO37MfajtLSXXg", 
            validRefreshToken, "test@test-mail.com");
    }
    
    private async Task AssertInvalidTokenRefresh(string accessToken, string refreshToken, string email)
    {
        var refreshTokenDto = new UserResponseDto()
        {
            Email = email,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    
        var response = await _client.PostAsJsonAsync("/api/auth/token/refresh", refreshTokenDto);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    */

}