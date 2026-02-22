# Landing Page WebGL — Design Document

**Data:** 2026-02-22
**Branch:** migracao
**Status:** Implementado

---

## Objetivo

Elevar o nível visual da landing page com efeitos WebGL usando Three.js, substituindo o background Canvas 2D simples, mantendo performance, acessibilidade e os tokens do design system.

---

## Arquitetura das 3 Camadas

```
① HeroVortex (Three.js WebGL)       → hero section apenas
② WebGLBackground (Three.js WebGL)  → global, substitui particle-background.tsx
③ FeaturesGrid (CSS animado)         → seção de features
```

---

## Decisões de Design

### Paleta de Cores

| Modo | Cor principal | Hex |
|------|--------------|-----|
| Light | Emerald-600 | `#059669` |
| Dark | Indigo-400 | `#818CF8` |

Alinhado ao design system existente: emerald em light mode (CTAs, sucessos), indigo em dark mode.

### WebGLBackground (`webgl-background.tsx`)

- **Substituição direta** do `particle-background.tsx` (Canvas 2D)
- Three.js com `PointsMaterial` para nós e `LineSegments` para conexões
- Opacidade intencional muito baixa (0.04 light / 0.06 dark) — efeito sutil, não distrai do conteúdo
- **Responsive:** desktop=60 partículas, tablet=40, mobile=20 sem linhas
- **Performance:** `powerPreference: 'low-power'`, `pixelRatio` limitado a 2

### HeroVortex (`hero-vortex.tsx`)

3 camadas sobrepostas:

1. **GLSL Shader Glow:** `PlaneGeometry` + `ShaderMaterial` com GLSL custom — cone de brilho saindo de baixo (origem `vec2(0.5, 0.0)`), falloff exponencial com máscara de cone angular. `AdditiveBlending` para blend natural.

2. **Partículas em Hélice Cônica:** 600 partículas (desktop) em espiral paramétrica — ângulo × 8π de espirais, raio crescente de 0 ao máximo, altura decrescente. Animação: rotação angular incremental + drift radial senoidal suave. `sizeAttenuation: true` para perspectiva natural.

3. **Fade CSS:** `mask-image` linear no canvas para fundir suavemente com o conteúdo acima e abaixo.

### Layout do Hero

**Antes:** coluna única, texto centralizado.
**Depois:** grid 2 colunas em desktop (lg+):
- Esquerda: headline + subtítulo + CTAs (text-left)
- Direita: `ScanPreviewCard` — card com ScoreGauge animado + lista de status de scan

O `ScanPreviewCard` é um componente inline em `page.tsx` — não justificava extração para arquivo separado dado que é único nesta página.

### Features Section

`.features-grid-bg` — grade 48×48px com linhas muito sutis (0.04 opacidade), animação de drift diagonal a 20s/ciclo. Complementa o background global sem competir.

---

## Estratégia de Performance e Acessibilidade

| Condição | Comportamento |
|----------|--------------|
| `prefers-reduced-motion: reduce` | Nenhum WebGL inicializado. CSS puro apenas. |
| `width < 768` (mobile) | WebGLBackground: 20 partículas, sem linhas. HeroVortex: só shader glow. |
| `768 ≤ width < 1024` (tablet) | Partículas reduzidas (~50%) |
| `width ≥ 1024` (desktop) | Full effect |
| WebGL não suportado | `try/catch` no `new THREE.WebGLRenderer()` → componente não renderiza |

---

## Arquivos Modificados

| Arquivo | Ação |
|---------|------|
| `src/components/ui/webgl-background.tsx` | **Criado** — substituto do particle-background |
| `src/components/ui/hero-vortex.tsx` | **Criado** — efeito vórtice do hero |
| `src/components/ui/particle-background.tsx` | Mantido (não deletado — pode ser referenciado) |
| `src/app/layout.tsx` | WebGLBackground no lugar de ParticleBackground |
| `src/app/(public)/page.tsx` | Hero 2 cols, ScanPreviewCard, HeroVortex |
| `src/app/globals.css` | bg-hero-gradient atualizado + features-grid-bg novo |
| `package.json` | `three@^0.183.1` + `@types/three@^0.183.1` |

---

## Three.js Version

`three@0.183.x` — versão LTS estável no momento da implementação (fev/2026).
