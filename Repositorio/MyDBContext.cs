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
        public DbSet<Peca> Peca { get; set; }
        public DbSet<ProdutoPeca> ProdutoPeca { get; set; }
        public DbSet<PecaMateriaPrima> PecaMateriaPrima { get; set; }
        public DbSet<Venda> Venda { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<QRCode> QRCode { get; set; }

        // 👇 Novo DbSet para auditoria
        public DbSet<Auditoria> Auditoria { get; set; }
        public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }
        public DbSet<Estoque> Estoque { get; set; }
        // Adicione estas duas propriedades na classe MyDBContext:
        public DbSet<VendaItem> VendaItem { get; set; }
        public DbSet<VendaEmbalagem> VendaEmbalagem { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<MovimentacaoEstoque>()
                .HasIndex(x => x.DataHora);

            modelBuilder.Entity<Estoque>()
                .HasIndex(x => new { x.TipoItem, x.ItemId })
                .IsUnique();
            modelBuilder.Entity<Estoque>()
                .Property(x => x.Quantidade)
                .HasPrecision(18, 3);

            modelBuilder.Entity<QRCode>()
                .HasIndex(x => x.CodigoPublico)
                .IsUnique();

            modelBuilder.Entity<PecaMateriaPrima>()
                .HasIndex(x => new { x.PecaId, x.MateriaPrimaId }).IsUnique();
            modelBuilder.Entity<PecaMateriaPrima>()
                .HasOne(x => x.Peca).WithMany(x => x.MateriasPrimas)
                .HasForeignKey(x => x.PecaId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PecaMateriaPrima>()
                .HasOne(x => x.MateriaPrima).WithMany()
                .HasForeignKey(x => x.MateriaPrimaId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PecaMateriaPrima>().Property(x => x.Peso).IsRequired();

            modelBuilder.Entity<ProdutoPeca>()
                .HasIndex(x => new { x.ProdutoId, x.PecaId })
                .IsUnique();
            modelBuilder.Entity<ProdutoPeca>()
                .HasOne(x => x.Produto).WithMany(x => x.Composicao)
                .HasForeignKey(x => x.ProdutoId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ProdutoPeca>()
                .HasOne(x => x.Peca).WithMany()
                .HasForeignKey(x => x.PecaId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProdutoPeca>()
                .Property(x => x.Quantidade).IsRequired();

                // Dentro do método OnModelCreating(ModelBuilder modelBuilder):
                modelBuilder.Entity<Venda>()
                .HasMany(v => v.ItensVenda)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<Venda>()
                .HasMany(v => v.EmbalagensVenda)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
