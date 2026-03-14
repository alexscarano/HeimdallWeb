# Animações React Bits + Magic UI — Design Spec

**Data:** 2026-03-14
**Status:** Aprovado

---

## Contexto

HeimdallWeb é uma plataforma de segurança web com estética cyber/dark. O objetivo é adicionar animações em 5 áreas principais usando React Bits e Magic UI, com Framer Motion instalado pontualmente (abordagem híbrida: landing e scan mais dramáticos, app interno mais sutil).

**Stack existente relevante:** Next.js 15, React 19, TailwindCSS v4, Three.js (já instalado), `tw-animate-css` (já instalado), shadcn/ui.

---

## Dependências Novas

| Pacote | Motivo |
|---|---|
| `framer-motion` | Base para Magic UI e React Bits (BlurFade, AnimatedList, etc.) |
| `motion` | Alias moderno do framer-motion (Magic UI usa esse nome) |

> React Bits e Magic UI são copiados como código-fonte (copy/paste via CLI ou manual), não instalados como pacotes npm. Framer Motion é a única dependência nova real.

---

## 1. Logo — `LogoMark` Component

**Arquivo:** `src/components/ui/logo-mark.tsx`
**Usado em:** Sidebar (`components/layout/sidebar.tsx`) e potencialmente landing page navbar.

### Especificação

- Novo componente `LogoMark` wrapping `MetallicPaint` do React Bits
- Aceita prop `size?: number` (default: 28 para sidebar, 48 para uso maior)
- Aceita prop `src?: string` — quando o logo real estiver disponível, basta passar a URL da imagem; fallback é um SVG de escudo gerado inline
- Parâmetros MetallicPaint fixos:
  - `speed: 0.13`
  - `liquid: 0.21`
  - `chromaticSpread: 0.9`
  - `mouseAnimation: true`
  - `blur: 0.029`
  - `tintColor: "#7e37a4"`
  - `brightness: 1.2`

### Integração na Sidebar

Substitui o bloco atual:
```tsx
<div className="flex h-7 w-7 shrink-0 items-center justify-center rounded-lg bg-accent-primary-subtle">
  <Shield className="h-4 w-4 text-accent-primary" />
</div>
```
Por:
```tsx
<LogoMark size={28} />
```

---

## 2. Landing Page

**Arquivo:** `src/app/(public)/page.tsx`
**Intensidade:** Alta (primeira impressão).

| Elemento | Componente | Lib | Detalhe |
|---|---|---|---|
| Título hero | `BlurText` | React Bits | Animação por palavra, stagger 100ms |
| Feature cards (grid 3x2) | `SpotlightCard` | React Bits | Spotlight que segue cursor no hover |
| Scan preview card (hero direito) | `BorderBeam` | Magic UI | Raio girando na borda, cor `indigo-400` dark / `emerald-600` light |
| Botão CTA primário | `ShimmerButton` | Magic UI | Substitui o `<Button>` primário do hero |
| Scroll de seções (Features, Score, FAQ, CTA) | `BlurFade` | Magic UI | Entrada viewport com `once: true` |

---

## 3. Login & Cadastro

**Arquivos:** `src/app/(auth)/login/page.tsx`, `src/app/(auth)/register/page.tsx`

| Elemento | Componente | Lib | Detalhe |
|---|---|---|---|
| Card principal do formulário | `MagicCard` | Magic UI | Spotlight gradient no hover |

---

## 4. Dashboard

**Arquivo:** `src/app/(app)/dashboard/user/page.tsx`
**Intensidade:** Sutil.

| Elemento | Componente | Lib | Detalhe |
|---|---|---|---|
| Valores nos 4 metric cards | `CountUp` (gradient) | React Bits | Conta de 0 ao valor real ao montar |
| Valores nos 5 severity cards | `CountUp` (gradient) | React Bits | Mesma animação, consistência visual |
| Entrada dos metric cards | `BlurFade` stagger | Magic UI | Delay incremental de 80ms entre cards |
| Entrada dos severity cards | `BlurFade` stagger | Magic UI | Delay incremental de 50ms |
| Entrada dos chart cards | `BlurFade` stagger | Magic UI | 2 cards, delay 100ms |
| Charts | nativa Recharts | — | Sem mudança |

---

## 5. Scan / Resultado

**Arquivos:** `src/app/(app)/scan/page.tsx`, `src/components/scan/scanner-result-cards.tsx`, `src/components/scan/score-gauge.tsx`
**Intensidade:** Misto (form sutil, resultado impactante).

| Elemento | Componente | Lib | Detalhe |
|---|---|---|---|
| Card do scan form | `TiltedCard` | React Bits | Perspectiva 3D leve no hover |
| Botão "Iniciar Scan" | `PulsatingButton` | Magic UI | Pulsa enquanto idle, para ao submeter |
| Número do score gauge | `CountUp` gradient | React Bits | Conta de 0 ao score real |
| Score gauge (entrada na página) | `BlurFade` | Magic UI | Envolve o componente inteiro |
| Result cards por scanner | `AnimatedList` | React Bits | Cards entram em sequência ao carregar |
| Cards Critical/High findings | `MagicCard` | Magic UI | Spotlight gradient, destaca findings graves |
| Scan loading | existente | — | Sem mudança (já animado) |

---

## 6. Sidebar & Navegação

**Arquivo:** `src/components/layout/sidebar.tsx`
**Intensidade:** Sutil — CSS puro, zero dependências.

| Elemento | Animação | Lib |
|---|---|---|
| Indicador de rota ativa (barra vertical) | `transition-all duration-200` no translate | CSS |
| Hover nos nav items | `translate-x-1 transition-transform` | CSS |
| Botão colapso/expand (ícone) | `rotate-180 transition-transform` | CSS |
| Largura da sidebar | já existe `transition-[width] duration-300` | CSS |

---

## 7. Histórico

**Arquivos:** `src/app/(app)/history/page.tsx`, `src/app/(app)/history/[id]/page.tsx`
**Intensidade:** Sutil.

| Elemento | Componente | Lib | Detalhe |
|---|---|---|---|
| Linhas da tabela de scans | `BlurFade` stagger | Magic UI | Stagger 30ms por linha ao carregar |
| Risk cards na página de detalhe | `BlurFade` stagger | Magic UI | Entrada dos cards ao montar |

---

## Resumo de Componentes por Lib

### React Bits (copy/paste)
- `MetallicPaint` — logo
- `BlurText` — título hero
- `SpotlightCard` — feature cards landing
- `CountUp` (gradient) — score gauge, metric cards, severity cards
- `AnimatedList` — result cards do scan
- `TiltedCard` — scan form card

### Magic UI (copy/paste via CLI)
- `BorderBeam` — scan preview card no hero
- `ShimmerButton` — CTA hero
- `BlurFade` — entradas de seção, cards, tabela, risk cards
- `MagicCard` — login, cadastro, findings Critical/High
- `PulsatingButton` — botão iniciar scan

### Framer Motion
- Dependência base para Magic UI e React Bits animated. Instalar como `framer-motion`.

### CSS/Tailwind
- Sidebar: transições de translate, rotate, width (sem nova dependência).

---

## Considerações de Performance

- `BlurFade` com `once: true` — anima apenas na primeira vez que o elemento entra no viewport, não re-anima em re-renders.
- `AnimatedList` na página de resultado — renderizar apenas os cards visíveis se o número de scanners crescer.
- `MetallicPaint` (WebGL canvas) — leve; funciona bem em 28px mas o efeito `mouseAnimation` fica mais rico em tamanhos maiores.
- Nenhuma animação bloqueia interação — todas são `pointer-events-none` ou decorativas.
