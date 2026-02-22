# Heimdall Architecture Reference

## Padrões Atuais (CQRS Light)
- **Domain**: Entidades (`ScanHistory`, `Finding`, `Technology`, `User`) e Value Objects.
- **Application**: Commands, Queries, Handlers e DTOs (Extensions para ToDto/ToDomain).
- **Infrastructure**: EF Core e Integrações externas (Scanners).
- **WebAPI**: Endpoints REST que invocam Handlers.

## Regras de Implementação
- **Validação**: FluentValidation nos comandos.
- **Transações**: Padrão IUnitOfWork.
- **Erros**: Exceções customizadas (`NotFoundException`, `ValidationException`).
- **Migração**: Status atual indica foco em migrar os ~17 handlers restantes do legado.
