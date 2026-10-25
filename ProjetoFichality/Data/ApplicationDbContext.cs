using Microsoft.EntityFrameworkCore;
using ProjetoFichality.Models;

namespace ProjetoFichality.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Ficha> Fichas { get; set; }
        public DbSet<SistemaRpg> SistemasRpg { get; set; }
        public DbSet<ModeloFicha> ModelosFicha { get; set; }
        public DbSet<Destaque> Destaques { get; set; }
        public DbSet<ItemSistemaRpg> ItensSistemaRpg { get; set; }
        public DbSet<Campanha> Campanhas { get; set; }
        public DbSet<CampanhaFicha> CampanhasFichas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<Ficha>()
                .Property(f => f.EstruturaJson).HasColumnType("nvarchar(max)");

            modelBuilder.Entity<Ficha>()
                .HasOne(f => f.Usuario).WithMany(u => u.Fichas)
                .HasForeignKey(f => f.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ficha>()
                .HasOne(f => f.Sistema).WithMany(s => s.Fichas)
                .HasForeignKey(f => f.SistemaRpgId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Ficha>()
                .HasOne(f => f.ModeloFicha).WithMany(m => m.Fichas)
                .HasForeignKey(f => f.ModeloFichaId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SistemaRpg>()
                .HasOne(s => s.CriadorUsuario).WithMany()
                .HasForeignKey(s => s.CriadorUsuarioId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SistemaRpg>()
                .HasOne(s => s.SistemaOrigem).WithMany(s => s.Copias)
                .HasForeignKey(s => s.SistemaOrigemId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ModeloFicha>()
                .Property(m => m.EstruturaJson).HasColumnType("nvarchar(max)");
            modelBuilder.Entity<ModeloFicha>()
                .HasOne(m => m.SistemaRpg).WithMany(s => s.Modelos)
                .HasForeignKey(m => m.SistemaRpgId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Destaque>()
                .HasOne(d => d.SistemaRpg).WithMany()
                .HasForeignKey(d => d.SistemaRpgId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Destaque>()
                .HasOne(d => d.ModeloFicha).WithMany()
                .HasForeignKey(d => d.ModeloFichaId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ItemSistemaRpg>()
                .Property(i => i.PropriedadesJson).HasColumnType("nvarchar(max)");
            modelBuilder.Entity<ItemSistemaRpg>()
                .Property(i => i.EfeitosJson).HasColumnType("nvarchar(max)");
            modelBuilder.Entity<ItemSistemaRpg>()
                .HasOne(i => i.SistemaRpg).WithMany(s => s.Itens)
                .HasForeignKey(i => i.SistemaRpgId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Campanha>()
                .Property(c => c.HouseRulesJson).HasColumnType("nvarchar(max)");
            modelBuilder.Entity<Campanha>()
                .HasOne(c => c.SistemaRpg).WithMany(s => s.Campanhas)
                .HasForeignKey(c => c.SistemaRpgId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Campanha>()
                .HasOne(c => c.MestreUsuario).WithMany()
                .HasForeignKey(c => c.MestreUsuarioId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CampanhaFicha>()
                .HasIndex(cf => new { cf.CampanhaId, cf.FichaId }).IsUnique();
            modelBuilder.Entity<CampanhaFicha>()
                .HasOne(cf => cf.Campanha).WithMany(c => c.Participantes)
                .HasForeignKey(cf => cf.CampanhaId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CampanhaFicha>()
                .HasOne(cf => cf.Ficha).WithMany(f => f.Campanhas)
                .HasForeignKey(cf => cf.FichaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
