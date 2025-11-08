using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Interfaces;
using minimal_api.Infraestrutura.Db;

namespace minimal_api.Dominio.Servicos;

public class VeiculoServico : IVeiculoServico
{
    private readonly DbContexto _contexto;
    public VeiculoServico(DbContexto contexto)
    {
        _contexto = contexto;
    }

    public void Apagar(Veiculo veiculo)
    {
        _contexto.Veiculos.Remove(veiculo);
        _contexto.SaveChanges();
    }

    public void Atualizar(Veiculo veiculo)
    {
        _contexto.Veiculos.Update(veiculo);
        _contexto.SaveChanges();
    }

    public Veiculo? BuscarPorId(int id)
    {
        return _contexto.Veiculos.Where(v => v.Id == id).FirstOrDefault();
    }

    public void Incluir(Veiculo veiculo)
    {
        _contexto.Veiculos.Add(veiculo);
        _contexto.SaveChanges();
    }

    public List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null, int? ano = null)
    {
        var querry = _contexto.Veiculos.AsQueryable();
        if (!string.IsNullOrEmpty(nome))
        {
            querry = querry.Where(v => v.Nome == nome);
        }

        if (!string.IsNullOrEmpty(marca))
        {
            querry = querry.Where(v => v.Marca == marca);
        }

        if (ano != null)
        {
            querry = querry.Where(v => v.Ano == ano);
        }

        int itensPorPagina = 10;
        int paginaAtual = pagina ?? 1;

        querry = querry.Skip((paginaAtual - 1) * itensPorPagina).Take(itensPorPagina);

        return querry.ToList();
    }
}