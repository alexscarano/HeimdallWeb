# =============================================================================
# Dockerfile — HeimdallWeb.WebApi (.NET 10 Minimal APIs)
# =============================================================================
#
# Estratégia multi-stage:
#   Stage "restore"  : Restaura dependências NuGet em camada separada (cache)
#   Stage "dev"      : Ambiente de desenvolvimento com dotnet watch (hot reload)
#   Stage "build"    : Compila e publica em modo Release
#   Stage "runtime"  : Imagem mínima para produção (sem SDK)
#
# Segurança:
#   - Processos rodam como usuário não-privilegiado "appuser" (UID 1001)
#   - Stage de produção usa mcr.microsoft.com/dotnet/aspnet (sem SDK)
#   - Nenhuma chave/segredo está hardcoded — injetados via variáveis de ambiente
# =============================================================================

# ─── Stage: restore ──────────────────────────────────────────────────────────
# Copia apenas os arquivos .csproj para maximizar cache do Docker.
# O restore só roda novamente quando as dependências mudam.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore

WORKDIR /workspace

# Copia os arquivos de projeto (.csproj) de toda a solution (exceto HeimdallWebOld)
# A ordem reflete a árvore de dependências: Domain → Contracts → Application → Infrastructure → WebApi
COPY src/HeimdallWeb.Domain/HeimdallWeb.Domain.csproj             src/HeimdallWeb.Domain/
COPY src/HeimdallWeb.Contracts/HeimdallWeb.Contracts.csproj       src/HeimdallWeb.Contracts/
COPY src/HeimdallWeb.Application/HeimdallWeb.Application.csproj   src/HeimdallWeb.Application/
COPY src/HeimdallWeb.Infrastructure/HeimdallWeb.Infrastructure.csproj src/HeimdallWeb.Infrastructure/
COPY src/HeimdallWeb.WebApi/HeimdallWeb.WebApi.csproj             src/HeimdallWeb.WebApi/

# Copia DLLs externas referenciadas pelo projeto (ASHelpers.dll, etc.)
# HintPath em Application.csproj: ..\..\dlls\ASHelpers.dll → /workspace/dlls/
COPY dlls/ dlls/

# Restaura dependências de todos os projetos da árvore do WebApi
RUN dotnet restore src/HeimdallWeb.WebApi/HeimdallWeb.WebApi.csproj

# ─── Stage: dev ──────────────────────────────────────────────────────────────
# Ambiente de desenvolvimento com hot reload (dotnet watch).
# O código-fonte é montado via bind mount no docker-compose — não é copiado aqui.
FROM restore AS dev

# Instala ferramentas necessárias para o entrypoint:
#   - pg_isready (cliente PostgreSQL para healthcheck)
#   - dotnet-ef (EF Core CLI para migrations)
RUN apt-get update \
    && apt-get install -y --no-install-recommends postgresql-client \
    && rm -rf /var/lib/apt/lists/* \
    && dotnet tool install --global dotnet-ef \
    && ln -s /root/.dotnet/tools/dotnet-ef /usr/local/bin/dotnet-ef

# Cria usuário não-privilegiado — o dotnet watch roda como appuser
# Nota: em dev o /root/.nuget precisa ser acessível para o restore funcionar
# durante o watch, portanto mantemos o cache em volume gerenciado pelo compose
RUN groupadd --gid 1001 appgroup \
    && useradd --uid 1001 --gid appgroup --shell /bin/bash --create-home appuser

WORKDIR /workspace

# Copia o script de entrypoint e define permissão de execução
COPY src/HeimdallWeb.WebApi/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

EXPOSE 5110

# O entrypoint roda como root para aplicar migrations e depois o watch
# inicia como appuser via exec — aceitável em ambiente de desenvolvimento.
# Em produção (stage runtime), o processo roda inteiramente como appuser.
ENTRYPOINT ["/entrypoint.sh"]

# ─── Stage: build ────────────────────────────────────────────────────────────
# Compila e publica o projeto em modo Release para produção.
FROM restore AS build

# Copia o código-fonte completo (exceto HeimdallWebOld)
COPY src/ src/

RUN dotnet publish src/HeimdallWeb.WebApi/HeimdallWeb.WebApi.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

# ─── Stage: runtime ──────────────────────────────────────────────────────────
# Imagem de produção mínima — apenas o ASP.NET runtime, sem SDK.
# Superfície de ataque drasticamente reduzida em relação ao stage build.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

# Metadados da imagem
LABEL maintainer="HeimdallWeb Team"
LABEL description="HeimdallWeb WebApi — .NET 10 Minimal APIs"

WORKDIR /app

# Cria usuário não-privilegiado para rodar o processo em produção
RUN groupadd --gid 1001 appgroup \
    && useradd --uid 1001 --gid appgroup --no-create-home --shell /bin/false appuser

# Copia apenas o artefato publicado do stage build
COPY --from=build --chown=appuser:appgroup /app/publish .

# Cria diretório para uploads com as permissões corretas
RUN mkdir -p /app/wwwroot/uploads/profiles \
    && chown -R appuser:appgroup /app/wwwroot

USER appuser

EXPOSE 5110

ENTRYPOINT ["dotnet", "HeimdallWeb.WebApi.dll"]
