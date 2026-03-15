# GitHub Globe Hero Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the `HeroVortex` spiral animation with an Aceternity GitHub Globe as the hero section background, dual-themed (indigo dark / orange light).

**Architecture:** Install the Aceternity Globe component via shadcn CLI, create a `GlobeHeimdall` wrapper that reads `resolvedTheme` and builds the appropriate `GlobeConfig` + arc data, then swap it into `page.tsx` in place of `HeroVortex`. The spiral file is kept but dereferenced.

**Tech Stack:** Next.js 15, React 19, TailwindCSS, `next-themes`, Aceternity Globe (`cobe` under the hood), shadcn CLI

---

## Chunk 1: Install, Data, Component, Integration

### Task 1: Install Aceternity Globe and download globe data

**Files:**
- Run: `npx shadcn@latest add @aceternity/globe` (adds component files to project)
- Create: `data/globe.json` (downloaded GeoJSON asset)

- [ ] **Step 1: Run shadcn install command**

Working directory: `src/HeimdallWeb.Next`

```bash
cd src/HeimdallWeb.Next
npx shadcn@latest add @aceternity/globe
```

Expected output: Files added to the project (likely `src/components/ui/globe.tsx` or similar). Note the exact path(s) printed by the CLI — you will import `World` from that path in Task 2.

- [ ] **Step 2: Note the installed component path**

After install, run:
```bash
ls src/components/ui/ | grep -i globe
```

The file name tells you the import path for `World`. Common result: `globe.tsx`. Import path will be `@/components/ui/globe`.

- [ ] **Step 3: Download globe.json and place in data/**

```bash
cd /home/floppydisk/Documents/Projects/HeimdallWeb
mkdir -p data
curl -o data/globe.json https://assets.aceternity.com/globe.json
```

Verify the file was downloaded and is valid JSON:
```bash
head -c 200 data/globe.json
```
Expected: starts with `{` (GeoJSON FeatureCollection or similar structure, not an error page).

- [ ] **Step 4: Commit dependencies and data**

```bash
cd /home/floppydisk/Documents/Projects/HeimdallWeb
git add data/globe.json src/HeimdallWeb.Next/src/components/ui/globe.tsx
git add src/HeimdallWeb.Next/package.json src/HeimdallWeb.Next/package-lock.json 2>/dev/null || true
git commit -m "feat: install aceternity globe component and add globe.json data"
```

---

### Task 2: Create `globe-heimdall.tsx`

**Files:**
- Create: `src/HeimdallWeb.Next/src/components/ui/globe-heimdall.tsx`

This component is a `'use client'` wrapper around the Aceternity `World` component. It:
1. Reads `resolvedTheme` to pick dark (indigo) or light (orange) config
2. Guards against `prefers-reduced-motion`
3. Hides on mobile (`hidden lg:block`)
4. Applies CSS mask fade at top/bottom edges

- [ ] **Step 1: Create the component file**

Create `src/HeimdallWeb.Next/src/components/ui/globe-heimdall.tsx` with the following content.

> **Note:** The import path for `World` on line 4 uses `"./globe"` — adjust if the shadcn CLI installed the file with a different name (check what you noted in Task 1 Step 2).

```tsx
'use client';

import React, { useEffect, useState } from 'react';
import { useTheme } from 'next-themes';
import dynamic from 'next/dynamic';

// Adjust this import path if the installed file has a different name
const World = dynamic(
  () => import('./globe').then((m) => m.World),
  { ssr: false }
);

// ── Types ────────────────────────────────────────────────────────────────────

interface Position {
  order: number;
  startLat: number;
  startLng: number;
  endLat: number;
  endLng: number;
  arcAlt: number;
  color: string;
}

// ── Arc data ─────────────────────────────────────────────────────────────────

function buildArcs(isDark: boolean): Position[] {
  const c1 = isDark ? '#818cf8' : '#C2410C';
  const c2 = isDark ? '#6366f1' : '#ea580c';

  const pairs: Array<[number, number, number, number, number]> = [
    // [startLat, startLng, endLat, endLng, arcAlt]
    [-23.55, -46.63,  40.71, -74.01, 0.30], // São Paulo → New York
    [ 40.71, -74.01,  51.51,  -0.13, 0.40], // New York → London
    [ 51.51,  -0.13,  50.11,   8.68, 0.10], // London → Frankfurt
    [ 50.11,   8.68,   1.35, 103.82, 0.40], // Frankfurt → Singapore
    [  1.35, 103.82,  35.68, 139.69, 0.20], // Singapore → Tokyo
    [ 35.68, 139.69,  37.77,-122.42, 0.40], // Tokyo → San Francisco
    [ 37.77,-122.42, -23.55, -46.63, 0.30], // San Francisco → São Paulo
    [ 51.51,  -0.13,   6.52,   3.38, 0.30], // London → Lagos
    [  1.35, 103.82, -33.87, 151.21, 0.20], // Singapore → Sydney
    [ 40.71, -74.01,  43.65, -79.38, 0.10], // New York → Toronto
    [ 50.11,   8.68,  52.37,   4.90, 0.10], // Frankfurt → Amsterdam
    [ 52.37,   4.90,  19.08,  72.88, 0.35], // Amsterdam → Mumbai
    [ 19.08,  72.88,   1.35, 103.82, 0.20], // Mumbai → Singapore
    [ 35.68, 139.69,  37.57, 126.98, 0.10], // Tokyo → Seoul
    [ 37.57, 126.98,  22.32, 114.17, 0.10], // Seoul → Hong Kong
    [ 22.32, 114.17, -33.87, 151.21, 0.30], // Hong Kong → Sydney
    [-23.55, -46.63, -34.60, -58.38, 0.10], // São Paulo → Buenos Aires
    [-34.60, -58.38,  51.51,  -0.13, 0.40], // Buenos Aires → London
    [ 37.77,-122.42,  35.68, 139.69, 0.40], // San Francisco → Tokyo
    [ 51.51,  -0.13,  55.75,  37.62, 0.15], // London → Moscow
    [ 55.75,  37.62,  19.08,  72.88, 0.25], // Moscow → Mumbai
    [ 40.71, -74.01,  25.77, -80.19, 0.10], // New York → Miami
    [ 25.77, -80.19, -23.55, -46.63, 0.20], // Miami → São Paulo
    [ 50.11,   8.68,  25.20,  55.27, 0.25], // Frankfurt → Dubai
    [ 25.20,  55.27,   1.35, 103.82, 0.30], // Dubai → Singapore
    [  6.52,   3.38, -26.20,  28.04, 0.15], // Lagos → Johannesburg
    [-26.20,  28.04,  19.08,  72.88, 0.35], // Johannesburg → Mumbai
    [ 47.61,-122.33,  35.68, 139.69, 0.40], // Seattle → Tokyo
    [ 48.86,   2.35,  40.71, -74.01, 0.35], // Paris → New York
    [ 41.88, -87.63,  51.51,  -0.13, 0.30], // Chicago → London
  ];

  return pairs.map(([startLat, startLng, endLat, endLng, arcAlt], i) => ({
    order: i + 1,
    startLat,
    startLng,
    endLat,
    endLng,
    arcAlt,
    color: i % 2 === 0 ? c1 : c2,
  }));
}

// ── Globe configs ─────────────────────────────────────────────────────────────

const DARK_CONFIG = {
  pointSize: 2,
  globeColor: '#1a1a2e',
  showAtmosphere: true,
  atmosphereColor: '#818cf8',
  atmosphereAltitude: 0.1,
  emissive: '#2d2d5e',
  emissiveIntensity: 0.1,
  shininess: 0.9,
  polygonColor: '#818cf8',
  ambientLight: '#38bdf8',
  directionalLeftLight: '#ffffff',
  directionalTopLight: '#ffffff',
  pointLight: '#818cf8',
  arcTime: 1000,
  arcLength: 0.9,
  rings: 3,
  maxRings: 3,
  autoRotate: true,
  autoRotateSpeed: 0.5,
};

const LIGHT_CONFIG = {
  ...DARK_CONFIG,
  globeColor: '#f0ebe4',
  showAtmosphere: true,
  atmosphereColor: '#fb923c',
  emissive: '#fff3ed',
  polygonColor: '#C2410C',
  ambientLight: '#fed7aa',
  pointLight: '#C2410C',
};

// ── CSS mask ──────────────────────────────────────────────────────────────────

const maskStyle: React.CSSProperties = {
  maskImage:
    'linear-gradient(to bottom, transparent 0%, black 10%, black 85%, transparent 100%)',
  WebkitMaskImage:
    'linear-gradient(to bottom, transparent 0%, black 10%, black 85%, transparent 100%)',
};

// ── Component ─────────────────────────────────────────────────────────────────

export function GlobeHeimdall() {
  const { resolvedTheme } = useTheme();
  const [mounted, setMounted] = useState(false);
  const [reducedMotion, setReducedMotion] = useState(false);

  useEffect(() => {
    setMounted(true);
    setReducedMotion(
      window.matchMedia('(prefers-reduced-motion: reduce)').matches
    );
  }, []);

  if (!mounted || reducedMotion) return null;

  // resolvedTheme is undefined on first render → default to dark
  const isDark = resolvedTheme !== 'light';
  const globeConfig = isDark ? DARK_CONFIG : LIGHT_CONFIG;
  const arcs = buildArcs(isDark);

  return (
    <div
      className="hidden lg:block absolute inset-0 z-0 pointer-events-none w-full h-full"
      style={maskStyle}
    >
      <World
        key={resolvedTheme ?? 'dark'}
        config={globeConfig}
        data={arcs}
      />
    </div>
  );
}
```

- [ ] **Step 2: Check for TypeScript errors**

```bash
cd src/HeimdallWeb.Next
npx tsc --noEmit 2>&1 | grep globe-heimdall
```

Expected: no output (no errors). If errors appear about `World` props, inspect the installed globe component's exported types and adjust the `config` / `data` prop names to match.

- [ ] **Step 3: Commit the new component**

```bash
git add src/HeimdallWeb.Next/src/components/ui/globe-heimdall.tsx
git commit -m "feat: add GlobeHeimdall dual-theme wrapper for Aceternity Globe"
```

---

### Task 3: Swap HeroVortex → GlobeHeimdall in page.tsx

**Files:**
- Modify: `src/HeimdallWeb.Next/src/app/(public)/page.tsx`

- [ ] **Step 1: Update imports in page.tsx**

In `src/HeimdallWeb.Next/src/app/(public)/page.tsx`, make these two changes:

**Remove** this line:
```tsx
import { HeroVortex } from "@/components/ui/hero-vortex";
```

**Add** this line (in its place or alongside other UI imports):
```tsx
import { GlobeHeimdall } from "@/components/ui/globe-heimdall";
```

- [ ] **Step 2: Replace the JSX in the Hero section**

Find this JSX (around line 113):
```tsx
{/* WebGL Vortex effect */}
<HeroVortex className="absolute inset-0 z-0 w-full h-full pointer-events-none" />
```

Replace with:
```tsx
{/* GitHub Globe background */}
<GlobeHeimdall />
```

Do NOT change anything else in `page.tsx`.

- [ ] **Step 3: Check for TypeScript errors**

```bash
cd src/HeimdallWeb.Next
npx tsc --noEmit 2>&1 | grep -E "page|globe"
```

Expected: no output.

- [ ] **Step 4: Commit the integration**

```bash
git add src/HeimdallWeb.Next/src/app/\(public\)/page.tsx
git commit -m "feat: replace HeroVortex with GlobeHeimdall in landing page hero"
```

---

### Task 4: Visual verification with Playwright

**No test files to create** — use Playwright MCP to visually verify the result in the running Docker dev environment (`http://localhost:3000`).

- [ ] **Step 1: Verify the landing page loads without console errors**

Using Playwright MCP:
1. Navigate to `http://localhost:3000`
2. Check browser console — expect: no errors or warnings about WebGL, Three.js, missing modules, or hydration mismatches
3. Take a desktop screenshot (1280px viewport)
4. Confirm: globe animation is visible in the hero background
5. Confirm: headline "Escaneie. Analise. Proteja." is readable on top
6. Confirm: `ScanPreviewCard` is visible on the right

- [ ] **Step 2: Verify mobile layout (375px)**

Using Playwright MCP:
1. Resize viewport to 375px width
2. Take a screenshot
3. Confirm: globe is NOT visible (hidden lg:block)
4. Confirm: hero text and CTA buttons are visible
5. Confirm: no layout breakage

- [ ] **Step 3: Verify light mode**

Using Playwright MCP:
1. Toggle to light mode (click the theme toggle in the header)
2. Take a screenshot
3. Confirm: globe reappears with orange/beige palette (not indigo)
4. Confirm: no flash or broken state during theme toggle

- [ ] **Step 4: Verify HeroVortex is still present (not deleted)**

```bash
ls src/HeimdallWeb.Next/src/components/ui/hero-vortex.tsx
```

Expected: file exists.

- [ ] **Step 5: Final commit if any fixes were applied**

If you made any adjustments during verification:
```bash
git add -p
git commit -m "fix: adjust globe integration after visual verification"
```

---

## Summary

| Task | Files | Commit |
|------|-------|--------|
| 1 | `data/globe.json`, installed globe component | `feat: install aceternity globe component and add globe.json data` |
| 2 | `src/components/ui/globe-heimdall.tsx` | `feat: add GlobeHeimdall dual-theme wrapper for Aceternity Globe` |
| 3 | `src/app/(public)/page.tsx` | `feat: replace HeroVortex with GlobeHeimdall in landing page hero` |
| 4 | (visual checks only) | `fix: adjust globe integration after visual verification` (if needed) |
