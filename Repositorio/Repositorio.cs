using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TechToysDominio;

namespace Repositorio
{
    public class ItemQuantidadeInput
    {
        public int ItemId { get; set; }
        public int Quantidade { get; set; } = 1;
    }

    public class MateriaPrimaPesoInput
    {
        public int MateriaPrimaId { get; set; }
        public double Peso { get; set; }
    }

    public class RepositorioTechToys
    {
        private readonly IDbContextFactory<MyDBContext> _contextFactory;
        private readonly UsuarioAtualService _usuarioAtual;

        private void ExigirPapel(params string[] papeisPermitidos)
        {
            var papel = _usuarioAtual.ObterPapelAsync().GetAwaiter().GetResult();
            if (string.IsNullOrWhiteSpace(papel) || !papeisPermitidos.Contains(papel, StringComparer.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Você não possui permissão para executar esta operação.");
        }

        public RepositorioTechToys(IDbContextFactory<MyDBContext> contextFactory, UsuarioAtualService usuarioAtual)
        {
            _contextFactory = contextFactory;
            _usuarioAtual = usuarioAtual;
        }

        public void CriarBanco()
        {
            using var db = _contextFactory.CreateDbContext();

            // Recupera bancos cujo esquema já foi criado, mas cuja tabela de histórico
            // do EF ficou vazia. Só registra a migração inicial quando TODAS as tabelas
            // dela existem; assim não apaga dados nem mascara um esquema incompleto.
            RegistrarMigracaoInicialSeEsquemaJaExiste(db);

            db.Database.Migrate();
            MigrarSenhasLegadas(db);
        }

        private static void RegistrarMigracaoInicialSeEsquemaJaExiste(MyDBContext db)
        {
            var tabelasDaMigracaoInicial = new[]
            {
                "Auditoria", "Cliente", "Estoque", "Fornecedor", "Impressora",
                "MovimentacoesEstoque", "QRCode", "Usuario", "Venda", "Embalagem",
                "MateriaPrima", "VendaEmbalagem", "Peca", "Produto", "ProdutoPeca",
                "VendaItem"
            };

            var conexao = db.Database.GetDbConnection();
            var abriuAqui = conexao.State != System.Data.ConnectionState.Open;
            if (abriuAqui)
                conexao.Open();

            try
            {
                using var comando = conexao.CreateCommand();
                comando.CommandText = @"
                    SELECT
                        to_regclass('""__EFMigrationsHistory""') IS NOT NULL,
                        (SELECT COUNT(*)
                         FROM information_schema.tables
                         WHERE table_schema = current_schema()
                           AND table_type = 'BASE TABLE'
                           AND table_name = ANY (@tabelas));";

                var parametro = comando.CreateParameter();
                parametro.ParameterName = "tabelas";
                parametro.Value = tabelasDaMigracaoInicial;
                comando.Parameters.Add(parametro);

                using var leitor = comando.ExecuteReader();
                leitor.Read();
                var historicoExiste = leitor.GetBoolean(0);
                var quantidadeTabelas = leitor.GetInt32(1);
                leitor.Close();

                if (quantidadeTabelas != tabelasDaMigracaoInicial.Length)
                    return;

                if (historicoExiste)
                {
                    comando.CommandText = @"SELECT COUNT(*) FROM ""__EFMigrationsHistory"";";
                    comando.Parameters.Clear();
                    if (Convert.ToInt32(comando.ExecuteScalar()) != 0)
                        return;
                }
                else
                {
                    comando.CommandText = @"
                        CREATE TABLE ""__EFMigrationsHistory"" (
                            ""MigrationId"" character varying(150) NOT NULL,
                            ""ProductVersion"" character varying(32) NOT NULL,
                            CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
                        );";
                    comando.Parameters.Clear();
                    comando.ExecuteNonQuery();
                }

                comando.CommandText = @"
                    INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
                    VALUES ('20261116212043_InitialCreate', '10.0.12')
                    ON CONFLICT (""MigrationId"") DO NOTHING;";
                comando.Parameters.Clear();
                comando.ExecuteNonQuery();

                // Também reconhece a segunda migração caso a tabela e seus índices
                // já tenham sido criados anteriormente.
                comando.CommandText = @"
                    SELECT
                        to_regclass('""PecaMateriaPrima""') IS NOT NULL
                        AND EXISTS (SELECT 1 FROM pg_indexes
                                    WHERE schemaname = current_schema()
                                      AND indexname = 'IX_PecaMateriaPrima_MateriaPrimaId')
                        AND EXISTS (SELECT 1 FROM pg_indexes
                                    WHERE schemaname = current_schema()
                                      AND indexname = 'IX_PecaMateriaPrima_PecaId_MateriaPrimaId');";
                var segundaMigracaoJaExiste = Convert.ToBoolean(comando.ExecuteScalar());
                if (segundaMigracaoJaExiste)
                {
                    comando.CommandText = @"
                        INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
                        VALUES ('20261116220000_PecaMultiplasMateriasPrimas', '10.0.12')
                        ON CONFLICT (""MigrationId"") DO NOTHING;";
                    comando.ExecuteNonQuery();
                }
            }
            finally
            {
                if (abriuAqui)
                    conexao.Close();
            }
        }

        private static void MigrarSenhasLegadas(MyDBContext db)
        {
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Usuario>();
            var alterou = false;

            foreach (var usuario in db.Usuario.Where(u => u.SenhaHash == null && u.Senha != string.Empty))
            {
                bool pareceSha256 = false;
                try
                {
                    pareceSha256 = Convert.FromBase64String(usuario.Senha).Length == 32;
                }
                catch (FormatException) { }

                if (pareceSha256) continue;

                usuario.SenhaHash = hasher.HashPassword(usuario, usuario.Senha);
                usuario.Senha = string.Empty;
                alterou = true;
            }

            if (alterou)
                db.SaveChanges();
        }

        public void LimparBanco()
        {
            ExigirPapel("admin");
            using var db = _contextFactory.CreateDbContext();
            db.Auditoria.Add(new Auditoria
            {
                Entidade = "Banco de dados",
                EntidadeId = 0,
                Operacao = "Limpeza dos dados operacionais",
                Usuario = _usuarioAtual.ObterUsuarioAsync().GetAwaiter().GetResult(),
                             DataHora = DateTime.UtcNow
            });
            db.SaveChanges();

            db.Database.ExecuteSqlRaw(@"
            TRUNCATE TABLE
            ""VendaEmbalagem"", ""VendaItem"", ""Venda"", ""MovimentacoesEstoque"", ""Estoque"", ""QRCode"", ""ProdutoPeca"", ""Produto"", ""Peca"", ""Embalagem"", ""MateriaPrima"", ""Cliente"", ""Impressora"", ""Fornecedor""
            RESTART IDENTITY CASCADE;");
        }

        public void LimparBanco(string tabela)
        {
            ExigirPapel("admin");
            var tabelasPermitidas = new HashSet<string>(StringComparer.Ordinal)
            {
                "VendaEmbalagem", "VendaItem", "Venda", "MovimentacoesEstoque", "Estoque", "QRCode", "ProdutoPeca", "Produto", "Peca", "Embalagem", "MateriaPrima",
                "Cliente", "Impressora", "Fornecedor"
            };

            if (!tabelasPermitidas.Contains(tabela))
                throw new ArgumentException("Tabela inválida.", nameof(tabela));

            using var db = _contextFactory.CreateDbContext();
            db.Auditoria.Add(new Auditoria
            {
                Entidade = tabela,
                EntidadeId = 0,
                Operacao = "Limpeza de tabela",
                Usuario = _usuarioAtual.ObterUsuarioAsync().GetAwaiter().GetResult(),
                             DataHora = DateTime.UtcNow
            });
            db.SaveChanges();
            #pragma warning disable EF1003
            db.Database.ExecuteSqlRaw("TRUNCATE TABLE \"" + tabela + "\" RESTART IDENTITY CASCADE;");
            #pragma warning restore EF1003
            if (tabela is "Produto" or "Peca" or "MateriaPrima" or "Embalagem")
                db.Database.ExecuteSqlInterpolated($"DELETE FROM \"Estoque\" WHERE \"TipoItem\" = {tabela}");
        }

        #region Funções Globais (CRUD Genérico)

        public T? ObterPorId<T>(int id) where T : class
        {
            if (typeof(T) == typeof(Usuario) || typeof(T) == typeof(Auditoria))
                ExigirPapel("admin");
            using var db = _contextFactory.CreateDbContext();

            if (typeof(T) == typeof(MateriaPrima))
            {
                return db.MateriaPrima
                .AsNoTracking()
                .Include(x => x.Fornecedor)
                .FirstOrDefault(x => x.Id == id) as T;
            }

            if (typeof(T) == typeof(Produto))
            {
                return db.Produto
                .AsNoTracking()
                .Include(x => x.Materia_Prima).Include(x => x.Impressora)
                .Include(x => x.Composicao).ThenInclude(x => x.Peca).ThenInclude(x => x.MateriasPrimas).ThenInclude(x => x.MateriaPrima)
                .Include(x => x.Composicao).ThenInclude(x => x.Peca).ThenInclude(x => x.Impressora)
                .FirstOrDefault(x => x.Id == id) as T;
            }

            if (typeof(T) == typeof(Peca))
            {
                return db.Peca.AsNoTracking().Include(x => x.Materia_Prima).Include(x => x.Impressora).Include(x => x.MateriasPrimas).ThenInclude(x => x.MateriaPrima).FirstOrDefault(x => x.Id == id) as T;
            }

            if (typeof(T) == typeof(Embalagem))
            {
                return db.Embalagem
                .AsNoTracking()
                .Include(x => x.Fornecedor)
                .FirstOrDefault(x => x.Id == id) as T;
            }

            if (typeof(T) == typeof(Venda))
            {
                return db.Venda
                .AsNoTracking()
                .Include(x => x.Cliente)
                .Include(x => x.ItensVenda).ThenInclude(i => i.Produto).ThenInclude(p => p.Materia_Prima)
                .Include(x => x.ItensVenda).ThenInclude(i => i.Produto).ThenInclude(p => p.Impressora)
                .Include(x => x.ItensVenda).ThenInclude(i => i.Produto).ThenInclude(p => p.Composicao).ThenInclude(cp => cp.Peca).ThenInclude(peca => peca.MateriasPrimas).ThenInclude(mp => mp.MateriaPrima)
                .Include(x => x.ItensVenda).ThenInclude(i => i.Produto).ThenInclude(p => p.Composicao).ThenInclude(cp => cp.Peca).ThenInclude(peca => peca.Impressora)
                .Include(x => x.EmbalagensVenda)
                .ThenInclude(e => e.Embalagem)
                .FirstOrDefault(x => x.Id == id) as T;
            }

            return db.Set<T>().Find(id);
        }

        public List<T> ListarTodos<T>() where T : class
        {
            if (typeof(T) == typeof(Usuario) || typeof(T) == typeof(Auditoria))
                ExigirPapel("admin");
            using var db = _contextFactory.CreateDbContext();

            if (typeof(T) == typeof(MateriaPrima))
            {
                return db.MateriaPrima
                .AsNoTracking()
                .Include(x => x.Fornecedor)
                .ToList() as List<T> ?? new List<T>();
            }

            if (typeof(T) == typeof(Produto))
            {
                return db.Produto
                .AsNoTracking()
                .Include(x => x.Materia_Prima).Include(x => x.Impressora)
                .Include(x => x.Composicao).ThenInclude(x => x.Peca).ThenInclude(x => x.MateriasPrimas).ThenInclude(x => x.MateriaPrima)
                .Include(x => x.Composicao).ThenInclude(x => x.Peca).ThenInclude(x => x.Impressora)
                .ToList() as List<T> ?? new List<T>();
            }

            if (typeof(T) == typeof(Peca))
            {
                return db.Peca.AsNoTracking().Include(x => x.Materia_Prima).Include(x => x.Impressora).Include(x => x.MateriasPrimas).ThenInclude(x => x.MateriaPrima).ToList() as List<T> ?? new List<T>();
            }

            if (typeof(T) == typeof(Embalagem))
            {
                return db.Embalagem
                .AsNoTracking()
                .Include(x => x.Fornecedor)
                .ToList() as List<T> ?? new List<T>();
            }

            if (typeof(T) == typeof(Venda))
            {
                return db.Venda
                .AsNoTracking()
                .Include(x => x.Cliente)
                .Include(x => x.ItensVenda).ThenInclude(i => i.Produto).ThenInclude(p => p.Materia_Prima)
                .Include(x => x.ItensVenda).ThenInclude(i => i.Produto).ThenInclude(p => p.Impressora)
                .Include(x => x.ItensVenda).ThenInclude(i => i.Produto).ThenInclude(p => p.Composicao).ThenInclude(cp => cp.Peca).ThenInclude(peca => peca.MateriasPrimas).ThenInclude(mp => mp.MateriaPrima)
                .Include(x => x.ItensVenda).ThenInclude(i => i.Produto).ThenInclude(p => p.Composicao).ThenInclude(cp => cp.Peca).ThenInclude(peca => peca.Impressora)
                .Include(x => x.EmbalagensVenda)
                .ThenInclude(e => e.Embalagem)
                .ToList() as List<T> ?? new List<T>();
            }

            return db.Set<T>().AsNoTracking().ToList();
        }

        public void Atualizar<T>(T entidade) where T : class
        {
            ExigirPapel("admin", "gerente");
            if (entidade == null) throw new ArgumentNullException(nameof(entidade));
            if (typeof(T) == typeof(Usuario) || typeof(T) == typeof(Auditoria) || typeof(T) == typeof(QRCode))
                throw new InvalidOperationException("Este tipo de registro não pode ser alterado pelo CRUD genérico.");
            if (typeof(T) == typeof(Venda))
                throw new InvalidOperationException("Vendas devem ser alteradas pelo fluxo transacional de vendas.");

            using var db = _contextFactory.CreateDbContext();

            // Marca somente a entidade principal como modificada. DbSet.Update()
            // percorre todo o grafo de navegações e pode tentar anexar duas
            // instâncias diferentes da mesma entidade relacionada (por exemplo,
            // a mesma Impressora em Produto.Impressora e em Produto.Composicao).
            db.Entry(entidade).State = EntityState.Modified;

            // Preserva relacionamentos de referência selecionados nos formulários,
            // sem anexar coleções inteiras ou seus grafos aninhados.
            var metadados = db.Entry(entidade).Metadata;
            foreach (var navegacao in metadados.GetNavigations().Where(n => !n.IsCollection))
            {
                var relacionada = navegacao.PropertyInfo?.GetValue(entidade);
                if (relacionada == null)
                    continue;

                var entradaRelacionada = db.Entry(relacionada);
                if (entradaRelacionada.State == EntityState.Detached)
                    entradaRelacionada.State = EntityState.Unchanged;
            }

            db.SaveChanges();
        }

        public bool DeletarPorId<T>(int id) where T : class
        {
            ExigirPapel("admin", "gerente");
            if (typeof(T) == typeof(Usuario) || typeof(T) == typeof(Auditoria) || typeof(T) == typeof(QRCode))
                throw new InvalidOperationException("Este tipo de registro não pode ser removido pelo CRUD genérico.");
            if (typeof(T) == typeof(Venda))
                throw new InvalidOperationException("Vendas devem ser canceladas pelo fluxo que restaura o estoque.");

            using var db = _contextFactory.CreateDbContext();
            var item = db.Set<T>().Find(id);
            if (item == null) return false;

            if (item is Item itemEstoque && ObterQuantidadeEstoque(db, itemEstoque.GetType().Name, itemEstoque.Id) > 0)
                throw new InvalidOperationException("Não é possível excluir um item com estoque disponível. Faça a baixa registrada do saldo antes.");

            // Impede apagar cadastros que ainda são utilizados por outros registros.
            // Assim o usuário recebe uma orientação clara em vez de uma falha de FK do banco.
            if (item is Peca pecaVinculada && db.ProdutoPeca.Any(x => x.PecaId == pecaVinculada.Id))
                throw new InvalidOperationException("Não é possível excluir esta peça porque ela faz parte da composição de um ou mais produtos.");

            if (item is MateriaPrima materiaVinculada &&
                (db.PecaMateriaPrima.Any(x => x.MateriaPrimaId == materiaVinculada.Id) ||
                 db.Peca.Any(x => x.Materia_Prima != null && x.Materia_Prima.Id == materiaVinculada.Id) ||
                 db.Produto.Any(x => x.Materia_Prima != null && x.Materia_Prima.Id == materiaVinculada.Id)))
                throw new InvalidOperationException("Não é possível excluir esta matéria-prima porque ela está vinculada a uma peça ou produto.");

            if (item is Impressora impressoraVinculada &&
                (db.Peca.Any(x => x.Impressora != null && x.Impressora.Id == impressoraVinculada.Id) ||
                 db.Produto.Any(x => x.Impressora != null && x.Impressora.Id == impressoraVinculada.Id)))
                throw new InvalidOperationException("Não é possível excluir esta impressora porque ela está vinculada a uma peça ou produto.");

            if (item is Fornecedor fornecedorVinculado &&
                (db.MateriaPrima.Any(x => x.Fornecedor != null && x.Fornecedor.Id == fornecedorVinculado.Id) ||
                 db.Embalagem.Any(x => x.Fornecedor != null && x.Fornecedor.Id == fornecedorVinculado.Id)))
                throw new InvalidOperationException("Não é possível excluir este fornecedor porque ele está vinculado a uma matéria-prima ou embalagem.");

            if (item is Cliente clienteVinculado &&
                db.Venda.Any(x => x.Cliente != null && x.Cliente.Id == clienteVinculado.Id))
                throw new InvalidOperationException("Não é possível excluir este cliente porque existem vendas vinculadas a ele.");

            if (item is Produto produtoVinculado &&
                db.VendaItem.Any(x => x.ProdutoId == produtoVinculado.Id))
                throw new InvalidOperationException("Não é possível excluir este produto porque ele aparece no histórico de vendas.");

            if (item is Embalagem embalagemVinculada &&
                db.VendaEmbalagem.Any(x => x.EmbalagemId == embalagemVinculada.Id))
                throw new InvalidOperationException("Não é possível excluir esta embalagem porque ela aparece no histórico de vendas.");

            if (item is Item itemComEstoque)
            {
                var registroEstoque = db.Estoque.FirstOrDefault(x => x.TipoItem == itemComEstoque.GetType().Name && x.ItemId == itemComEstoque.Id);
                if (registroEstoque != null) db.Estoque.Remove(registroEstoque);
            }
            db.Set<T>().Remove(item);
            db.SaveChanges();
            return true;
        }

        #endregion

        private static decimal ObterQuantidadeEstoque(MyDBContext db, string tipoItem, int itemId)
        {
            return db.Estoque.Where(x => x.TipoItem == tipoItem && x.ItemId == itemId)
                .Select(x => (decimal?)x.Quantidade).FirstOrDefault() ?? 0m;
        }

        public decimal ObterQuantidadeEstoque<T>(int itemId) where T : class
        {
            using var db = _contextFactory.CreateDbContext();
            return ObterQuantidadeEstoque(db, typeof(T).Name, itemId);
        }

        private static Estoque ObterOuCriarEstoque(MyDBContext db, Item item)
        {
            var tipo = item.GetType().Name;

            // Reutiliza primeiro qualquer saldo já acompanhado pelo contexto.
            var estoqueLocal = db.Estoque.Local.FirstOrDefault(x =>
                x.TipoItem == tipo && x.ItemId == item.Id &&
                db.Entry(x).State != EntityState.Deleted);
            if (estoqueLocal != null) return estoqueLocal;

            // Cria o saldo de forma atômica. O índice único (TipoItem, ItemId)
            // também protege contra registros criados simultaneamente por outra
            // operação/aba: ON CONFLICT evita tentar inserir uma segunda linha.
            db.Database.ExecuteSqlInterpolated($"""
                INSERT INTO "Estoque" ("TipoItem", "ItemId", "Quantidade", "Unidade_Medida")
                VALUES ({tipo}, {item.Id}, 0, {item.Unidade_Medida})
                ON CONFLICT ("TipoItem", "ItemId") DO NOTHING;
                """);

            return db.Estoque.Single(x => x.TipoItem == tipo && x.ItemId == item.Id);
        }

        #region Funções de Cadastro Específicas

        public Cliente CadastrarCliente(string nome, string telefone, string email, string instagram)
        {
            ExigirPapel("admin", "gerente");
            using var db = _contextFactory.CreateDbContext();
            var cli = new Cliente { Nome = nome, Telefone = telefone, Email = email, Instagram = instagram };
            db.Cliente.Add(cli);
            db.SaveChanges();
            return cli;
        }

        public Fornecedor CadastrarFornecedor(string nome, string email, string telefone, string site)
        {
            ExigirPapel("admin", "gerente");
            using var db = _contextFactory.CreateDbContext();
            var forn = new Fornecedor { Nome = nome, Email = email, Telefone = telefone, Site = site };
            db.Fornecedor.Add(forn);
            db.SaveChanges();
            return forn;
        }

        public Impressora CadastrarImpressora(string nome, string modelo, double kwh)
        {
            ExigirPapel("admin", "gerente");
            using var db = _contextFactory.CreateDbContext();
            var imp = new Impressora { Nome = nome, Modelo = modelo, Kwh = kwh };
            db.Impressora.Add(imp);
            db.SaveChanges();
            return imp;
        }

        public MateriaPrima CadastrarMateriaPrima(string nome, int pesoCompra, double precoCompra, Fornecedor fornecedor, int estoque = 0)
        {
            ExigirPapel("admin", "gerente");
            if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("O nome da matéria-prima é obrigatório.", nameof(nome));
            if (pesoCompra <= 0) throw new ArgumentOutOfRangeException(nameof(pesoCompra), "O peso de compra deve ser maior que zero.");
            if (!double.IsFinite(precoCompra) || precoCompra < 0) throw new ArgumentOutOfRangeException(nameof(precoCompra), "O preço de compra deve ser válido e não negativo.");
            if (estoque < 0) throw new ArgumentOutOfRangeException(nameof(estoque), "O estoque não pode ser negativo.");
            if (fornecedor == null || fornecedor.Id <= 0) throw new ArgumentException("Selecione um fornecedor válido.", nameof(fornecedor));

            using var db = _contextFactory.CreateDbContext();
            using var transacao = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
            var fornRef = db.Fornecedor.Find(fornecedor.Id) ?? throw new InvalidOperationException("Fornecedor não encontrado.");
            var mat = new MateriaPrima(pesoCompra) { Nome = nome, Preco_compra = precoCompra, Fornecedor = fornRef, Unidade_Medida = "g" };
            db.MateriaPrima.Add(mat);
            db.SaveChanges();

            var saldo = ObterOuCriarEstoque(db, mat);
            saldo.Quantidade = estoque;
            if (estoque > 0) RegistrarMovimentacao(db, nameof(MateriaPrima), mat.Id, mat.Nome, "Entrada", estoque, "g", "Estoque inicial");
            db.SaveChanges();
            transacao.Commit();
            return mat;
        }

        public Embalagem CadastrarEmbalagem(string nome, string tipo, double largura, double comprimento, double altura, double preco, Fornecedor? fornecedor = null, int estoque = 0)
        {
            ExigirPapel("admin", "gerente");
            if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("O nome da embalagem é obrigatório.", nameof(nome));
            if (!double.IsFinite(preco) || preco < 0 || !double.IsFinite(largura) || largura < 0 || !double.IsFinite(comprimento) || comprimento < 0 || !double.IsFinite(altura) || altura < 0)
                throw new ArgumentException("Preço e dimensões devem ser valores válidos e não negativos.");
            if (estoque < 0) throw new ArgumentOutOfRangeException(nameof(estoque), "O estoque não pode ser negativo.");

            using var db = _contextFactory.CreateDbContext();
            using var transacao = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
            Fornecedor? fornRef = fornecedor == null ? null : db.Fornecedor.Find(fornecedor.Id);
            if (fornecedor != null && fornRef == null) throw new InvalidOperationException("Fornecedor não encontrado.");

            var emb = new Embalagem { Nome = nome, Tipo = tipo, largura = largura, comprimento = comprimento, altura = altura, Preco = preco, Fornecedor = fornRef, Unidade_Medida = "UN" };
            db.Embalagem.Add(emb);
            db.SaveChanges();
            var saldo = ObterOuCriarEstoque(db, emb);
            saldo.Quantidade = estoque;
            if (estoque > 0) RegistrarMovimentacao(db, nameof(Embalagem), emb.Id, emb.Nome, "Entrada", estoque, "UN", "Estoque inicial");
            db.SaveChanges();
            transacao.Commit();
            return emb;
        }

        public Peca CadastrarPeca(string nome, List<MateriaPrimaPesoInput> materias, Impressora imp, double pesoImp, double tempoImp, double largura, double comp, double altura, int estoqueInicial = 0)
        {
            ExigirPapel("admin", "gerente");
            if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length > 120)
                throw new ArgumentException("O nome da peça é obrigatório e deve ter até 120 caracteres.", nameof(nome));
            if (materias == null || materias.Count == 0 || materias.Count > 50 ||
                materias.Any(x => x.MateriaPrimaId <= 0 || !double.IsFinite(x.Peso) || x.Peso <= 0 || x.Peso > 100000))
                throw new ArgumentException("Adicione matérias-primas válidas (máximo de 50), cada uma com peso maior que zero.");
            if (!double.IsFinite(pesoImp) || pesoImp < 0.001 || pesoImp > 100000 ||
                Math.Abs(materias.Sum(x => x.Peso) - pesoImp) > 0.001)
                throw new ArgumentException("A soma dos pesos das matérias-primas deve ser igual ao peso total da peça.");
            if (imp == null || imp.Id <= 0) throw new ArgumentException("Selecione uma impressora válida.");
            if (!double.IsFinite(tempoImp) || tempoImp <= 0 || tempoImp > 1000000 ||
                !double.IsFinite(largura) || largura <= 0 || largura > 100000 ||
                !double.IsFinite(comp) || comp <= 0 || comp > 100000 ||
                !double.IsFinite(altura) || altura <= 0 || altura > 100000 ||
                estoqueInicial < 0 || estoqueInicial > 10000000)
                throw new ArgumentException("Tempo, dimensões ou estoque fora dos limites permitidos.");

            using var db = _contextFactory.CreateDbContext();
            using var transacao = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
            var ids = materias.Select(x => x.MateriaPrimaId).Distinct().ToList();
            if (ids.Count != materias.Count) throw new ArgumentException("Não selecione a mesma matéria-prima mais de uma vez.");
            var materiasDb = db.MateriaPrima.Where(x => ids.Contains(x.Id)).ToDictionary(x => x.Id);
            if (materiasDb.Count != ids.Count) throw new InvalidOperationException("Uma ou mais matérias-primas não foram encontradas.");
            var impressora = db.Impressora.Find(imp.Id) ?? throw new InvalidOperationException("Impressora não encontrada.");
            var peca = new Peca { Nome = nome.Trim(), Materia_Prima = materiasDb[materias[0].MateriaPrimaId], Impressora = impressora, Peso_Impressao = pesoImp, Tempo_Impressao = tempoImp, largura = largura, comprimento = comp, altura = altura, Ativo = true, Unidade_Medida = "UN" };
            foreach (var linha in materias)
                peca.MateriasPrimas.Add(new PecaMateriaPrima { MateriaPrimaId = linha.MateriaPrimaId, MateriaPrima = materiasDb[linha.MateriaPrimaId], Peso = linha.Peso });
            db.Peca.Add(peca);
            db.SaveChanges();
            var estoquePeca = ObterOuCriarEstoque(db, peca);
            if (estoqueInicial > 0)
            {
                foreach (var linha in peca.MateriasPrimas)
                {
                    var consumo = decimal.Round((decimal)linha.Peso * estoqueInicial, 3, MidpointRounding.AwayFromZero);
                    var saldoMateria = ObterOuCriarEstoque(db, linha.MateriaPrima);
                    if (!saldoMateria.Estoque_Remover(consumo, out var erro))
                        throw new InvalidOperationException($"Matéria-prima insuficiente ({linha.MateriaPrima.Nome}) para produzir {estoqueInicial} peça(s). Necessário: {consumo} g. {erro}");
                    RegistrarMovimentacao(db, nameof(MateriaPrima), linha.MateriaPrima.Id, linha.MateriaPrima.Nome, "Saída", consumo, "g", "Produção de peças");
                }
                estoquePeca.Estoque_Adicionar(estoqueInicial, out _);
                RegistrarMovimentacao(db, nameof(Peca), peca.Id, peca.Nome, "Entrada", estoqueInicial, "UN", "Estoque inicial - produção de peças");
            }
            db.SaveChanges();
            transacao.Commit();
            return peca;
        }

        // Sobrecarga de compatibilidade com cadastros antigos de uma única matéria-prima.
        public Peca CadastrarPeca(string nome, MateriaPrima mat, Impressora imp, double pesoImp, double tempoImp, double largura, double comp, double altura, int estoqueInicial = 0)
            => CadastrarPeca(nome, new List<MateriaPrimaPesoInput> { new() { MateriaPrimaId = mat.Id, Peso = pesoImp } }, imp, pesoImp, tempoImp, largura, comp, altura, estoqueInicial);

        public Produto CadastrarProduto(string nome, double valorVenda, List<ItemQuantidadeInput> pecas, int estoqueInicial = 0)
        {
            ExigirPapel("admin", "gerente");
            if (string.IsNullOrWhiteSpace(nome)) throw new ArgumentException("O nome do produto é obrigatório.");
            if (!double.IsFinite(valorVenda) || valorVenda < 0) throw new ArgumentException("O valor de venda deve ser válido e não negativo.");
            if (pecas == null || pecas.Count == 0 || pecas.Any(x => x.ItemId <= 0 || x.Quantidade <= 0)) throw new ArgumentException("Adicione peças válidas à composição do produto.");
            if (estoqueInicial < 0) throw new ArgumentOutOfRangeException(nameof(estoqueInicial));

            using var db = _contextFactory.CreateDbContext();
            using var transacao = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
            var produto = new Produto { Nome = nome.Trim(), Valor_Venda = valorVenda, Unidade_Medida = "UN", Ativo = true };
            db.Produto.Add(produto);
            db.SaveChanges();
            foreach (var grupo in pecas.GroupBy(x => x.ItemId))
            {
                if (!db.Peca.Any(x => x.Id == grupo.Key && x.Ativo)) throw new InvalidOperationException($"Peça com ID {grupo.Key} não encontrada ou está inativa.");
                produto.Composicao.Add(new ProdutoPeca { ProdutoId = produto.Id, PecaId = grupo.Key, Quantidade = checked(grupo.Sum(x => x.Quantidade)) });
            }
            ObterOuCriarEstoque(db, produto);
            db.SaveChanges();
            if (estoqueInicial > 0 && !AjustarEstoqueInterno(db, produto, estoqueInicial, true, out var mensagem)) throw new InvalidOperationException(mensagem);
            db.SaveChanges();
            transacao.Commit();
            return produto;
        }

        // Compatibilidade temporária com a tela antiga de cadastro; o novo fluxo deve cadastrar peças e composição.
        public Produto CadastrarProduto(string nome, MateriaPrima mat, Impressora imp, double valorVenda, double pesoImp, double tempoImp, double largura, double comp, double altura, int estoque)
        {
            var peca = CadastrarPeca(nome, mat, imp, pesoImp, tempoImp, largura, comp, altura, estoque);
            return CadastrarProduto(nome, valorVenda, new List<ItemQuantidadeInput> { new() { ItemId = peca.Id, Quantidade = 1 } }, estoque);
        }

        private void RegistrarMovimentacao(MyDBContext db, string tipoItem, int itemId,
                                           string nomeItem, string operacao, decimal quantidade, string unidade,
                                           string origem, int? referenciaId = null)
        {
            db.MovimentacoesEstoque.Add(new MovimentacaoEstoque
            {
                TipoItem = tipoItem,
                ItemId = itemId,
                NomeItem = nomeItem,
                Operacao = operacao,
                Quantidade = quantidade,
                Unidade = unidade,
                Origem = origem,
                ReferenciaId = referenciaId,
                DataHora = DateTime.UtcNow,
                Usuario = _usuarioAtual.ObterUsuarioAsync().GetAwaiter().GetResult()
            });
        }

        private (List<VendaItem> Itens, List<VendaEmbalagem> Embalagens) ProcessarItensEEmbalagens(
            MyDBContext db,
            List<ItemQuantidadeInput> produtosInput,
            List<ItemQuantidadeInput>? embalagensInput,
            string origemMovimento,
            int? vendaId)
        {
            if (produtosInput == null || produtosInput.Count == 0)
                throw new ArgumentException("Adicione pelo menos um produto à venda.");

            if (produtosInput.Any(p => p.ItemId <= 0 || p.Quantidade <= 0))
                throw new ArgumentException("Todos os produtos devem estar selecionados e possuir quantidade maior que zero.");

            var listaEmbalagensInput = embalagensInput ?? new List<ItemQuantidadeInput>();
            if (listaEmbalagensInput.Any(e => e.ItemId <= 0 || e.Quantidade <= 0))
                throw new ArgumentException("Todas as embalagens adicionadas devem estar selecionadas e possuir quantidade maior que zero.");

            var itensResultantes = new List<VendaItem>();
            var embalagensResultantes = new List<VendaEmbalagem>();

            // Processa produtos agrupados por ID
            foreach (var grupo in produtosInput.GroupBy(p => p.ItemId))
            {
                int totalQtd = grupo.Sum(x => x.Quantidade);
                var produto = db.Produto.FirstOrDefault(x => x.Id == grupo.Key)
                ?? throw new InvalidOperationException($"Produto com ID {grupo.Key} não foi encontrado.");

                if (!produto.Ativo)
                    throw new InvalidOperationException($"O produto '{produto.Nome}' está inativo.");

                var saldoProduto = ObterOuCriarEstoque(db, produto);
                if (!saldoProduto.Estoque_Remover(totalQtd, out var erroProduto))
                    throw new InvalidOperationException($"Estoque insuficiente do produto '{produto.Nome}'. {erroProduto} Faça a fabricação do produto antes de registrar a venda.");
                RegistrarMovimentacao(db, nameof(Produto), produto.Id, produto.Nome, "Saída", totalQtd, "UN", origemMovimento, vendaId);

                foreach (var linha in grupo)
                {
                    itensResultantes.Add(new VendaItem
                    {
                        ProdutoId = produto.Id,
                        Produto = produto,
                        Quantidade = linha.Quantidade
                    });
                }
            }

            // Processa embalagens agrupadas por ID
            foreach (var grupo in listaEmbalagensInput.GroupBy(e => e.ItemId))
            {
                int totalQtd = grupo.Sum(x => x.Quantidade);
                var embalagem = db.Embalagem.FirstOrDefault(x => x.Id == grupo.Key)
                ?? throw new InvalidOperationException($"Embalagem com ID {grupo.Key} não foi encontrada.");

                var saldoEmbalagem = ObterOuCriarEstoque(db, embalagem);
                if (!saldoEmbalagem.Estoque_Remover(totalQtd, out var erroEmbalagem))
                    throw new InvalidOperationException($"Estoque insuficiente da embalagem '{embalagem.Nome}'. {erroEmbalagem}");

                RegistrarMovimentacao(db, nameof(Embalagem), embalagem.Id, embalagem.Nome, "Saída", totalQtd, "UN", origemMovimento, vendaId);

                foreach (var linha in grupo)
                {
                    embalagensResultantes.Add(new VendaEmbalagem
                    {
                        EmbalagemId = embalagem.Id,
                        Embalagem = embalagem,
                        Quantidade = linha.Quantidade
                    });
                }
            }

            return (itensResultantes, embalagensResultantes);
        }

        public Venda CadastrarVendaSobDemanda(int clienteId, List<ItemQuantidadeInput> produtos, List<ItemQuantidadeInput>? embalagens, DateTime dataHora)
        {
            if (clienteId <= 0)
                throw new ArgumentException("Selecione um cliente válido.");

            using var db = _contextFactory.CreateDbContext();
            using var transacao = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            var cliente = db.Cliente.Find(clienteId)
            ?? throw new InvalidOperationException($"Cliente com ID {clienteId} não foi encontrado.");

            var (itensProcessados, embalagensProcessadas) = ProcessarItensEEmbalagens(db, produtos, embalagens, "Venda", null);

            var venda = new Venda
            {
                Cliente = cliente,
                ItensVenda = itensProcessados,
                EmbalagensVenda = embalagensProcessadas,
                Data_Hora = dataHora.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dataHora, DateTimeKind.Utc) : dataHora.ToUniversalTime()
            };

            db.Venda.Add(venda);
            db.SaveChanges();

            foreach (var mov in db.ChangeTracker.Entries<MovimentacaoEstoque>().Where(x => x.Entity.ReferenciaId == null))
                mov.Entity.ReferenciaId = venda.Id;

            db.SaveChanges();
            transacao.Commit();
            return venda;
        }

        public void AtualizarVendaSobDemanda(int vendaId, int clienteId, List<ItemQuantidadeInput> novosProdutos, List<ItemQuantidadeInput>? novasEmbalagens, DateTime dataHora)
        {
            ExigirPapel("admin", "gerente");
            if (vendaId <= 0 || clienteId <= 0)
                throw new ArgumentException("Confira os dados da venda.");

            using var db = _contextFactory.CreateDbContext();
            using var transacao = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            var venda = db.Venda
            .Include(x => x.ItensVenda)
            .Include(x => x.EmbalagensVenda)
            .FirstOrDefault(x => x.Id == vendaId)
            ?? throw new InvalidOperationException("Venda não encontrada.");

            var cliente = db.Cliente.Find(clienteId) ?? throw new InvalidOperationException("Cliente não encontrado.");

            // Estorna produtos antigos
            foreach (var itemAntigo in venda.ItensVenda)
            {
                var prod = db.Produto.Find(itemAntigo.ProdutoId)!;
                ObterOuCriarEstoque(db, prod).Estoque_Adicionar(itemAntigo.Quantidade, out _);
                RegistrarMovimentacao(db, nameof(Produto), prod.Id, prod.Nome,
                                      "Entrada", itemAntigo.Quantidade, "UN", "Edição de venda - estorno", vendaId);
            }

            // Estorna embalagens antigas
            foreach (var embAntiga in venda.EmbalagensVenda)
            {
                var emb = db.Embalagem.Find(embAntiga.EmbalagemId)!;
                ObterOuCriarEstoque(db, emb).Estoque_Adicionar(embAntiga.Quantidade, out _);
                RegistrarMovimentacao(db, nameof(Embalagem), emb.Id, emb.Nome,
                                      "Entrada", embAntiga.Quantidade, "UN", "Edição de venda - estorno", vendaId);
            }

            db.VendaItem.RemoveRange(venda.ItensVenda);
            db.VendaEmbalagem.RemoveRange(venda.EmbalagensVenda);
            venda.ItensVenda.Clear();
            venda.EmbalagensVenda.Clear();

            var (itensProcessados, embalagensProcessadas) = ProcessarItensEEmbalagens(db, novosProdutos, novasEmbalagens, "Edição de venda", vendaId);

            venda.Cliente = cliente;
            venda.ItensVenda = itensProcessados;
            venda.EmbalagensVenda = embalagensProcessadas;
            venda.Data_Hora = dataHora.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dataHora, DateTimeKind.Utc) : dataHora.ToUniversalTime();

            db.SaveChanges();
            transacao.Commit();
        }

        public bool CancelarVendaRestaurandoEstoque(int vendaId)
        {
            ExigirPapel("admin", "gerente");
            using var db = _contextFactory.CreateDbContext();
            using var transacao = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            var venda = db.Venda
            .Include(x => x.ItensVenda)
            .Include(x => x.EmbalagensVenda)
            .FirstOrDefault(x => x.Id == vendaId);

            if (venda == null) return false;

            foreach (var item in venda.ItensVenda)
            {
                var produto = db.Produto.Find(item.ProdutoId)!;
                ObterOuCriarEstoque(db, produto).Estoque_Adicionar(item.Quantidade, out _);
                RegistrarMovimentacao(db, nameof(Produto), produto.Id, produto.Nome,
                                      "Entrada", item.Quantidade, "UN", "Cancelamento de venda", vendaId);
            }

            foreach (var emb in venda.EmbalagensVenda)
            {
                var embalagem = db.Embalagem.Find(emb.EmbalagemId)!;
                ObterOuCriarEstoque(db, embalagem).Estoque_Adicionar(emb.Quantidade, out _);
                RegistrarMovimentacao(db, nameof(Embalagem), embalagem.Id, embalagem.Nome,
                                      "Entrada", emb.Quantidade, "UN", "Cancelamento de venda", vendaId);
            }

            db.Venda.Remove(venda);
            db.SaveChanges();
            transacao.Commit();
            return true;
        }

        public List<MovimentacaoEstoque> ListarMovimentacoesEstoque(int limite = 500)
        {
            using var db = _contextFactory.CreateDbContext();
            return db.MovimentacoesEstoque.AsNoTracking()
            .OrderByDescending(x => x.DataHora).ThenByDescending(x => x.Id)
            .Take(Math.Clamp(limite, 1, 5000)).ToList();
        }

        private bool AjustarEstoqueInterno(MyDBContext db, Item item, int quantidade, bool adicionar, out string mensagem)
        {
            if (quantidade <= 0) { mensagem = "A quantidade deve ser maior que zero."; return false; }
            var saldo = ObterOuCriarEstoque(db, item);

            if (item is Peca peca && adicionar)
            {
                var pecaCompleta = db.Peca.Include(x => x.Materia_Prima).Include(x => x.MateriasPrimas).ThenInclude(x => x.MateriaPrima).FirstOrDefault(x => x.Id == peca.Id)
                    ?? throw new InvalidOperationException("Peça não encontrada.");
                if (!pecaCompleta.Ativo) { mensagem = $"A peça '{pecaCompleta.Nome}' está inativa."; return false; }
                List<(MateriaPrima Materia, double Peso)> composicaoMaterias;
                if (pecaCompleta.MateriasPrimas.Count > 0)
                    composicaoMaterias = pecaCompleta.MateriasPrimas.Select(x => (Materia: x.MateriaPrima, Peso: x.Peso)).ToList();
                else if (pecaCompleta.Materia_Prima != null)
                    composicaoMaterias = new List<(MateriaPrima Materia, double Peso)> { (pecaCompleta.Materia_Prima, pecaCompleta.Peso_Impressao) };
                else
                    composicaoMaterias = new List<(MateriaPrima Materia, double Peso)>();
                if (composicaoMaterias.Count == 0) { mensagem = $"A peça '{peca.Nome}' não possui matérias-primas vinculadas."; return false; }
                foreach (var linha in composicaoMaterias)
                {
                    var consumo = decimal.Round((decimal)linha.Peso * quantidade, 3, MidpointRounding.AwayFromZero);
                    if (consumo <= 0) { mensagem = "O peso de impressão é muito pequeno para registrar o consumo em gramas."; return false; }
                    var saldoMateria = ObterOuCriarEstoque(db, linha.Materia);
                    if (!saldoMateria.Estoque_Remover(consumo, out var erro))
                    { mensagem = $"Matéria-prima insuficiente ({linha.Materia.Nome}) para produzir {quantidade} unidade(s) de '{peca.Nome}'. Necessário: {consumo} g. {erro}"; return false; }
                    RegistrarMovimentacao(db, nameof(MateriaPrima), linha.Materia.Id, linha.Materia.Nome, "Saída", consumo, "g", "Produção de peças");
                }
                saldo.Estoque_Adicionar(quantidade, out _);
                RegistrarMovimentacao(db, nameof(Peca), peca.Id, peca.Nome, "Entrada", quantidade, "UN", "Produção de peças");
                mensagem = "Peças produzidas e estoque atualizado com sucesso.";
                return true;
            }

            if (item is Produto produto && adicionar)
            {
                if (!produto.Ativo) { mensagem = $"O produto '{produto.Nome}' está inativo."; return false; }
                var composicao = db.ProdutoPeca.Include(x => x.Peca).Where(x => x.ProdutoId == produto.Id).ToList();
                if (composicao.Count == 0) { mensagem = $"O produto '{produto.Nome}' não possui peças cadastradas na composição."; return false; }
                foreach (var linha in composicao)
                {
                    if (!linha.Peca.Ativo) { mensagem = $"A peça '{linha.Peca.Nome}' está inativa e não pode ser usada na fabricação."; return false; }
                    if (linha.Quantidade <= 0) { mensagem = "A composição contém uma quantidade inválida."; return false; }
                    var necessario = checked(linha.Quantidade * quantidade);
                    var saldoPeca = ObterOuCriarEstoque(db, linha.Peca);
                    if (!saldoPeca.Estoque_Remover(necessario, out var erro))
                    { mensagem = $"Estoque insuficiente da peça '{linha.Peca.Nome}'. Necessário: {necessario} UN. {erro}"; return false; }
                    RegistrarMovimentacao(db, nameof(Peca), linha.Peca.Id, linha.Peca.Nome, "Saída", necessario, "UN", "Fabricação de produto");
                }
                saldo.Estoque_Adicionar(quantidade, out _);
                RegistrarMovimentacao(db, nameof(Produto), produto.Id, produto.Nome, "Entrada", quantidade, "UN", "Fabricação de produto");
                mensagem = "Produto fabricado e estoque atualizado com sucesso.";
                return true;
            }

            var sucesso = adicionar ? saldo.Estoque_Adicionar(quantidade, out mensagem) : saldo.Estoque_Remover(quantidade, out mensagem);
            if (!sucesso) return false;
            RegistrarMovimentacao(db, item.GetType().Name, item.Id, item.Nome, adicionar ? "Entrada" : "Saída", quantidade, saldo.Unidade_Medida, "Ajuste manual");
            return true;
        }

        public bool AjustarEstoque<T>(int id, int quantidade, bool adicionar, out string mensagem) where T : class
        {
            ExigirPapel("admin", "gerente");
            if (id <= 0) { mensagem = "Informe um ID válido."; return false; }
            if (quantidade <= 0) { mensagem = "A quantidade deve ser maior que zero."; return false; }
            using var db = _contextFactory.CreateDbContext();
            using var transacao = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
            Item? item = typeof(T) == typeof(Produto) ? db.Produto.FirstOrDefault(x => x.Id == id)
                : typeof(T) == typeof(Peca) ? db.Peca.Include(x => x.Materia_Prima).FirstOrDefault(x => x.Id == id)
                : typeof(T) == typeof(MateriaPrima) ? db.MateriaPrima.Find(id)
                : typeof(T) == typeof(Embalagem) ? db.Embalagem.Find(id) : null;
            if (item == null) { mensagem = $"{typeof(T).Name} com ID {id} não foi encontrado ou não permite movimentação."; return false; }
            if (!AjustarEstoqueInterno(db, item, quantidade, adicionar, out mensagem)) return false;
            db.SaveChanges();
            transacao.Commit();
            return true;
        }

        // Registra somente logins concluídos com sucesso. A senha e outros dados
        // de autenticação nunca são armazenados no histórico.
        public void RegistrarLogin(int usuarioId, string login)
        {
            if (usuarioId <= 0 || string.IsNullOrWhiteSpace(login))
                throw new ArgumentException("Dados do usuário inválidos para auditoria.");

            using var db = _contextFactory.CreateDbContext();
            db.Auditoria.Add(new Auditoria
            {
                Entidade = "Sessão",
                EntidadeId = usuarioId,
                Operacao = "Login realizado",
                Usuario = login.Trim(),
                DataHora = DateTime.UtcNow
            });
            db.SaveChanges();
        }

        public Usuario? ValidarUsuario(string login, string senha)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrEmpty(senha) || login.Length > 100 || senha.Length > 1024)
                return null;

            using var db = _contextFactory.CreateDbContext();
            var usuario = db.Usuario.FirstOrDefault(u => u.Login.ToLower() == login.Trim().ToLower());
            if (usuario == null) return null;

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Usuario>();
            bool valido = false;

            if (!string.IsNullOrWhiteSpace(usuario.SenhaHash))
            {
                var resultado = hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);
                valido = resultado != Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed;

                if (resultado == Microsoft.AspNetCore.Identity.PasswordVerificationResult.SuccessRehashNeeded)
                {
                    usuario.SenhaHash = hasher.HashPassword(usuario, senha);
                    db.SaveChanges();
                }
            }
            else
            {
                var sha256 = Convert.ToBase64String(
                    System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(senha)));
                valido = string.Equals(usuario.Senha, senha, StringComparison.Ordinal) ||
                string.Equals(usuario.Senha, sha256, StringComparison.Ordinal);

                if (valido)
                {
                    usuario.SenhaHash = hasher.HashPassword(usuario, senha);
                    usuario.Senha = string.Empty;
                    db.SaveChanges();
                }
            }

            if (valido)
            {
                var roleNormalizada = (usuario.Role ?? string.Empty).Trim().ToLowerInvariant();
                if (roleNormalizada is not ("admin" or "gerente" or "funcionario")) return null;
                if (usuario.Role != roleNormalizada) { usuario.Role = roleNormalizada; db.SaveChanges(); }
            }

            return valido ? usuario : null;
        }

        public bool CriarAdminInicialSeVazio(string login, string senha)
        {
            login = (login ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(login) || login.Length > 100 ||
                string.IsNullOrWhiteSpace(senha) || senha.Length < 10 || senha.Length > 1024)
                throw new ArgumentException("As credenciais iniciais do administrador são inválidas.");

            using var db = _contextFactory.CreateDbContext();
            using var transacao = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

            if (db.Usuario.Any()) { transacao.Rollback(); return false; }

            var admin = new Usuario { Login = login, Role = "admin", Senha = string.Empty };
            admin.SenhaHash = new Microsoft.AspNetCore.Identity.PasswordHasher<Usuario>().HashPassword(admin, senha);
            db.Usuario.Add(admin);
            db.SaveChanges();
            transacao.Commit();
            return true;
        }

        public Usuario CadastrarUsuario(string login, string senha, string role)
        {
            ExigirPapel("admin", "gerente");
            login = (login ?? string.Empty).Trim();
            role = (role ?? string.Empty).Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(login) || login.Length > 100) throw new ArgumentException("Informe um login válido.");
            if (string.IsNullOrWhiteSpace(senha) || senha.Length < 10 || senha.Length > 1024) throw new ArgumentException("A senha deve ter pelo menos 10 caracteres.");
            if (role is not ("admin" or "gerente" or "funcionario")) throw new ArgumentException("Cargo inválido.");
            if (role == "admin") ExigirPapel("admin");

            using var db = _contextFactory.CreateDbContext();
            if (db.Usuario.Any(u => u.Login.ToLower() == login.ToLower()))
                throw new InvalidOperationException("Já existe um usuário com esse login.");

            var novo = new Usuario { Login = login, Role = role, Senha = string.Empty };
            novo.SenhaHash = new Microsoft.AspNetCore.Identity.PasswordHasher<Usuario>().HashPassword(novo, senha);

            db.Usuario.Add(novo);
            db.SaveChanges();
            return novo;
        }

        public void AtualizarUsuario(int id, string login, string role, string? novaSenha)
        {
            ExigirPapel("admin");
            login = (login ?? string.Empty).Trim();
            role = (role ?? string.Empty).Trim().ToLowerInvariant();
            if (id <= 0 || string.IsNullOrWhiteSpace(login) || login.Length > 100) throw new ArgumentException("Informe um usuário e login válidos.");
            if (role is not ("admin" or "gerente" or "funcionario")) throw new ArgumentException("Cargo inválido.");
            if (!string.IsNullOrEmpty(novaSenha) && (novaSenha.Length < 10 || novaSenha.Length > 1024)) throw new ArgumentException("A nova senha deve ter pelo menos 10 caracteres.");

            using var db = _contextFactory.CreateDbContext();
            var usuario = db.Usuario.FirstOrDefault(x => x.Id == id) ?? throw new InvalidOperationException("Usuário não encontrado.");

            if (db.Usuario.Any(x => x.Id != id && x.Login.ToLower() == login.ToLower()))
                throw new InvalidOperationException("Já existe outro usuário com esse login.");

            if (usuario.Role == "admin" && role != "admin" && db.Usuario.Count(x => x.Role == "admin") <= 1)
                throw new InvalidOperationException("Não é possível remover o último administrador.");

            usuario.Login = login;
            usuario.Role = role;
            if (!string.IsNullOrWhiteSpace(novaSenha))
            {
                usuario.SenhaHash = new Microsoft.AspNetCore.Identity.PasswordHasher<Usuario>().HashPassword(usuario, novaSenha);
                usuario.Senha = string.Empty;
            }

            db.SaveChanges();
        }

        public bool ExcluirUsuario(int id)
        {
            ExigirPapel("admin");
            using var db = _contextFactory.CreateDbContext();
            var usuario = db.Usuario.FirstOrDefault(x => x.Id == id);
            if (usuario == null) return false;

            var atual = _usuarioAtual.ObterUsuarioAsync().GetAwaiter().GetResult();
            if (string.Equals(usuario.Login, atual, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Você não pode excluir o próprio usuário enquanto estiver conectado.");

            if (usuario.Role == "admin" && db.Usuario.Count(x => x.Role == "admin") <= 1)
                throw new InvalidOperationException("Não é possível excluir o último administrador.");

            db.Usuario.Remove(usuario);
            db.SaveChanges();
            return true;
        }

        public List<Usuario> ListarUsuariosSemCredenciais()
        {
            ExigirPapel("admin");
            using var db = _contextFactory.CreateDbContext();
            return db.Usuario.AsNoTracking()
            .Select(u => new { u.Id, u.Login, u.Role })
            .AsEnumerable()
            .Select(u => new Usuario { Id = u.Id, Login = u.Login, Role = u.Role })
            .ToList();
        }

        public Usuario? ObterUsuarioSemCredenciais(int id)
        {
            ExigirPapel("admin");
            using var db = _contextFactory.CreateDbContext();
            var dados = db.Usuario.AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new { u.Id, u.Login, u.Role })
            .FirstOrDefault();

            return dados == null ? null : new Usuario { Id = dados.Id, Login = dados.Login, Role = dados.Role };
        }

        public List<Auditoria> ListarAuditoria(int limite = 300)
        {
            ExigirPapel("admin");
            using var db = _contextFactory.CreateDbContext();
            return db.Auditoria.AsNoTracking()
            .OrderByDescending(x => x.DataHora)
            .ThenByDescending(x => x.Id)
            .Take(Math.Clamp(limite, 1, 1000))
            .Select(x => new Auditoria
            {
                Id = x.Id,
                Entidade = x.Entidade,
                EntidadeId = x.EntidadeId,
                Operacao = x.Operacao,
                Usuario = x.Usuario,
                DataHora = x.DataHora
            })
            .ToList();
        }
        #endregion
    }
}
