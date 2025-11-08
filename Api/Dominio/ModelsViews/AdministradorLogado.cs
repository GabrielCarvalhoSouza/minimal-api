using minimal_api.Dominio.Enuns;

namespace minimal_api.Dominio.ModelsViews
{
    public record AdministradorLogado
    {
        public string Email { get; set; } = default!;
        public string Perfil { get; set; } = default!;
        public string Token { get; set; } = default!;
    }
}