using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TechToysDominio;

namespace Repositorio;

public sealed class AuditoriaInterceptor : SaveChangesInterceptor
{
    private readonly UsuarioAtualService _usuarioService;

    public AuditoriaInterceptor(UsuarioAtualService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        PrepararAuditoria(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        PrepararAuditoria(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void PrepararAuditoria(DbContext? context)
    {
        if (context == null) return;

        context.ChangeTracker.DetectChanges();
        var usuario = _usuarioService.ObterUsuarioAsync().GetAwaiter().GetResult();
        var auditorias = new List<Auditoria>();

        // Materializa antes de adicionar auditorias para não iterar sobre a própria auditoria.
        var alteracoes = context.ChangeTracker.Entries()
            .Where(entry => entry.Entity is not Auditoria &&
                            entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            // Leituras públicas do QR atualizam somente métricas; não precisam gerar
            // uma linha de auditoria por escaneamento.
            .Where(entry => !(entry.Entity is QRCode &&
                              entry.State == EntityState.Modified &&
                              entry.Properties.Where(p => p.IsModified)
                                  .All(p => p.Metadata.Name is "TotalAcessos" or "UltimoAcessoUtc")))
            .ToList();

        foreach (var entry in alteracoes)
        {
            var auditoria = new Auditoria
            {
                Entidade = entry.Metadata.ClrType.Name,
                Usuario = usuario,
                DataHora = DateTime.UtcNow,
                Operacao = entry.State switch
                {
                    EntityState.Added => "Adicionado",
                    EntityState.Modified => "Editado",
                    EntityState.Deleted => "Removido",
                    _ => "Alterado"
                }
            };

            var idProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
            if (idProp?.CurrentValue != null)
                auditoria.EntidadeId = Convert.ToInt32(idProp.CurrentValue);

            if (entry.State == EntityState.Added)
                auditoria.DadosNovos = SerializarPropriedades(entry);
            else if (entry.State == EntityState.Deleted)
                auditoria.DadosAntigos = SerializarPropriedades(entry);
            else
            {
                auditoria.DadosAntigos = SerializarPropriedades(entry, somenteAlterados: true);
                auditoria.DadosNovos = SerializarPropriedades(entry, somenteAlterados: true);
            }

            auditorias.Add(auditoria);
        }

        if (auditorias.Count > 0)
            context.Set<Auditoria>().AddRange(auditorias);
    }

    private static string SerializarPropriedades(
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry,
        bool somenteAlterados = false)
    {
        var nomes = new List<string>();

        foreach (var propriedade in entry.Properties)
        {
            var nome = propriedade.Metadata.Name;
            if (somenteAlterados && !propriedade.IsModified) continue;

            // Registra apenas nomes de campos, nunca valores. Credenciais e campos
            // de contato são omitidos até mesmo da lista de propriedades.
            if (nome.Contains("senha", StringComparison.OrdinalIgnoreCase) ||
                nome.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                nome.Contains("token", StringComparison.OrdinalIgnoreCase) ||
                nome.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
                nome.Equals("Email", StringComparison.OrdinalIgnoreCase) ||
                nome.Equals("Telefone", StringComparison.OrdinalIgnoreCase) ||
                nome.Equals("Instagram", StringComparison.OrdinalIgnoreCase))
                continue;

            nomes.Add(nome);
        }

        return JsonSerializer.Serialize(nomes);
    }
}
