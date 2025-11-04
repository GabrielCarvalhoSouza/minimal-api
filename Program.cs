using MinimalApi.DTOs;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World! 1234");

app.MapPost("/login", (LoginDTO loginDTO) =>
{
    if (loginDTO.Email == "adm@teste.com" && loginDTO.Password == "1234")
    {
        return Results.Ok("Login realizado com sucesso!");
    }
    return Results.Unauthorized();
});


app.Run();


