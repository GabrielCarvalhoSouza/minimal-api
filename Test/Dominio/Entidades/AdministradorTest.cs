using minimal_api.Dominio.Entidades;

namespace Test.Dominio.Entidades;

[TestClass]
public class AdministradorTest
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        // Arrange
        var adm = new Administrador();

        // Act
        adm.Id = 1;
        adm.Email = "teste@teste.com";
        adm.Senha = "1234";
        adm.Perfil = "Adm";

        // Assert
        Assert.AreEqual(1, adm.Id);
        Assert.AreEqual("teste@teste.com", adm.Email);
        Assert.AreEqual("1234", adm.Senha);
        Assert.AreEqual("Adm", adm.Perfil);
    }
}