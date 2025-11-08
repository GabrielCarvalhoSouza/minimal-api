using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;

namespace minimal_api.Dominio.Interfaces;

public interface IAdministradorServico
{
    Administrador? Login(LoginDTO loginDTO);
    void Incluir(Administrador administrador);
    Administrador? BuscarPorId(int id);
    List<Administrador> Todos(int? pagina);
}