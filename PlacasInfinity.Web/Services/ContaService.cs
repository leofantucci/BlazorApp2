using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Repositorio;
using TechToysDominio;

namespace PlacasInfinity.Web.Services;

public sealed class ContaService(
    IDbContextFactory<MyDBContext> factory,
    IPasswordHasher<Usuario> hasher)
{
    public async Task<UsuarioComercial?> ObterUsuarioAsync(int id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Set<UsuarioComercial>().Include(x => x.Cliente)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<bool> RegistrarAsync(string nome, string email, string senha, string telefone,
        CancellationToken ct = default)
    {
        email = email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(nome) || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email)
            || senha.Length < 8 || senha.Length > 128)
            return false;

        await using var db = await factory.CreateDbContextAsync(ct);
        if (await db.Set<Usuario>().AnyAsync(x => x.Login.ToLower() == email, ct))
            return false;

        var cliente = new Cliente
        {
            Nome = nome.Trim(), Email = email,
            Telefone = string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim(),
            TipoCliente = TipoCliente.PF
        };
        var usuario = new UsuarioComercial
        {
            Login = email, Role = "cliente", Cliente = cliente
        };
        usuario.SenhaHash = hasher.HashPassword(usuario, senha);
        db.Set<UsuarioComercial>().Add(usuario);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<UsuarioComercial?> AutenticarAsync(string email, string senha, CancellationToken ct = default)
    {
        email = email.Trim().ToLowerInvariant();
        await using var db = await factory.CreateDbContextAsync(ct);
        var usuario = await db.Set<UsuarioComercial>().Include(x => x.Cliente)
            .FirstOrDefaultAsync(x => x.Login.ToLower() == email, ct);
        if (usuario?.SenhaHash is null) return null;
        var result = hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);
        return result == PasswordVerificationResult.Failed ? null : usuario;
    }

    public async Task AtualizarClienteAsync(int usuarioId, Cliente dados, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var usuario = await db.Set<UsuarioComercial>().Include(x => x.Cliente)
            .FirstOrDefaultAsync(x => x.Id == usuarioId, ct)
            ?? throw new InvalidOperationException("Conta não encontrada.");
        var c = usuario.Cliente;
        c.Nome = dados.Nome.Trim(); c.NomeFantasia = dados.NomeFantasia;
        c.TipoCliente = dados.TipoCliente; c.CPF = dados.CPF; c.CNPJ = dados.CNPJ;
        c.Telefone = dados.Telefone; c.Endereco = dados.Endereco; c.Numero = dados.Numero;
        c.Complemento = dados.Complemento; c.Bairro = dados.Bairro; c.Cidade = dados.Cidade;
        c.Estado = dados.Estado?.ToUpperInvariant(); c.CEP = dados.CEP;
        await db.SaveChangesAsync(ct);
    }

    public async Task<Cliente?> ObterClienteAsync(int usuarioId, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Set<UsuarioComercial>().Where(x => x.Id == usuarioId)
            .Select(x => x.Cliente).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<List<VendaComercial>> ListarPedidosAsync(int usuarioId, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.VendaComercial.AsNoTracking()
            .Where(v => db.Set<UsuarioComercial>().Any(u => u.Id == usuarioId && u.ClienteId == v.ClienteId))
            .Include(v => v.ItensVenda).ThenInclude(i => i.ProdutoComercial)
            .OrderByDescending(v => v.Data_Hora).ToListAsync(ct);
    }

    public static ClaimsPrincipal CriarPrincipal(UsuarioComercial usuario)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Cliente?.Nome ?? usuario.Login),
            new Claim(ClaimTypes.Email, usuario.Login),
            new Claim(ClaimTypes.Role, "cliente")
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }
}
