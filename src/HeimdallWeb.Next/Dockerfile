# =============================================================================
# Dockerfile — HeimdallWeb.Next (Next.js 16 + React 19)
# =============================================================================
#
# Estratégia multi-stage:
#   Stage "deps"    : Instala dependências npm (cache isolado — só roda quando
#                     package.json ou package-lock.json mudam)
#   Stage "dev"     : Ambiente de desenvolvimento com next dev (hot reload /
#                     Fast Refresh)
#   Stage "builder" : Build de produção (next build)
#   Stage "runner"  : Imagem mínima para produção com Next.js standalone
#
# Segurança:
#   - Processos rodam como usuário não-privilegiado "nextjs" (UID 1001)
#   - Stage de produção usa node:20-alpine (imagem mínima)
#   - NEXT_PUBLIC_* são as únicas variáveis expostas ao browser
# =============================================================================

# ─── Stage: deps ─────────────────────────────────────────────────────────────
# Instala dependências em camada dedicada para maximizar cache.
# Esta camada só é invalidada quando package.json ou package-lock.json mudam.
FROM node:20-alpine AS deps

# libc6-compat é necessário para alguns módulos nativos node em Alpine
RUN apk add --no-cache libc6-compat

WORKDIR /app

# Copia apenas os manifestos de dependência
COPY package.json package-lock.json* ./

# Instala dependências usando ci para reproducibilidade exata
# --frozen-lockfile garante que o package-lock.json não seja modificado
RUN npm ci --frozen-lockfile

# ─── Stage: dev ──────────────────────────────────────────────────────────────
# Ambiente de desenvolvimento com hot reload (Fast Refresh).
# O código-fonte é montado via bind mount no docker-compose.
# O node_modules é servido do volume Docker (não sobrescrito pelo bind mount).
FROM node:20-alpine AS dev

RUN apk add --no-cache libc6-compat

# Cria usuário não-privilegiado
RUN addgroup --system --gid 1001 nodejs \
    && adduser --system --uid 1001 nextjs

WORKDIR /app

# Copia dependências instaladas no stage deps
COPY --from=deps --chown=nextjs:nodejs /app/node_modules ./node_modules

# Define variáveis de ambiente para desenvolvimento
ENV NODE_ENV=development
ENV NEXT_TELEMETRY_DISABLED=1
ENV PORT=3000
ENV HOSTNAME=0.0.0.0

USER nextjs

EXPOSE 3000

# next dev inicia o servidor de desenvolvimento com Fast Refresh
# O código-fonte chega via bind mount definido no docker-compose.override.yml
CMD ["./node_modules/.bin/next", "dev", "--hostname", "0.0.0.0", "--port", "3000"]

# ─── Stage: builder ──────────────────────────────────────────────────────────
# Compila a aplicação Next.js em modo produção (next build).
# Requer que o código-fonte seja copiado inteiro aqui.
FROM node:20-alpine AS builder

RUN apk add --no-cache libc6-compat

WORKDIR /app

# Copia dependências
COPY --from=deps /app/node_modules ./node_modules

# Copia o código-fonte completo
COPY . .

# Variáveis de build — substituir pelos valores reais no pipeline CI/CD
# ou passar via --build-arg
ARG NEXT_PUBLIC_API_URL=http://localhost:5110
ENV NEXT_PUBLIC_API_URL=${NEXT_PUBLIC_API_URL}
ENV NEXT_TELEMETRY_DISABLED=1

# Configura output standalone para imagem de produção mínima
# (requer output: 'standalone' no next.config.ts — adicionar se necessário)
RUN npm run build

# ─── Stage: runner ───────────────────────────────────────────────────────────
# Imagem de produção mínima — apenas o runtime Node.js e os artefatos do build.
FROM node:20-alpine AS runner

RUN apk add --no-cache libc6-compat

LABEL maintainer="HeimdallWeb Team"
LABEL description="HeimdallWeb Frontend — Next.js 16"

WORKDIR /app

ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1
ENV PORT=3000
ENV HOSTNAME=0.0.0.0

# Cria usuário não-privilegiado para produção
RUN addgroup --system --gid 1001 nodejs \
    && adduser --system --uid 1001 nextjs

# Copia apenas os artefatos necessários para produção
COPY --from=builder --chown=nextjs:nodejs /app/public ./public
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static

# Para output standalone (recomendado em produção):
# COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./

# Para output padrão (sem standalone configurado):
COPY --from=builder --chown=nextjs:nodejs /app/.next ./.next
COPY --from=builder /app/node_modules ./node_modules
COPY --from=builder /app/package.json ./package.json

USER nextjs

EXPOSE 3000

CMD ["./node_modules/.bin/next", "start", "--hostname", "0.0.0.0", "--port", "3000"]
