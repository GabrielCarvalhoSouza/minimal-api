using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Servicos;
using minimal_api.Infraestrutura.Db;

namespace Test.Dominio.Entidades;

[TestClass]
public class AdministradorSevicoTest
{
    private DbContexto CriarContextoDeTeste()
    {
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var path = Path.GetFullPath(Path.Combine(assemblyPath ?? string.Empty, "", "..", "..", ".."));

        var builder = new ConfigurationBuilder()
            .SetBasePath(path ?? Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        var connectionString = configuration.GetConnectionString("mysql");

        var optionsBuilder = new DbContextOptionsBuilder<DbContexto>();

        optionsBuilder.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString)
        );

        var contexto = new DbContexto(optionsBuilder.Options);

        contexto.Database.EnsureDeleted();
        contexto.Database.EnsureCreated();

        return contexto;
    }


    [TestMethod]
    public void TestandoSalvarAdministrador()
    {
        // Arrange
        var adm = new Administrador();
        adm.Email = "teste@gmail.com";
        adm.Senha = "123456";
        adm.Perfil = "Adm";

        var contexto = CriarContextoDeTeste();
        var administradorServico = new AdministradorServico(contexto);

        // Act
        administradorServico.Incluir(adm);

        // Assert
        var listaDoBanco = administradorServico.Todos();
        var ultimoAdm = listaDoBanco.Last();

        Assert.AreEqual(2, listaDoBanco.Count());
        Assert.AreEqual("teste@gmail.com", ultimoAdm.Email);
        Assert.AreEqual("123456", ultimoAdm.Senha);
        Assert.AreEqual("Adm", ultimoAdm.Perfil);

    }

    [TestMethod]
    public void TestandoBuscaPorId()
    {
        // Arrange
        var adm = new Administrador();
        adm.Email = "teste@gmail.com";
        adm.Senha = "123456";
        adm.Perfil = "Adm";

        var contexto = CriarContextoDeTeste();
        var administradorServico = new AdministradorServico(contexto);

        // Act
        administradorServico.Incluir(adm);
        var admBuscado = administradorServico.BuscarPorId(adm.Id);
        
        // Assert
        Assert.IsNotNull(admBuscado);
        Assert.AreEqual(adm.Id, admBuscado.Id);
        Assert.AreEqual(adm.Email, admBuscado.Email);
        Assert.AreEqual(adm.Senha, admBuscado.Senha);
        Assert.AreEqual(adm.Perfil, admBuscado.Perfil);
    }
}