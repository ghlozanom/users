using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

const string AuthScheme = "cookie";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(AuthScheme)
    .AddCookie("patreon")
    .AddCookie(AuthScheme)
    .AddOAuth("external-oauth", o => {
        o.SignInScheme = "patreon";

        o.ClientId = "id";
        o.ClientSecret = "secret";

        o.AuthorizationEndpoint = "http://oauth.mocklab.io/oauth/authorize";
        o.TokenEndpoint = "http://oauth.mocklab.io/oauth/token";

        o.CallbackPath = "/oauth-cb";
        o.Scope.Add("profile");
        o.SaveTokens = true;
    });

builder.Services.AddAuthorization(builder =>
{
    builder.AddPolicy("col passport", pb =>
    {
        pb.RequireAuthenticatedUser()
            .AddAuthenticationSchemes(AuthScheme, "patreon")
            .RequireClaim("passport", "COL");
    });

    builder.AddPolicy("user", pb =>
    {
        pb.RequireAuthenticatedUser()
            .AddAuthenticationSchemes(AuthScheme)
            .RequireAuthenticatedUser();
    });

    builder.AddPolicy("oauthuser", pb =>
    {
        pb.RequireAuthenticatedUser()
            .AddAuthenticationSchemes("patreon")
            .RequireAuthenticatedUser();
    });    
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = (int)HttpStatusCode.TemporaryRedirect;
    options.HttpsPort = 5001;
});

var app = builder.Build();

app.UseAuthentication();

app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/sweden", (HttpContext ctx) =>
{

    return "allowed";
});

app.MapGet("/col", (HttpContext ctx) =>
{
    return "allowed";

}).RequireAuthorization("col passport");

app.MapGet("/onlyoauth", (HttpContext ctx) =>
{
    return "allowed";

}).RequireAuthorization("oauthuser");

app.MapGet("/oauth-cb", (HttpContext ctx) =>
{
    return "allowed";
    
});

app.MapGet("/login", async (HttpContext ctx) =>
{
    var claims = new List<Claim>();
    claims.Add(new Claim("usr", "gabriel"));
    claims.Add(new Claim("passport", "COL"));
    var identity = new ClaimsIdentity(claims, AuthScheme);
    var user = new ClaimsPrincipal(identity);
    await ctx.SignInAsync(AuthScheme, user);
});

app.MapGet("/login-oauth", async (HttpContext ctx) =>
{
    await ctx.ChallengeAsync("external-oauth", new AuthenticationProperties()
    {
        RedirectUri = "/onlyoauth"
    });
}).RequireAuthorization("user");

app.Run();
