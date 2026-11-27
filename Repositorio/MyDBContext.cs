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
        public DbSet<PlacaNFC> PlacaNFC { get; set; }
        public DbSet<PlacaNFCCard> PlacaNFCCard { get; set; }

        // 👇 Novo DbSet para auditoria
        public DbSet<Auditoria> Auditoria { get; set; }
        public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }
        public DbSet<Estoque> Estoque { get; set; }
        // Adicione estas duas propriedades na classe MyDBContext:
        public DbSet<VendaItem> VendaItem { get; set; }
        public DbSet<VendaEmbalagem> VendaEmbalagem { get; set; }
        public DbSet<ProdutoComercial> ProdutoComercial { get; set; }
        public DbSet<VendaComercial> VendaComercial { get; set; }
        public DbSet<VendaComercialItem> VendaComercialItem { get; set; }
        public DbSet<Pagamento> Pagamento { get; set; }
        public DbSet<ControleComercial> ControleComercial { get; set; }
        public DbSet<AsaasWebhookEvento> AsaasWebhookEventos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.Property(x => x.Nome).HasMaxLength(100).IsRequired();
                entity.Property(x => x.NomeFantasia).HasMaxLength(100);
                entity.Property(x => x.TipoCliente).HasConversion<string>().HasMaxLength(2).IsRequired();
                entity.Property(x => x.CPF).HasMaxLength(14);
                entity.Property(x => x.CNPJ).HasMaxLength(18);
                entity.Property(x => x.Endereco).HasMaxLength(200);
                entity.Property(x => x.Numero).HasMaxLength(12);
                entity.Property(x => x.Complemento).HasMaxLength(100);
                entity.Property(x => x.Bairro).HasMaxLength(100);
                entity.Property(x => x.Cidade).HasMaxLength(100);
                entity.Property(x => x.Estado).HasMaxLength(2);
                entity.Property(x => x.CEP).HasMaxLength(20);
                entity.Property(x => x.Telefone).HasMaxLength(20);
                entity.Property(x => x.Email).HasMaxLength(150);
                entity.Property(x => x.AsaasCustomerId).HasMaxLength(80);
            });

            modelBuilder.Entity<Usuario>()
                .HasDiscriminator<string>("TipoUsuario")
                .HasValue<Usuario>("Interno")
                .HasValue<UsuarioComercial>("Comercial");

            modelBuilder.Entity<UsuarioComercial>(entity =>
            {
                entity.HasIndex(x => x.Login).IsUnique().HasFilter("\"TipoUsuario\" = 'Comercial'");
                entity.HasOne(x => x.Cliente).WithOne()
                    .HasForeignKey<UsuarioComercial>(x => x.ClienteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProdutoComercial>(entity =>
            {
                entity.Property(x => x.Nome).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Foto).HasMaxLength(500);
                entity.Property(x => x.Descricao).HasMaxLength(2000);
                entity.Property(x => x.Preco).HasPrecision(18, 2);
                entity.HasIndex(x => x.Disponivel);
                entity.HasOne(x => x.ProdutoBase).WithMany()
                    .HasForeignKey(x => x.ProdutoBaseId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<VendaComercial>(entity =>
            {
                entity.Property(x => x.Status).HasConversion<int>();
                entity.Property(x => x.Subtotal).HasPrecision(18, 2);
                entity.Property(x => x.Frete).HasPrecision(18, 2);
                entity.Property(x => x.Desconto).HasPrecision(18, 2);
                entity.Property(x => x.Total).HasPrecision(18, 2);
                entity.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(x => x.Data_Hora);
                entity.HasIndex(x => x.Status);
            });

            modelBuilder.Entity<VendaComercialItem>(entity =>
            {
                entity.Property(x => x.PrecoUnitario).HasPrecision(18, 2);
                entity.Property(x => x.Subtotal).HasPrecision(18, 2);
                entity.HasOne(x => x.VendaComercial).WithMany(x => x.ItensVenda)
                    .HasForeignKey(x => x.VendaComercialId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.ProdutoComercial).WithMany()
                    .HasForeignKey(x => x.ProdutoComercialId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Pagamento).WithMany()
                    .HasForeignKey(x => x.PagamentoId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Pagamento>(entity =>
            {
                entity.Property(x => x.Valor).HasPrecision(18, 2);
                entity.Property(x => x.Status).HasConversion<int>();
                entity.Property(x => x.Metodo).HasMaxLength(40);
                entity.Property(x => x.Provedor).HasMaxLength(80);
                entity.Property(x => x.TransacaoExternaId).HasMaxLength(150);
                entity.Property(x => x.StatusProvedor).HasMaxLength(80);
                entity.Property(x => x.PixQrCodeBase64).HasMaxLength(12000);
                entity.Property(x => x.PixCopiaECola).HasMaxLength(2000);
                entity.HasOne(x => x.VendaComercial).WithMany(x => x.Pagamentos)
                    .HasForeignKey(x => x.VendaComercialId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(x => x.TransacaoExternaId).IsUnique()
                    .HasFilter("\"TransacaoExternaId\" IS NOT NULL");
            });

            modelBuilder.Entity<AsaasWebhookEvento>(entity =>
            {
                entity.HasIndex(x => x.EventoExternoId).IsUnique();
                entity.Property(x => x.EventoExternoId).HasMaxLength(150).IsRequired();
                entity.Property(x => x.TipoEvento).HasMaxLength(100).IsRequired();
                entity.Property(x => x.PagamentoExternoId).HasMaxLength(150);
            });

            modelBuilder.Entity<ControleComercial>(entity =>
            {
                entity.Property(x => x.Status).HasConversion<int>();
                entity.Property(x => x.Observacao).HasMaxLength(500);
                entity.Property(x => x.Responsavel).HasMaxLength(100);
                entity.HasOne(x => x.Venda).WithMany(x => x.Historico)
                    .HasForeignKey(x => x.VendaComercialId).OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(x => new { x.VendaComercialId, x.DataHoraUtc });
            });

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

            modelBuilder.Entity<PlacaNFC>(entity =>
            {
                entity.HasIndex(x => x.CodigoPublico).IsUnique();
                entity.Property(x => x.Nome).HasMaxLength(120);
                entity.HasOne(x => x.Cliente).WithMany()
                    .HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.Produto).WithMany()
                    .HasForeignKey(x => x.ProdutoId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.QRCode).WithMany()
                    .HasForeignKey(x => x.QRCodeId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.VendaComercialItem).WithMany()
                    .HasForeignKey(x => x.VendaComercialItemId).OnDelete(DeleteBehavior.SetNull);
                entity.HasMany(x => x.Cards).WithOne(x => x.PlacaNFC)
                    .HasForeignKey(x => x.PlacaNFCId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PlacaNFCCard>(entity =>
            {
                entity.Property(x => x.Nome).HasMaxLength(80).IsRequired();
                entity.Property(x => x.Descricao).HasMaxLength(160);
                entity.Property(x => x.Link).HasMaxLength(500);
                entity.Property(x => x.Icone).HasMaxLength(500);
                entity.HasIndex(x => new { x.PlacaNFCId, x.Ordem });
            });

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
