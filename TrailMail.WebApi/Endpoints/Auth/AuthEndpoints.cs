using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Client.WebIntegration;
using TrailMail.WebApi.Context;
using TrailMail.WebApi.Entities;

namespace TrailMail.WebApi.Endpoints.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/Auth").RequireAuthorization();

        auth.MapGet("/WhoAmI", WhoAmI);
        auth.MapGet("/Login", Login).AllowAnonymous();
        auth.MapMethods("/Callback/LinkedIn", [HttpMethods.Get, HttpMethods.Post], LinkedInCallback).AllowAnonymous();


        return app;
    }

    private static IResult Login()
    {
        return Results.Challenge(
            properties: null,
            authenticationSchemes: [OpenIddictClientWebIntegrationConstants.Providers.LinkedIn]
        );
    }

    private static async Task<IResult> WhoAmI(HttpContext context, AppDbContext dbContext, CancellationToken ct)
    {
        var id = Guid.Parse(
            context.User.GetClaim(ClaimTypes.NameIdentifier) ?? throw new Exception("Name identifier is missing.")
        );

        var user = await dbContext.Users.SingleOrDefaultAsync(
            u => u.Id == id,
            cancellationToken: ct
        );

        ArgumentNullException.ThrowIfNull(user);

        return TypedResults.Json(new
        {
            user.Id,
            user.FirstName,
            user.LastName,
            user.Picture,
            user.LinkedInId,
            user.Name
        });
    }

    private static async Task<IResult> LinkedInCallback(HttpContext context, AppDbContext dbContext,
        CancellationToken ct)
    {
        var result = await context.AuthenticateAsync(
            OpenIddictClientWebIntegrationConstants.Providers.LinkedIn
        );

        ArgumentNullException.ThrowIfNull(result.Properties);
        ArgumentNullException.ThrowIfNull(result.Principal);

        var user = await dbContext.Users.SingleOrDefaultAsync(
            u => u.LinkedInId == result.Principal.GetClaim(ClaimTypes.NameIdentifier),
            cancellationToken: ct
        );

        if (user is null)
        {
            var entity = await dbContext.Users.AddAsync(
                new User
                {
                    FirstName = result.Principal.GetClaim(ClaimTypes.GivenName) ??
                                throw new Exception("Given name is missing."),
                    LastName = result.Principal.GetClaim(ClaimTypes.Surname) ??
                               throw new Exception("Surname is missing."),
                    LinkedInId = result.Principal.GetClaim(ClaimTypes.NameIdentifier) ??
                                 throw new Exception("NameIdentifier is missing."),
                    Picture = new Uri(
                        result.Principal.GetClaims("picture").FirstOrDefault() ??
                        throw new Exception("Picture is missing."),
                        UriKind.Absolute
                    ),
                },
                ct);

            await dbContext.SaveChangesAsync(ct);

            user = entity.Entity;
        }

        var identity = new ClaimsIdentity(
            authenticationType: OpenIddictClientWebIntegrationConstants.Providers.LinkedIn
        );

        identity.SetClaim(ClaimTypes.NameIdentifier, user.Id.ToString());

        var properties = new AuthenticationProperties(result.Properties.Items)
        {
            RedirectUri = result.Properties.RedirectUri ?? "/Auth/WhoAmI"
        };

        return Results.SignIn(new ClaimsPrincipal(identity), properties);
    }
}