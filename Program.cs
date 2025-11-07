using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Enuns;
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


app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{
    var validacao = new ErrosValidacao();
    validacao.mensagens = new List<string>();

    if (string.IsNullOrEmpty(administradorDTO.Email))
    {
        validacao.mensagens.Add("O campo Email é obrigatório.");
    }
    if (string.IsNullOrEmpty(administradorDTO.Senha))
    {
        validacao.mensagens.Add("O campo Senha é obrigatório.");
    }
    if(administradorDTO.Perfil == null)
    {
        validacao.mensagens.Add("O campo Perfil é obrigatório.");
    }
    if (validacao.mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    var administrador = new Administrador {
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString(),
    };
    administradorServico.Incluir(administrador);
    return Results.Created($"/administrador/{administrador.Id}", new AdministradoresModelView
        {
            Id = administrador.Id,
            Email = administrador.Email,
            Perfil = administrador.Perfil,
        });
}).WithTags("Administradores");


app.MapGet("/administrador/{id}", ([FromRoute]int id, IAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscarPorId(id);
    if (administrador == null) return Results.NotFound();
    return Results.Ok(new AdministradoresModelView
        {
            Id = administrador.Id,
            Email = administrador.Email,
            Perfil = administrador.Perfil,
        });

}).WithTags("Administradores");


app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
{
    var adms = new List<AdministradoresModelView>();
    var administradores = administradorServico.Todos(pagina);
    foreach (var adm in administradores)
    {
        adms.Add(new AdministradoresModelView
        {
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil,
        });
    }
    return Results.Ok(adms);
}).WithTags("Administradores");

#endregion

#region Veiculos
ErrosValidacao validaDTO(VeiculoDTO veiculoDTO)
{
    var validacao = new ErrosValidacao();
    validacao.mensagens = new List<string>();

    if (string.IsNullOrEmpty(veiculoDTO.Nome))
    {
        validacao.mensagens.Add("O campo Nome é obrigatório.");
    }
    if (string.IsNullOrEmpty(veiculoDTO.Marca))
    {
        validacao.mensagens.Add("O campo Marca é obrigatório.");
    }
    if (veiculoDTO.Ano < 1950)
    {
        validacao.mensagens.Add("O campo Ano precisa ser maior que 1950.");
    }

    return validacao;

}


app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var validacao = validaDTO(veiculoDTO);
    if (validacao.mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    var veiculo = new Veiculo {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };
    veiculoServico.Incluir(veiculo);
    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).WithTags("Veiculos");


app.MapGet("/veiculos", ([FromQuery] int? pagina, [FromQuery] string? nome, [FromQuery] string? marca, [FromQuery] int? ano, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina, nome, marca, ano);
    return Results.Ok(veiculos);
}).WithTags("Veiculos");


app.MapGet("/veiculo/{id}", ([FromRoute]int id,  IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscarPorId(id);
    if (veiculo == null) return Results.NotFound();
    return Results.Ok(veiculo);

}).WithTags("Veiculos");


app.MapPut("/veiculos/{id}", ([FromRoute] int id, [FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var validacao = validaDTO(veiculoDTO);
    if (validacao.mensagens.Count > 0)
    {
        return Results.BadRequest(validacao);
    }

    var veiculo = veiculoServico.BuscarPorId(id);
    if (veiculo == null) return Results.NotFound();

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoServico.Atualizar(veiculo);

    return Results.Ok(veiculo);

}).WithTags("Veiculos");


app.MapDelete("/veiculos/{id}", ([FromRoute]int id,  IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscarPorId(id);
    if (veiculo == null) return Results.NotFound();

    veiculoServico.Apagar(veiculo);
    return Results.NoContent();

}).WithTags("Veiculos");

#endregion

# region App
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
#endregion


