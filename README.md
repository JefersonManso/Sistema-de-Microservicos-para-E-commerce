# Sistema de Microserviços para E-commerce

Este projeto implementa uma arquitetura de microserviços para gerenciamento de estoque e vendas em uma plataforma de e-commerce.

## Arquitetura

- **InventoryService**: Gerencia produtos e estoque (porta 5001)
- **SalesService**: Gerencia pedidos de vendas (porta 5002)
- **ApiGateway**: Gateway de API usando Ocelot (porta 5000)
- **Shared**: Biblioteca compartilhada com modelos

## PRD - Documento de Requisitos do Produto

### Visão Geral

Este projeto desenvolve um sistema de microserviços para uma plataforma de e-commerce, focado no gerenciamento eficiente de inventário e processamento de vendas. O sistema permite a administração de produtos, controle de estoque em tempo real e criação de pedidos, com comunicação assíncrona entre serviços para garantir escalabilidade e resiliência.

### Objetivos

- Fornecer uma arquitetura modular e escalável para e-commerce.
- Garantir integridade de dados com validação de estoque antes de vendas.
- Implementar comunicação assíncrona para atualizações de inventário.
- Oferecer APIs seguras com autenticação JWT.
- Facilitar manutenção e expansão através de microserviços independentes.

### Funcionalidades Principais

1. **Gerenciamento de Produtos (InventoryService)**:

   - CRUD completo de produtos (criar, listar, atualizar, deletar).
   - Controle de quantidade em estoque.
   - Validação automática de disponibilidade.

2. **Processamento de Pedidos (SalesService)**:

   - Criação de pedidos com verificação de estoque.
   - Cálculo automático de total baseado no preço do produto.
   - Publicação de mensagens para atualização de estoque via RabbitMQ.

3. **Gateway de API (ApiGateway)**:

   - Roteamento unificado de requisições para os microserviços.
   - Exposição de endpoints centralizados via Ocelot.

4. **Comunicação Assíncrona**:

   - Atualização de estoque em tempo real via mensagens RabbitMQ.
   - Consumo de mensagens pelo InventoryService para sincronização.

5. **Autenticação e Segurança**:
   - JWT Bearer Token para proteção de endpoints.
   - Autorização obrigatória para operações sensíveis.

### Requisitos Não Funcionais

- **Performance**: Respostas rápidas (< 500ms para operações CRUD).
- **Escalabilidade**: Arquitetura de microserviços permite adição de novos serviços (ex: Pagamento, Envio).
- **Confiabilidade**: Validação de estoque evita vendas de produtos indisponíveis.
- **Observabilidade**: Logs estruturados com Serilog para monitoramento.
- **Testabilidade**: Cobertura de testes unitários para operações críticas.

### Stakeholders

- **Desenvolvedores**: Responsáveis pela manutenção e expansão do sistema.
- **Administradores de E-commerce**: Usuários finais que gerenciam produtos e pedidos via APIs.
- **Clientes**: Indiretamente beneficiados pela eficiência do sistema.

### Critérios de Aceitação

- Todos os endpoints devem retornar códigos HTTP apropriados (200, 400, 404, etc.).
- Validação de estoque deve impedir pedidos com quantidade insuficiente.
- Mensagens RabbitMQ devem ser processadas corretamente para atualização de estoque.
- Testes unitários devem passar com cobertura mínima de 80%.
- Logs devem registrar transações e erros para auditoria.

## Tecnologias

- .NET 8.0
- Entity Framework Core com SQL Server
- RabbitMQ para comunicação assíncrona
- JWT para autenticação
- Ocelot para API Gateway
- xUnit e Moq para testes unitários
- Serilog para logs estruturados

## Configuração

1. Instalar SQL Server LocalDB ou configurar connection string.
2. Instalar RabbitMQ e garantir que esteja rodando na porta padrão.
3. Executar migrations: `dotnet ef database update` em cada serviço.

## Executando

1. Iniciar InventoryService: `dotnet run` na pasta InventoryService
2. Iniciar SalesService: `dotnet run` na pasta SalesService
3. Iniciar ApiGateway: `dotnet run` na pasta ApiGateway

Acesse o Swagger em http://localhost:5000/swagger para testar via gateway.

## Testes

Testes unitários foram implementados para operações CRUD de produtos e criação de pedidos.

Para executar testes:

- InventoryService: `dotnet test` na pasta InventoryService.Tests
- SalesService: `dotnet test` na pasta SalesService.Tests

## Monitoramento e Logs

Logs estruturados usando Serilog são configurados em todos os serviços:

- InventoryService: Logs para adição, atualização e exclusão de produtos.
- SalesService: Logs para criação de pedidos, avisos de estoque insuficiente.
- ApiGateway: Logs de requisições via Ocelot.

Logs são exibidos no console. Para produção, configure sinks adicionais (arquivo, ELK, etc.).

## Endpoints

### Inventory

- GET /inventory/api/products - Listar produtos
- POST /inventory/api/products - Criar produto
- PUT /inventory/api/products/{id} - Atualizar produto
- DELETE /inventory/api/products/{id} - Deletar produto

### Sales

- GET /sales/api/orders - Listar pedidos
- POST /sales/api/orders - Criar pedido

## Autenticação

Todos os endpoints requerem JWT. Configure chave no appsettings.json.

## Comunicação

- SalesService publica mensagens para "stock_update" queue via RabbitMQ.
- InventoryService consome e atualiza estoque.

## Escalabilidade

Para adicionar novos microserviços:

1. Criar novo projeto ASP.NET Core Minimal API.
2. Adicionar referência ao projeto Shared.
3. Configurar DbContext se necessário, com migrations.
4. Implementar endpoints e lógica.
5. Adicionar rota no ocelot.json do ApiGateway (ex: "/payment/\*" -> "http://localhost:5003").
6. Configurar logs com Serilog.
7. Adicionar testes unitários.

Exemplos de novos serviços: PaymentService, ShippingService, NotificationService.

## Melhorias Futuras

- Implementar consumer para atualização de estoque (já implementado).
- Validação de estoque antes de venda (já implementado).
- Autenticação completa com usuários.
- Rastreamento distribuído com CorrelationId.
- Containerização com Docker.
- Orquestração com Kubernetes.

## Screenshots

Para visualizar o projeto em ação, aqui estão alguns screenshots ilustrativos. Crie uma pasta `images` na raiz do projeto e adicione as imagens capturadas.

- **Arquitetura dos Microserviços**:  
  ![Arquitetura](images/arquitetura.png)  
  Diagrama mostrando a interação entre InventoryService, SalesService, ApiGateway e RabbitMQ.

- **Swagger da API Gateway**:  
  ![Swagger](images/swagger.png)  
  Interface do Swagger em http://localhost:5000/swagger, mostrando os endpoints disponíveis para Inventory e Sales.

- **Logs Estruturados no Console**:  
  ![Logs Console](images/logs_console.png)  
  Exemplo de logs no terminal do InventoryService, exibindo transações de produtos e mensagens do RabbitMQ.

- **Testes Unitários Executando**:  
  ![Testes](images/testes.png)  
  Resultado da execução de `dotnet test` no InventoryService.Tests, mostrando testes passando.

Para capturar essas imagens:

1. Execute o projeto conforme a seção "Executando".
2. Abra o navegador em http://localhost:5000/swagger para o screenshot do Swagger.
3. Monitore os terminais para logs durante operações (ex: criar um produto).
4. Execute `dotnet test` em uma pasta de testes para o screenshot dos testes.
5. Para o diagrama de arquitetura, use ferramentas como Draw.io ou Lucidchart para criar um simples.
