using System.Net;
using System.Text;
using System.Text.Json;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.ModelsViews;
using minimal_api.Test.Dominio.Helpers;


namespace Test.Dominio.Requests;

[TestClass]
public class AdministradorRequestTest
{
    [ClassInitialize]
    public static void ClassInit(TestContext testContext)
    {
        Setup.ClassInit(testContext);
    }

    [ClassCleanup]
    public static void ClassCleanUp()
    {
        Setup.ClassCleanUp();
    }

    [TestMethod]
    public async Task TestarLogin()
    {
        // Arrange
        var loginDTO = new LoginDTO()
        {
            Email = "administrador@teste.com",
            Senha = "1234"
        };

        var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "application/json");

        // Act
        var response = await Setup.client.PostAsync("/administradores/login", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();
        var admLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.IsNotNull(admLogado?.Email ?? "");
        Assert.IsNotNull(admLogado?.Perfil ?? "");
        Assert.IsNotNull(admLogado?.Token ?? "");
    }
}