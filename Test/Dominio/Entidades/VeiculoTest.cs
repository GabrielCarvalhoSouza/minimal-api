using minimal_api.Dominio.Entidades;

namespace Test.Dominio.Entidades;

[TestClass]
public class VeiculoTest
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        // Arrange
        var veiculo = new Veiculo();

        // Act
        veiculo.Id = 1;
        veiculo.Nome = "Fusca";
        veiculo.Marca = "VW";
        veiculo.Ano = 1969;

        // Assert
        Assert.AreEqual(1, veiculo.Id);
        Assert.AreEqual("Fusca", veiculo.Nome);
        Assert.AreEqual("VW", veiculo.Marca);
        Assert.AreEqual(1969, veiculo.Ano);
    }
}