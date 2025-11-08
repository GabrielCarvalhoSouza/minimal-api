using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Servicos;
using minimal_api.Infraestrutura.Db;

namespace Test.Dominio.Entidades;

[TestClass]
public class VeiculoSevicoTest
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
    public void TestandoSalvarVeiculo()
    {
        // Arrange
        var veiculo = new Veiculo();
        veiculo.Nome = "Fusca";
        veiculo.Marca = "VW";
        veiculo.Ano = 1969;

        var contexto = CriarContextoDeTeste();
        var veiculoServico = new VeiculoServico(contexto);

        // Act
        veiculoServico.Incluir(veiculo);

        // Assert
        var listaDoBanco = veiculoServico.Todos();
        var ultimoVeiculo = listaDoBanco.Last();

        Assert.AreEqual(1, listaDoBanco.Count());
        Assert.AreEqual("Fusca", ultimoVeiculo.Nome);
        Assert.AreEqual("VW", ultimoVeiculo.Marca);
        Assert.AreEqual(1969, ultimoVeiculo.Ano);

    }

    [TestMethod]
    public void TestandoBuscaPorId()
    {
        // Arrange
        var veiculo = new Veiculo();
        veiculo.Nome = "Fusca";
        veiculo.Marca = "VW";
        veiculo.Ano = 1969;

        var contexto = CriarContextoDeTeste();
        var veiculoServico = new VeiculoServico(contexto);

        // Act
        veiculoServico.Incluir(veiculo);
        var veiculoBuscado = veiculoServico.BuscarPorId(veiculo.Id);

        Assert.IsNotNull(veiculoBuscado);
        Assert.AreEqual(veiculo.Id, veiculoBuscado.Id);
        Assert.AreEqual(veiculo.Nome, veiculoBuscado.Nome);
        Assert.AreEqual(veiculo.Marca, veiculoBuscado.Marca);
        Assert.AreEqual(veiculo.Ano, veiculoBuscado.Ano);
    }
}