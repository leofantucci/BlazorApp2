using Microsoft.EntityFrameworkCore;
using TechToysDominio;

namespace Repositorio
{
    public class MyDBContext : DbContext
    {
        public MyDBContext(DbContextOptions<MyDBContext> options) : base(options)
        {
        }

        public DbSet<Cliente> Cliente { get; set; }
        public DbSet<Fornecedor> Fornecedor { get; set; }
        public DbSet<Impressora> Impressora { get; set; }
        public DbSet<MateriaPrima> MateriaPrima { get; set; }
        public DbSet<Embalagem> Embalagem { get; set; }
        public DbSet<Produto> Produto { get; set; }
        public DbSet<Venda> Venda { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configurações adicionais de mapeamento se houver
        }
    }
}