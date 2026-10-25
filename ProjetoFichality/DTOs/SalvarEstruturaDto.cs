using System.ComponentModel.DataAnnotations;

namespace ProjetoFichality.DTOs
{
    public class SalvarEstruturaDto
    {
        [Range(1, int.MaxValue)] public int FichaId { get; set; }
        [Required] public string EstruturaJson { get; set; } = "[]";
    }
}
