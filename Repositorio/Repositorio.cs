using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TechToysDominio;

namespace Repositorio
{
    public class RepositorioTechToys
    {
        private readonly IDbContextFactory<MyDBContext> _contextFactory;

        public RepositorioTechToys(IDbContextFactory<MyDBContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // Garante que o banco e as tabelas existam
        public void CriarBanco()
        {
            using var db = _contextFactory.CreateDbContext();
            db.Database.EnsureCreated();
        }

        // Limpa todas as tabelas resetando IDs
        public void LimparBanco()
        {
            using var db = _contextFactory.CreateDbContext();
            db.Database.ExecuteSqlRaw(@"
                TRUNCATE TABLE 
                    ""Venda"", ""Produto"", ""Embalagem"", ""MateriaPrima"", ""Cliente"", ""Impressora"", ""Fornecedor"" 
                RESTART IDENTITY CASCADE;");
        }

        // Limpa a tabela desejada resetando IDs
        public void LimparBanco(string tabela)
        {
            using var db = _contextFactory.CreateDbContext();
            db.Database.ExecuteSqlRaw(@$"
                TRUNCATE TABLE 
                    ""{tabela}"" 
                RESTART IDENTITY CASCADE;");
        }

        #region Funções Globais (CRUD Genérico para Qualquer Tabela)

        // READ: Obter registro por ID
        public T ObterPorId<T>(int id) where T : class
        {
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
                    .Include(x => x.Materia_Prima)
                    .Include(x => x.Impressora)
                    .FirstOrDefault(x => x.Id == id) as T;
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
                    .Include(x => x.Produto)
                        .ThenInclude(x => x.Materia_Prima)
                    .Include(x => x.Produto)
                        .ThenInclude(x => x.Impressora)
                    .Include(x => x.Embalagem)
                    .FirstOrDefault(x => x.Id == id) as T;
            }

            return db.Set<T>().Find(id);
        }

        // READ: Listar todos com AsNoTracking() para evitar dados defasados em cache
        public List<T> ListarTodos<T>() where T : class
        {
            using var db = _contextFactory.CreateDbContext();

            if (typeof(T) == typeof(MateriaPrima))
            {
                return db.MateriaPrima
                    .AsNoTracking()
                    .Include(x => x.Fornecedor)
                    .ToList() as List<T>;
            }

            if (typeof(T) == typeof(Produto))
            {
                return db.Produto
                    .AsNoTracking()
                    .Include(x => x.Materia_Prima)
                    .Include(x => x.Impressora)
                    .ToList() as List<T>;
            }

            if (typeof(T) == typeof(Embalagem))
            {
                return db.Embalagem
                    .AsNoTracking()
                    .Include(x => x.Fornecedor)
                    .ToList() as List<T>;
            }

            if (typeof(T) == typeof(Venda))
            {
                return db.Venda
                    .AsNoTracking()
                    .Include(x => x.Cliente)
                    .Include(x => x.Produto)
                    .Include(x => x.Embalagem)
                    .ToList() as List<T>;
            }

            return db.Set<T>().AsNoTracking().ToList();
        }

        // UPDATE: Atualiza qualquer entidade alterada
        public void Atualizar<T>(T entidade) where T : class
        {
            using var db = _contextFactory.CreateDbContext();
            db.Set<T>().Update(entidade);
            db.SaveChanges();
        }

        // DELETE: Deleta pelo ID em qualquer tabela
        public bool DeletarPorId<T>(int id) where T : class
        {
            using var db = _contextFactory.CreateDbContext();
            var item = db.Set<T>().Find(id);
            if (item == null) return false;

            db.Set<T>().Remove(item);
            db.SaveChanges();
            return true;
        }

        #endregion

        #region Funções de Cadastro Específicas

        public Cliente CadastrarCliente(string nome, string telefone, string email, string instagram)
        {
            using var db = _contextFactory.CreateDbContext();
            var cli = new Cliente
            {
                Nome = nome,
                Telefone = telefone,
                Email = email,
                Instagram = instagram
            };
            db.Cliente.Add(cli);
            db.SaveChanges();
            return cli;
        }

        public Fornecedor CadastrarFornecedor(string nome, string email, string telefone, string site)
        {
            using var db = _contextFactory.CreateDbContext();
            var forn = new Fornecedor
            {
                Nome = nome,
                Email = email,
                Telefone = telefone,
                Site = site
            };
            db.Fornecedor.Add(forn);
            db.SaveChanges();
            return forn;
        }

        public Impressora CadastrarImpressora(string nome, string modelo, double kwh)
        {
            using var db = _contextFactory.CreateDbContext();
            var imp = new Impressora
            {
                Nome = nome,
                Modelo = modelo,
                Kwh = kwh
            };
            db.Impressora.Add(imp);
            db.SaveChanges();
            return imp;
        }

        public MateriaPrima CadastrarMateriaPrima(string nome, int pesoCompra, double precoCompra, Fornecedor fornecedor, int estoque)
        {
            using var db = _contextFactory.CreateDbContext();
            if (fornecedor != null && fornecedor.Id > 0)
                db.Attach(fornecedor);

            var mat = new MateriaPrima(pesoCompra)
            {
                Nome = nome,
                Preco_compra = precoCompra,
                Fornecedor = fornecedor,
                Estoque = estoque
            };
            db.MateriaPrima.Add(mat);
            db.SaveChanges();
            return mat;
        }

        public Embalagem CadastrarEmbalagem(string nome, string tipo, double largura, double comprimento, double altura, double preco, Fornecedor fornecedor = null, int estoque = 0)
        {
            using var db = _contextFactory.CreateDbContext();
            if (fornecedor != null && fornecedor.Id > 0)
                db.Attach(fornecedor);

            var emb = new Embalagem
            {
                Nome = nome,
                Tipo = tipo,
                largura = largura,
                comprimento = comprimento,
                altura = altura,
                Preco = preco,
                Estoque = estoque,
                Fornecedor = fornecedor
            };
            db.Embalagem.Add(emb);
            db.SaveChanges();
            return emb;
        }

        public Produto CadastrarProduto(string nome, MateriaPrima mat, Impressora imp, double valorVenda, double pesoImp, double tempoImp, double largura, double comp, double altura, int estoque)
        {
            using var db = _contextFactory.CreateDbContext();
            if (mat != null && mat.Id > 0) db.Attach(mat);
            if (imp != null && imp.Id > 0) db.Attach(imp);

            var prod = new Produto
            {
                Nome = nome,
                Materia_Prima = mat,
                Impressora = imp,
                Valor_Venda = valorVenda,
                Peso_Impressao = pesoImp,
                Tempo_Impressao = tempoImp,
                largura = largura,
                comprimento = comp,
                altura = altura,
                Estoque = estoque,
                Ativo = true
            };
            db.Produto.Add(prod);
            db.SaveChanges();
            return prod;
        }

        public Venda CadastrarVenda(Cliente cliente, Produto produto, int quantidade, Embalagem embalagem, int quantidadeEmbalagem, DateTime dataHora)
        {
            using var db = _contextFactory.CreateDbContext();
            if (cliente != null && cliente.Id > 0) db.Attach(cliente);
            if (produto != null && produto.Id > 0) db.Attach(produto);
            if (embalagem != null && embalagem.Id > 0) db.Attach(embalagem);

            var venda = new Venda
            {
                Cliente = cliente,
                Produto = produto,
                Quantidade = quantidade,
                Embalagem = embalagem,
                Quantidade_Embalagem = quantidadeEmbalagem,
                Data_Hora = dataHora
            };

            if (embalagem != null)
            {
                venda.Calcular_Embalagens();
            }

            db.Venda.Add(venda);
            db.SaveChanges();
            return venda;
        }

        #endregion
    }
}