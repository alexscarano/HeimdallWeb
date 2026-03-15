# Design: Substituição Globe → World Map

**Data:** 2026-03-15
**Status:** Aprovado

---

## Objetivo

Substituir o componente `GlobeHeimdall` (Three.js WebGL 3D globe) pelo componente `WorldMap` da Aceternity UI (SVG 2D flat map com linhas animadas), mantendo posicionamento, cores e comportamento responsivo iguais.

---

## Motivação

O Aceternity World Map é mais leve (sem `three-globe`/WebGL), usa SVG com `motion` para animação, e está alinhado com a stack de componentes Aceternity já em uso no projeto.

---

## Arquivos Afetados

| Arquivo | Ação |
|---------|------|
| `src/components/ui/world-map.tsx` | **Criado** — componente Aceternity WorldMap com `bg-transparent` |
| `src/components/ui/globe-heimdall.tsx` | **Atualizado** — troca `World` (ThreeGlobe) por `WorldMap` |
| `src/components/ui/globe.tsx` | **Deletado** — não mais necessário |
| `src/data/globe.json` | **Deletado** — só era usado por `globe.tsx` |
| `package.json` | **Atualizado** — adiciona `dotted-map`; remove `three-globe`, `@react-three/fiber`, `@react-three/drei` |

**Nota:** `three` permanece em `package.json` — ainda é usado por `webgl-background.tsx` e `hero-vortex.tsx`.

---

## Dependências

- `dotted-map` — nova, a instalar
- `motion` — já presente (`^12.36.0`); o componente importa de `motion/react`
- `three-globe`, `@react-three/fiber`, `@react-three/drei` — a remover (exclusivos de `globe.tsx`)

---

## Especificação do WorldMap

### Posicionamento

Igual ao globo atual em `globe-heimdall.tsx`:
- `absolute inset-0 z-0 pointer-events-none w-full h-full`
- `hidden lg:block` (oculto em mobile)
- CSS mask com fade vertical:
  ```
  linear-gradient(to bottom, transparent 0%, black 10%, black 85%, transparent 100%)
  ```

### Cores (tema-aware)

| Modo | lineColor |
|------|-----------|
| Dark | `#818cf8` (indigo-400) |
| Light | `#C2410C` (orange-700) |

**Nota intencional:** O globo atual alterna entre dois tons (c1/c2) por arco. O WorldMap usa um único `lineColor` por tema. Esta é uma simplificação proposital — o resultado visual é homogêneo, o que é adequado para o uso como background.

### Transparência

O componente nativo tem `dark:bg-black bg-white` — substituir por `bg-transparent` para que o background do hero apareça por trás.

### Dots (conexões)

Os 30 pares de cidades do `buildArcs()` atual convertidos para o formato WorldMap:

```ts
{ start: { lat: number, lng: number }, end: { lat: number, lng: number } }
```

A função `buildArcs()` e os campos `color`, `arcAlt`, `order` são removidos. Apenas `startLat/startLng/endLat/endLng` são preservados.

### SSR / hydration

O WorldMap usa apenas SVG + `motion/react` — não requer WebGL, portanto **não precisa de `dynamic` com `ssr: false`**. O guard `mounted` state é suficiente para evitar mismatch de tema no primeiro render.

### Prop `key` no tema

O atual `<World key={resolvedTheme ?? 'dark'} .../>` força remount ao trocar tema. Com `WorldMap`, o `lineColor` é passado como prop e React re-renderiza reativamente — **o `key` prop é intencionalmente omitido**.

### Comportamento responsivo / acessibilidade

- Mantém `hidden lg:block` (não renderiza em mobile)
- Mantém verificação de `prefers-reduced-motion` — retorna `null` se ativo
- Mantém `mounted` state para SSR safety

---

## Estrutura do `globe-heimdall.tsx` após mudança

```tsx
'use client';
import { useEffect, useState } from 'react';
import { useTheme } from 'next-themes';
import WorldMap from './world-map'; // static import (SSR safe, SVG only)

const DOTS = [
  // 30 pares convertidos de buildArcs()
  { start: { lat: -23.55, lng: -46.63 }, end: { lat: 40.71, lng: -74.01 } },
  // ...
];

export function GlobeHeimdall() {
  const { resolvedTheme } = useTheme();
  const [mounted, setMounted] = useState(false);
  const [reducedMotion, setReducedMotion] = useState(false);

  useEffect(() => {
    setMounted(true);
    setReducedMotion(window.matchMedia('(prefers-reduced-motion: reduce)').matches);
  }, []);

  if (!mounted || reducedMotion) return null;

  const isDark = resolvedTheme !== 'light';
  const lineColor = isDark ? '#818cf8' : '#C2410C';

  return (
    <div
      className="hidden lg:block absolute inset-0 z-0 pointer-events-none w-full h-full"
      style={{ maskImage: 'linear-gradient(to bottom, transparent 0%, black 10%, black 85%, transparent 100%)',
               WebkitMaskImage: 'linear-gradient(to bottom, transparent 0%, black 10%, black 85%, transparent 100%)' }}
    >
      <WorldMap dots={DOTS} lineColor={lineColor} />
    </div>
  );
}
```

---

## Não incluído

- Nenhuma mudança em `page.tsx` — `<GlobeHeimdall />` continua sendo usado da mesma forma
- Nenhuma mudança em `three` (pacote base) — ainda usado por `webgl-background.tsx` e `hero-vortex.tsx`
- Nenhuma mudança em outras seções da landing
