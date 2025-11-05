using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Interfaces;
using minimal_api.Dominio.ModelsViews;
using minimal_api.Dominio.Servicos;
using minimal_api.Infraestrutura.Db;


# region Builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbContexto>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
        );
});
var app = builder.Build();
#endregion

# region Home
app.MapGet("/", () => new Home()).WithTags("Home");
# endregion

# region Administradores
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
    if (administradorServico.Login(loginDTO) != null)
    {
        return Results.Ok("Login realizado com sucesso!");
    }
    return Results.Unauthorized();
}).WithTags("Administradores");
#endregion

#region Veiculos
app.MapPost("/veiculos", ([FromBody] VeiculoDTO VeiculoDTO, IVeiculoServico veiculoServico) =>
{
    var veiculo = new Veiculo {
        Nome = VeiculoDTO.Nome,
        Marca = VeiculoDTO.Marca,
        Ano = VeiculoDTO.Ano
    };
    veiculoServico.Incluir(veiculo);
    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery]int? pagina, [FromQuery]string? nome, [FromQuery]string? marca, [FromQuery]int? ano,  IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina, nome, marca, ano);
    return Results.Ok(veiculos);
}).WithTags("Veiculos");

#endregion

# region App
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
#endregion


