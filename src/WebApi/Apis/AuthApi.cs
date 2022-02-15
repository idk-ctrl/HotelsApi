namespace WebApi.Apis;

public class AuthApi
{
    public void Register(WebApplication app)
    {
        app.MapGet("/login", [AllowAnonymousAttribute] async (HttpContext context,
            ITokenService tokenService, IUserRepository userRepository) =>
        {

            var userModel = new UserModel()
            {
                UserName = context.Request.Query["username"],
                Password = context.Request.Query["password"]
            };
            var userDto = userRepository.GetUser(userModel);
            if (userDto == null) return Results.Unauthorized();
            var token = tokenService.BuildToken(app.Configuration["Jwt:Key"],
                app.Configuration["Jwt.Issuer"], userDto);

            return Results.Ok(token);
        });
    }
}
