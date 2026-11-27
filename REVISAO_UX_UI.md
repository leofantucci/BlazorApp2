# Revisão de UX/UI — Infinity Soluções

## Escopo
- Aplicação de gestão Blazor: revisão estrutural das 91 rotas e dos componentes compartilhados de layout.
- Loja Placas Infinity: ajustes responsivos e de acessibilidade nos fluxos de catálogo, carrinho, checkout, conta e ativação/edição de placas.
- Mantidas rotas, componentes funcionais, modelos, serviços, persistência e regras de autorização existentes.

## Alterações
- Padronização global de espaçamentos, tipografia, superfícies, bordas, cartões, botões e campos.
- Layout principal com largura de conteúdo controlada e navegação lateral dimensionada para desktop.
- Navegação móvel em coluna, menu recolhível e áreas de toque maiores.
- Ajustes para telas estreitas, tablets e larguras intermediárias.
- Tabelas com rolagem horizontal em vez de corte de colunas; suporte também às tabelas legadas sem wrapper responsivo explícito.
- Formulários com campos fluidos, altura mínima adequada e foco visível.
- Acessibilidade de teclado, contraste de estados, quebra de textos longos e respeito à preferência por movimento reduzido.
- Refinos adicionais nas páginas da loja Placas Infinity.

## Arquivos alterados
- `BlazorApp2/Components/Layout/MainLayout.razor`
- `BlazorApp2/wwwroot/app.css`
- `PlacasInfinity.Web/wwwroot/app.css`

## Validação
- Verificação estática de 91 rotas na aplicação principal e dos componentes da loja.
- CSS analisado sem erros sintáticos.
- Não foi possível executar `dotnet build` ou testes automatizados neste ambiente porque o .NET SDK não está instalado. Recomenda-se compilar e validar os fluxos no ambiente de desenvolvimento com o SDK .NET 10 utilizado pelo projeto.
