using minimal_api.Dominio.Enuns;

namespace minimal_api.Dominio.ModelsViews
{
    public record AdministradoresModelView
    {
        public int Id { get; set; }
        public string Email { get; set; } = default!;
        public string Perfil { get; set; } = default!;
    }
}