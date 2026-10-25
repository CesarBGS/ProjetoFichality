using System.ComponentModel.DataAnnotations;

namespace ProjetoFichality.DTOs
{
    public class CriarFichaDto
    {
        [Required, StringLength(100, MinimumLength = 1)]
        public string NomePersonagem { get; set; } = string.Empty;
        // Usado para compatibilidade com a interface antiga; não define a autoria.
        public int UsuarioId { get; set; }
        [StringLength(100)]
        public string SistemaRpg { get; set; } = string.Empty;
        public int? SistemaRpgId { get; set; }
        public int? ModeloFichaId { get; set; }
    }
}
