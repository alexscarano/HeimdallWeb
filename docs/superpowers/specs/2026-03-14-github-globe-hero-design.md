# Design Spec: GitHub Globe — Hero Landing Page

**Date:** 2026-03-14
**Status:** Approved

---

## Overview

Replace the current `HeroVortex` (WebGL helical cone spiral) with an Aceternity GitHub Globe as the hero background animation. The globe renders as an absolute-positioned background layer, exactly where `HeroVortex` sits today. The existing hero layout (2-column grid `lg:grid-cols-2`: text/CTAs left, `ScanPreviewCard` right) is preserved untouched on top (`z-10`, already in place).

`HeroVortex` is **dereferenced** (removed from `page.tsx`), not deleted — the file remains in the codebase.

---

## Installation

```bash
# 1. Add Aceternity Globe via shadcn registry
npx shadcn@latest add @aceternity/globe

# 2. Download globe GeoJSON data and commit it to the repo
# Place at: data/globe.json (Aceternity convention)
# Source: https://assets.aceternity.com/globe.json
# IMPORTANT: download and commit — do not reference the CDN URL at runtime
```

The Aceternity Globe component includes an internal `next/dynamic` wrapper with `ssr: false`. No additional SSR guard is needed at the consumer level. If the installed version does not include this guard, wrap `GlobeHeimdall` in `dynamic(() => import(...), { ssr: false })` at the call site in `page.tsx`.

---

## New Component: `globe-heimdall.tsx`

**Path:** `src/HeimdallWeb.Next/src/components/ui/globe-heimdall.tsx`

**Directive:** `'use client'` — required at the top of the file (uses `useTheme`, `useEffect`, `useState`).

**Aceternity export used:** `World` (named export from the installed globe component file).

### Responsibilities

- Read `resolvedTheme` from `next-themes`
- Build a `GlobeConfig` + `Position[]` based on theme
- Render `<World config={globeConfig} data={arcs} key={resolvedTheme ?? 'dark'} />`
- Wrap in a container with full absolute positioning + CSS mask + mobile hide

### SSR / Hydration safety

`resolvedTheme` is `undefined` on the first SSR render. Default to dark config:

```tsx
const isDark = resolvedTheme !== 'light'; // undefined → true → dark config
```

### `prefers-reduced-motion`

Checked once at mount via `useEffect`. This is a static snapshot (not subscribed to changes) — intentional trade-off for simplicity:

```tsx
const [mounted, setMounted] = useState(false);
const [reducedMotion, setReducedMotion] = useState(false);

useEffect(() => {
  setMounted(true);
  setReducedMotion(window.matchMedia('(prefers-reduced-motion: reduce)').matches);
}, []);

if (!mounted || reducedMotion) return null;
```

### Mobile breakpoint

The hero grid activates at `lg` (`1024px`, from `lg:grid-cols-2` in `page.tsx`). The globe is hidden below `lg` to match:

```tsx
<div className="hidden lg:block absolute inset-0 z-0 pointer-events-none" style={maskStyle}>
```

### Container sizing & positioning

The container replaces `HeroVortex` exactly. Explicit class list:

```
hidden lg:block absolute inset-0 z-0 pointer-events-none w-full h-full
```

### Theme re-render strategy

`key={resolvedTheme ?? 'dark'}` is passed to `<World>` to force a full remount on theme change. This destroys and recreates the WebGL context — intentional, as the Aceternity Globe does not support hot config swaps. The brief flash during theme toggle is acceptable.

### CSS Mask gradient

```tsx
const maskStyle: React.CSSProperties = {
  maskImage: 'linear-gradient(to bottom, transparent 0%, black 10%, black 85%, transparent 100%)',
  WebkitMaskImage: 'linear-gradient(to bottom, transparent 0%, black 10%, black 85%, transparent 100%)',
};
```

---

## GlobeConfig — Dark (indigo)

| Parameter | Value |
|-----------|-------|
| `globeColor` | `#1a1a2e` |
| `polygonColor` | `#818cf8` (indigo-400) |
| `emissive` | `#2d2d5e` |
| `emissiveIntensity` | `0.1` |
| `shininess` | `0.9` |
| `showAtmosphere` | `true` |
| `atmosphereColor` | `#818cf8` |
| `atmosphereAltitude` | `0.1` |
| `ambientLight` | `#38bdf8` |
| `directionalLeftLight` | `#ffffff` |
| `directionalTopLight` | `#ffffff` |
| `pointLight` | `#818cf8` |

## GlobeConfig — Light (orange)

| Parameter | Value |
|-----------|-------|
| `globeColor` | `#f0ebe4` |
| `polygonColor` | `#C2410C` (orange-700) |
| `emissive` | `#fff3ed` |
| `emissiveIntensity` | `0.1` |
| `shininess` | `0.9` |
| `showAtmosphere` | `true` |
| `atmosphereColor` | `#fb923c` |
| `atmosphereAltitude` | `0.1` |
| `ambientLight` | `#fed7aa` |
| `directionalLeftLight` | `#ffffff` |
| `directionalTopLight` | `#ffffff` |
| `pointLight` | `#C2410C` |

## Shared GlobeConfig

| Parameter | Value |
|-----------|-------|
| `pointSize` | `2` |
| `arcTime` | `1000` |
| `arcLength` | `0.9` |
| `rings` | `3` |
| `maxRings` | `3` |
| `autoRotate` | `true` |
| `autoRotateSpeed` | `0.5` |

---

## Arc Data Type

Each arc follows the Aceternity `Position` interface:

```ts
interface Position {
  order: number;
  startLat: number;
  startLng: number;
  endLat: number;
  endLng: number;
  arcAlt: number;  // 0.1–0.4
  color: string;
}
```

Arc colors:
- **Dark theme:** alternating `#818cf8` and `#6366f1`
- **Light theme:** alternating `#C2410C` and `#ea580c`

`arcAlt` cycles: `0.1, 0.2, 0.3, 0.4, 0.15, 0.25, ...` for visual depth.

## 30 Arc Pairs

| # | Start city | startLat | startLng | End city | endLat | endLng | arcAlt |
|---|-----------|----------|----------|----------|--------|--------|--------|
| 1 | São Paulo | -23.55 | -46.63 | New York | 40.71 | -74.01 | 0.3 |
| 2 | New York | 40.71 | -74.01 | London | 51.51 | -0.13 | 0.4 |
| 3 | London | 51.51 | -0.13 | Frankfurt | 50.11 | 8.68 | 0.1 |
| 4 | Frankfurt | 50.11 | 8.68 | Singapore | 1.35 | 103.82 | 0.4 |
| 5 | Singapore | 1.35 | 103.82 | Tokyo | 35.68 | 139.69 | 0.2 |
| 6 | Tokyo | 35.68 | 139.69 | San Francisco | 37.77 | -122.42 | 0.4 |
| 7 | San Francisco | 37.77 | -122.42 | São Paulo | -23.55 | -46.63 | 0.3 |
| 8 | London | 51.51 | -0.13 | Lagos | 6.52 | 3.38 | 0.3 |
| 9 | Singapore | 1.35 | 103.82 | Sydney | -33.87 | 151.21 | 0.2 |
| 10 | New York | 40.71 | -74.01 | Toronto | 43.65 | -79.38 | 0.1 |
| 11 | Frankfurt | 50.11 | 8.68 | Amsterdam | 52.37 | 4.90 | 0.1 |
| 12 | Amsterdam | 52.37 | 4.90 | Mumbai | 19.08 | 72.88 | 0.35 |
| 13 | Mumbai | 19.08 | 72.88 | Singapore | 1.35 | 103.82 | 0.2 |
| 14 | Tokyo | 35.68 | 139.69 | Seoul | 37.57 | 126.98 | 0.1 |
| 15 | Seoul | 37.57 | 126.98 | Hong Kong | 22.32 | 114.17 | 0.1 |
| 16 | Hong Kong | 22.32 | 114.17 | Sydney | -33.87 | 151.21 | 0.3 |
| 17 | São Paulo | -23.55 | -46.63 | Buenos Aires | -34.60 | -58.38 | 0.1 |
| 18 | Buenos Aires | -34.60 | -58.38 | London | 51.51 | -0.13 | 0.4 |
| 19 | San Francisco | 37.77 | -122.42 | Tokyo | 35.68 | 139.69 | 0.4 |
| 20 | London | 51.51 | -0.13 | Moscow | 55.75 | 37.62 | 0.15 |
| 21 | Moscow | 55.75 | 37.62 | Mumbai | 19.08 | 72.88 | 0.25 |
| 22 | New York | 40.71 | -74.01 | Miami | 25.77 | -80.19 | 0.1 |
| 23 | Miami | 25.77 | -80.19 | São Paulo | -23.55 | -46.63 | 0.2 |
| 24 | Frankfurt | 50.11 | 8.68 | Dubai | 25.20 | 55.27 | 0.25 |
| 25 | Dubai | 25.20 | 55.27 | Singapore | 1.35 | 103.82 | 0.3 |
| 26 | Lagos | 6.52 | 3.38 | Johannesburg | -26.20 | 28.04 | 0.15 |
| 27 | Johannesburg | -26.20 | 28.04 | Mumbai | 19.08 | 72.88 | 0.35 |
| 28 | Seattle | 47.61 | -122.33 | Tokyo | 35.68 | 139.69 | 0.4 |
| 29 | Paris | 48.86 | 2.35 | New York | 40.71 | -74.01 | 0.35 |
| 30 | Chicago | 41.88 | -87.63 | London | 51.51 | -0.13 | 0.3 |

---

## Hero Layout Changes (`page.tsx`)

1. Remove `import { HeroVortex } from "@/components/ui/hero-vortex"`
2. Add `import { GlobeHeimdall } from "@/components/ui/globe-heimdall"`
3. Replace:
   ```tsx
   <HeroVortex className="absolute inset-0 z-0 w-full h-full pointer-events-none" />
   ```
   with:
   ```tsx
   <GlobeHeimdall />
   ```
4. Hero content already has `relative z-10` on its container — no changes needed.

---

## WebGL Compatibility

`HeroVortex` (Three.js WebGL) is removed, so no WebGL context conflict with the Aceternity Globe. `WebGLBackground` (node network) is a fixed-position component not present on the public landing page — no conflict.

---

## Files Touched

| File | Action |
|------|--------|
| `src/components/ui/globe-heimdall.tsx` | **Create** |
| `src/app/(public)/page.tsx` | **Edit** — swap import + JSX |
| `data/globe.json` | **Add** — GeoJSON downloaded from Aceternity assets, committed to repo |
| `src/components/ui/hero-vortex.tsx` | **No change** — file kept, only dereferenced |

---

## Out of Scope

- No changes to any other section of the landing page
- No changes to `HeroVortex` implementation
- No changes to `WebGLBackground`
- No backend changes
