using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

const string AuthScheme = "cookie";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(AuthScheme)
    .AddCookie(AuthScheme);

builder.Services.AddAuthorization(builder =>
{
    builder.AddPolicy("col passport", pb =>
    {
        pb.RequireAuthenticatedUser()
            .AddAuthenticationSchemes(AuthScheme)
            .RequireClaim("passport", "COL");
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

// app.Use((ctx, next) =>
// {
//     if (ctx.Request.Path.StartsWithSegments("/login"))
//     {
//         return next();
//     }

//     if (ctx.Request.Path.StartsWithSegments("/favicon"))
//     {
//         return next();
//     }    

//     if (!ctx.User.Identities.Any(identity => identity.AuthenticationType == AuthScheme))
//     {
//         ctx.Response.StatusCode = 401;
//         return Task.CompletedTask;
//     }

//     if (!ctx.User.HasClaim("passport", "COL"))
//     {
//         ctx.Response.StatusCode = 403;
//         return Task.CompletedTask;
//     }    

//     return next();
// });

// Instead:
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
    // if (!ctx.User.Identities.Any(identity => identity.AuthenticationType == AuthScheme))
    // {
    //     ctx.Response.StatusCode = 401;
    //     return "";
    // }

    // if (!ctx.User.HasClaim("passport", "eur"))
    // {
    //     ctx.Response.StatusCode = 403;
    //     return "";
    // }

    return "allowed";
});

app.MapGet("/col", (HttpContext ctx) =>
{
    // if (!ctx.User.Identities.Any(identity => identity.AuthenticationType == AuthScheme))
    // {
    //     ctx.Response.StatusCode = 401;
    //     return "";
    // }

    // if (!ctx.User.HasClaim("passport", "COL"))
    // {
    //     ctx.Response.StatusCode = 403;
    //     return "";
    // }

    return "allowed";
}).RequireAuthorization("col passport");

app.MapGet("/login", async (HttpContext ctx) =>
{
    var claims = new List<Claim>();
    claims.Add(new Claim("usr", "gabriel"));
    claims.Add(new Claim("passport", "COL"));
    var identity = new ClaimsIdentity(claims, AuthScheme);
    var user = new ClaimsPrincipal(identity);
    await ctx.SignInAsync(AuthScheme, user);
}
);

app.Run();
