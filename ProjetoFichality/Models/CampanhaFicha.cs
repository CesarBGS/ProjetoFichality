namespace ProjetoFichality.Models
{
    // Relaciona uma ficha de personagem com uma campanha.
    public class CampanhaFicha
    {
        public int Id { get; set; }
        public int CampanhaId { get; set; }
        public Campanha? Campanha { get; set; }
        public int FichaId { get; set; }
        public Ficha? Ficha { get; set; }
        public DateTime AdicionadaEm { get; set; } = DateTime.UtcNow;
    }
}
