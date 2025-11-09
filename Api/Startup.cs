using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Enuns;
using minimal_api.Dominio.Interfaces;
using minimal_api.Dominio.ModelsViews;
using minimal_api.Dominio.Servicos;
using minimal_api.Infraestrutura.Db;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        key = Configuration.GetSection("Jwt").ToString() ?? "1234";
    }

    private string key;
    public IConfiguration Configuration { get; set; } = default!;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }
        ).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

        services.AddAuthorization();

        services.AddScoped<IAdministradorServico, AdministradorServico>();
        services.AddScoped<IVeiculoServico, VeiculoServico>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira  o token JWT aqui!"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        services.AddDbContext<DbContexto>(options =>
        {
            options.UseMySql(
                Configuration.GetConnectionString("mysql"),
                ServerVersion.AutoDetect(Configuration.GetConnectionString("mysql"))
                );
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", () => new Home()).AllowAnonymous().WithTags("Home");

            string GerarTokenJwt(Administrador administrador)
            {
                var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>()
                {
                    new Claim("Email", administrador.Email),
                    new Claim("Perfil", administrador.Perfil),
                    new Claim(ClaimTypes.Role, administrador.Perfil)
                };

                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }


            endpoints.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
            {
                var adm = administradorServico.Login(loginDTO);
                if (adm == null)
                {
                    return Results.Unauthorized();
                }
                string token = GerarTokenJwt(adm);
                return Results.Ok(new AdministradorLogado
                {
                    Email = adm.Email,
                    Perfil = adm.Perfil,
                    Token = token
                });
            }).AllowAnonymous().WithTags("Administradores");


            endpoints.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
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
                if (administradorDTO.Perfil == null)
                {
                    validacao.mensagens.Add("O campo Perfil é obrigatório.");
                }
                if (validacao.mensagens.Count > 0)
                {
                    return Results.BadRequest(validacao);
                }

                var administrador = new Administrador
                {
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
            }).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administradores");


            endpoints.MapGet("/administrador/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
            {
                var administrador = administradorServico.BuscarPorId(id);
                if (administrador == null) return Results.NotFound();
                return Results.Ok(new AdministradoresModelView
                {
                    Id = administrador.Id,
                    Email = administrador.Email,
                    Perfil = administrador.Perfil,
                });

            }).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administradores");


            endpoints.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
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
            }).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administradores");


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


            endpoints.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
            {
                var validacao = validaDTO(veiculoDTO);
                if (validacao.mensagens.Count > 0)
                {
                    return Results.BadRequest(validacao);
                }

                var veiculo = new Veiculo
                {
                    Nome = veiculoDTO.Nome,
                    Marca = veiculoDTO.Marca,
                    Ano = veiculoDTO.Ano
                };
                veiculoServico.Incluir(veiculo);
                return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
            }).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" }).WithTags("Veiculos");


            endpoints.MapGet("/veiculos", ([FromQuery] int? pagina, [FromQuery] string? nome, [FromQuery] string? marca, [FromQuery] int? ano, IVeiculoServico veiculoServico) =>
            {
                var veiculos = veiculoServico.Todos(pagina, nome, marca, ano);
                return Results.Ok(veiculos);
            }).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" }).WithTags("Veiculos");


            endpoints.MapGet("/veiculo/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
            {
                var veiculo = veiculoServico.BuscarPorId(id);
                if (veiculo == null) return Results.NotFound();
                return Results.Ok(veiculo);

            }).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" }).WithTags("Veiculos");


            endpoints.MapPut("/veiculos/{id}", ([FromRoute] int id, [FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
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

            }).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Veiculos");


            endpoints.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
            {
                var veiculo = veiculoServico.BuscarPorId(id);
                if (veiculo == null) return Results.NotFound();

                veiculoServico.Apagar(veiculo);
                return Results.NoContent();

            }).RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Veiculos");

        });
    }

}