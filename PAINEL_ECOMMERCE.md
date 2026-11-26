# E-commerce — Gestão Infinity / Placas Infinity

## Fluxo implementado

- `/ecommerce`: atalhos para o painel, catálogo administrativo e loja pública.
- `/produtos-comerciais/visualizar`: lista produtos, mostra se estão publicados e oferece ações por produto.
- `/produtos-comerciais/cadastrar`: cadastro e publicação de produto.
- `/produtos-comerciais/editar/{id}`: edição do produto selecionado.
- `/produtos-comerciais/remover/{id}`: remoção lógica (oculta da loja sem apagar o registro usado no histórico de pedidos).
- `/painel-de-controle` (também disponível em `/vendas-comerciais/visualizar`): acompanhamento de pedidos, itens, cliente/entrega, pagamentos, histórico e alteração de status.

As páginas administrativas exigem os papéis `admin` ou `gerente`.

## Integração com a loja pública

O projeto `PlacasInfinity.Web` já utiliza o mesmo `MyDBContext` e a mesma variável `DATABASE_URL` do Gestão Infinity. A página inicial da loja consulta `ProdutoComercial` filtrando `Disponivel == true`. Portanto, ao cadastrar/publicar ou editar um produto no Gestão Infinity, ele passa a aparecer no catálogo público usando o mesmo banco de dados; ao ocultar/remover, deixa de ser listado. Não há cópia manual de produtos entre aplicações.

## Banco de dados

A migration `20261126010000_ComercialOnline` já está presente na solução. O projeto Gestão Infinity executa `Database.Migrate()` na inicialização. Mantenha `DATABASE_URL` igual nos dois serviços para que compartilhem os mesmos produtos e pedidos.

## Observação sobre pagamentos

O checkout público registra o pedido como `AguardandoPagamento`; a própria tela de compra informa que a cobrança online ainda não está habilitada. No painel, a equipe pode confirmar manualmente um pagamento pendente ou atualizar o status operacional do pedido. Cada alteração de status é registrada em `ControleComercial`.
