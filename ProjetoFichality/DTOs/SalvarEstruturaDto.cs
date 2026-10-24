using System.ComponentModel.DataAnnotations;

namespace ProjetoFichality.DTOs;

public class SalvarEstruturaDto
{
    [Range(1, int.MaxValue, ErrorMessage = "O ID da ficha é inválido.")]
    public int FichaId { get; set; }

    [Required(ErrorMessage = "A estrutura da ficha é obrigatória.")]
    public string EstruturaJson { get; set; } = string.Empty;
}
