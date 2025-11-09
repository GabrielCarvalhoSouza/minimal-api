using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Interfaces;

namespace minimal_api.Test.Dominio.Mocks;

public class AdministradorServicoMock : IAdministradorServico
{
    private static List<Administrador> administradores = new List<Administrador>()
    {
        new Administrador()
        {
            Id = 1,
            Email = "administrador@teste.com",
            Senha = "1234",
            Perfil = "Adm"
        },
        new Administrador()
        {
            Id = 2,
            Email = "editor@teste.com",
            Senha = "1234",
            Perfil = "Editor"
        }
    };

    public Administrador? BuscarPorId(int id)
    {
        return administradores.Find(a => a.Id == id);
    }

    public void Incluir(Administrador administrador)
    {
        administrador.Id = administradores.Count() + 1;
        administradores.Add(administrador);
    }

    public Administrador? Login(LoginDTO loginDTO)
    {
        return administradores.Find(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha);
    }

    public List<Administrador> Todos(int? pagina)
    {
        return administradores;
    }
}