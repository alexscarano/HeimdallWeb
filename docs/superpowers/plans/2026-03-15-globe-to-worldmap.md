# Globe → World Map Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the Three.js WebGL 3D globe background in the landing page hero with the Aceternity UI 2D SVG World Map component, keeping the same colors, positioning, and accessibility behavior.

**Architecture:** The existing `GlobeHeimdall` wrapper component will be updated to use the new `WorldMap` SVG component instead of the ThreeGlobe `World` component. The `world-map.tsx` component is installed from the Aceternity registry, modified only to use `bg-transparent` instead of the default dark/light background. No changes are needed in `page.tsx`.

**Tech Stack:** Next.js 15, React 19, TailwindCSS, `dotted-map` (SVG map generation), `motion/react` (path animation)

---

## Chunk 1: Install dependency + create world-map.tsx

### Task 1: Install `dotted-map`

**Files:**
- Modify: `src/HeimdallWeb.Next/package.json`

- [ ] **Step 1: Install the package**

Run from `src/HeimdallWeb.Next/`:
```bash
npm install dotted-map
```

Expected: `dotted-map` appears in `dependencies` in `package.json`.

- [ ] **Step 2: Verify install**

```bash
node -e "require('dotted-map'); console.log('ok')"
```

Expected: `ok`

- [ ] **Step 3: Commit**

```bash
git add src/HeimdallWeb.Next/package.json src/HeimdallWeb.Next/package-lock.json
git commit -m "chore: add dotted-map dependency for world map component"
```

---

### Task 2: Create `world-map.tsx`

**Files:**
- Create: `src/HeimdallWeb.Next/src/components/ui/world-map.tsx`

This is the Aceternity WorldMap component with one modification: `bg-transparent` instead of `dark:bg-black bg-white`.

- [ ] **Step 1: Create the file**

Create `src/HeimdallWeb.Next/src/components/ui/world-map.tsx` with this exact content:

```tsx
"use client";

import { useRef } from "react";
import { motion } from "motion/react";
import DottedMap from "dotted-map";
import { useTheme } from "next-themes";

interface MapProps {
  dots?: Array<{
    start: { lat: number; lng: number; label?: string };
    end: { lat: number; lng: number; label?: string };
  }>;
  lineColor?: string;
}

export default function WorldMap({
  dots = [],
  lineColor = "#0ea5e9",
}: MapProps) {
  const svgRef = useRef<SVGSVGElement>(null);
  const map = new DottedMap({ height: 100, grid: "diagonal" });

  const { theme } = useTheme();

  const svgMap = map.getSVG({
    radius: 0.22,
    color: theme === "dark" ? "#FFFFFF40" : "#00000040",
    shape: "circle",
    backgroundColor: "transparent",
  });

  const projectPoint = (lat: number, lng: number) => {
    const x = (lng + 180) * (800 / 360);
    const y = (90 - lat) * (400 / 180);
    return { x, y };
  };

  const createCurvedPath = (
    start: { x: number; y: number },
    end: { x: number; y: number }
  ) => {
    const midX = (start.x + end.x) / 2;
    const midY = Math.min(start.y, end.y) - 50;
    return `M ${start.x} ${start.y} Q ${midX} ${midY} ${end.x} ${end.y}`;
  };

  return (
    <div className="w-full aspect-[2/1] bg-transparent rounded-lg relative font-sans">
      <img
        src={`data:image/svg+xml;utf8,${encodeURIComponent(svgMap)}`}
        className="h-full w-full [mask-image:linear-gradient(to_bottom,transparent,white_10%,white_90%,transparent)] pointer-events-none select-none"
        alt="world map"
        height="495"
        width="1056"
        draggable={false}
      />
      <svg
        ref={svgRef}
        viewBox="0 0 800 400"
        className="w-full h-full absolute inset-0 pointer-events-none select-none"
      >
        {dots.map((dot, i) => {
          const startPoint = projectPoint(dot.start.lat, dot.start.lng);
          const endPoint = projectPoint(dot.end.lat, dot.end.lng);
          return (
            <g key={`path-group-${i}`}>
              <motion.path
                d={createCurvedPath(startPoint, endPoint)}
                fill="none"
                stroke="url(#path-gradient)"
                strokeWidth="1"
                initial={{ pathLength: 0 }}
                animate={{ pathLength: 1 }}
                transition={{
                  duration: 1,
                  delay: 0.5 * i,
                  ease: "easeOut",
                }}
              />
            </g>
          );
        })}

        <defs>
          <linearGradient id="path-gradient" x1="0%" y1="0%" x2="100%" y2="0%">
            <stop offset="0%" stopColor="white" stopOpacity="0" />
            <stop offset="5%" stopColor={lineColor} stopOpacity="1" />
            <stop offset="95%" stopColor={lineColor} stopOpacity="1" />
            <stop offset="100%" stopColor="white" stopOpacity="0" />
          </linearGradient>
        </defs>

        {dots.map((dot, i) => (
          <g key={`points-group-${i}`}>
            <g key={`start-${i}`}>
              <circle
                cx={projectPoint(dot.start.lat, dot.start.lng).x}
                cy={projectPoint(dot.start.lat, dot.start.lng).y}
                r="2"
                fill={lineColor}
              />
              <circle
                cx={projectPoint(dot.start.lat, dot.start.lng).x}
                cy={projectPoint(dot.start.lat, dot.start.lng).y}
                r="2"
                fill={lineColor}
                opacity="0.5"
              >
                <animate attributeName="r" from="2" to="8" dur="1.5s" begin="0s" repeatCount="indefinite" />
                <animate attributeName="opacity" from="0.5" to="0" dur="1.5s" begin="0s" repeatCount="indefinite" />
              </circle>
            </g>
            <g key={`end-${i}`}>
              <circle
                cx={projectPoint(dot.end.lat, dot.end.lng).x}
                cy={projectPoint(dot.end.lat, dot.end.lng).y}
                r="2"
                fill={lineColor}
              />
              <circle
                cx={projectPoint(dot.end.lat, dot.end.lng).x}
                cy={projectPoint(dot.end.lat, dot.end.lng).y}
                r="2"
                fill={lineColor}
                opacity="0.5"
              >
                <animate attributeName="r" from="2" to="8" dur="1.5s" begin="0s" repeatCount="indefinite" />
                <animate attributeName="opacity" from="0.5" to="0" dur="1.5s" begin="0s" repeatCount="indefinite" />
              </circle>
            </g>
          </g>
        ))}
      </svg>
    </div>
  );
}
```

- [ ] **Step 2: Verify TypeScript compiles**

```bash
cd src/HeimdallWeb.Next && npx tsc --noEmit
```

Expected: No errors related to `world-map.tsx`.

- [ ] **Step 3: Commit**

```bash
git add src/HeimdallWeb.Next/src/components/ui/world-map.tsx
git commit -m "feat: add Aceternity WorldMap SVG component"
```

---

## Chunk 2: Update globe-heimdall.tsx

### Task 3: Replace Globe with WorldMap in `globe-heimdall.tsx`

**Files:**
- Modify: `src/HeimdallWeb.Next/src/components/ui/globe-heimdall.tsx`

Replace the entire file. Key changes:
- Remove `dynamic` import of `World` (ThreeGlobe)
- Add static import of `WorldMap`
- Remove `buildArcs()`, `DARK_CONFIG`, `LIGHT_CONFIG`
- Add `DOTS` array (30 city pairs from former arc data)
- Pass `lineColor` per theme instead of `globeConfig`
- Drop `key={resolvedTheme ?? 'dark'}` — reactive via prop

- [ ] **Step 1: Overwrite the file**

Write `src/HeimdallWeb.Next/src/components/ui/globe-heimdall.tsx`:

```tsx
'use client';

import { useEffect, useState, type CSSProperties } from 'react';
import { useTheme } from 'next-themes';
import WorldMap from './world-map';

// ── Connection pairs (same cities as the former arc data) ─────────────────────

const DOTS = [
  { start: { lat: -23.55, lng: -46.63 }, end: { lat:  40.71, lng: -74.01 } }, // São Paulo → New York
  { start: { lat:  40.71, lng: -74.01 }, end: { lat:  51.51, lng:  -0.13 } }, // New York → London
  { start: { lat:  51.51, lng:  -0.13 }, end: { lat:  50.11, lng:   8.68 } }, // London → Frankfurt
  { start: { lat:  50.11, lng:   8.68 }, end: { lat:   1.35, lng: 103.82 } }, // Frankfurt → Singapore
  { start: { lat:   1.35, lng: 103.82 }, end: { lat:  35.68, lng: 139.69 } }, // Singapore → Tokyo
  { start: { lat:  35.68, lng: 139.69 }, end: { lat:  37.77, lng:-122.42 } }, // Tokyo → San Francisco
  { start: { lat:  37.77, lng:-122.42 }, end: { lat: -23.55, lng: -46.63 } }, // San Francisco → São Paulo
  { start: { lat:  51.51, lng:  -0.13 }, end: { lat:   6.52, lng:   3.38 } }, // London → Lagos
  { start: { lat:   1.35, lng: 103.82 }, end: { lat: -33.87, lng: 151.21 } }, // Singapore → Sydney
  { start: { lat:  40.71, lng: -74.01 }, end: { lat:  43.65, lng: -79.38 } }, // New York → Toronto
  { start: { lat:  50.11, lng:   8.68 }, end: { lat:  52.37, lng:   4.90 } }, // Frankfurt → Amsterdam
  { start: { lat:  52.37, lng:   4.90 }, end: { lat:  19.08, lng:  72.88 } }, // Amsterdam → Mumbai
  { start: { lat:  19.08, lng:  72.88 }, end: { lat:   1.35, lng: 103.82 } }, // Mumbai → Singapore
  { start: { lat:  35.68, lng: 139.69 }, end: { lat:  37.57, lng: 126.98 } }, // Tokyo → Seoul
  { start: { lat:  37.57, lng: 126.98 }, end: { lat:  22.32, lng: 114.17 } }, // Seoul → Hong Kong
  { start: { lat:  22.32, lng: 114.17 }, end: { lat: -33.87, lng: 151.21 } }, // Hong Kong → Sydney
  { start: { lat: -23.55, lng: -46.63 }, end: { lat: -34.60, lng: -58.38 } }, // São Paulo → Buenos Aires
  { start: { lat: -34.60, lng: -58.38 }, end: { lat:  51.51, lng:  -0.13 } }, // Buenos Aires → London
  { start: { lat:  37.77, lng:-122.42 }, end: { lat:  35.68, lng: 139.69 } }, // San Francisco → Tokyo
  { start: { lat:  51.51, lng:  -0.13 }, end: { lat:  55.75, lng:  37.62 } }, // London → Moscow
  { start: { lat:  55.75, lng:  37.62 }, end: { lat:  19.08, lng:  72.88 } }, // Moscow → Mumbai
  { start: { lat:  40.71, lng: -74.01 }, end: { lat:  25.77, lng: -80.19 } }, // New York → Miami
  { start: { lat:  25.77, lng: -80.19 }, end: { lat: -23.55, lng: -46.63 } }, // Miami → São Paulo
  { start: { lat:  50.11, lng:   8.68 }, end: { lat:  25.20, lng:  55.27 } }, // Frankfurt → Dubai
  { start: { lat:  25.20, lng:  55.27 }, end: { lat:   1.35, lng: 103.82 } }, // Dubai → Singapore
  { start: { lat:   6.52, lng:   3.38 }, end: { lat: -26.20, lng:  28.04 } }, // Lagos → Johannesburg
  { start: { lat: -26.20, lng:  28.04 }, end: { lat:  19.08, lng:  72.88 } }, // Johannesburg → Mumbai
  { start: { lat:  47.61, lng:-122.33 }, end: { lat:  35.68, lng: 139.69 } }, // Seattle → Tokyo
  { start: { lat:  48.86, lng:   2.35 }, end: { lat:  40.71, lng: -74.01 } }, // Paris → New York
  { start: { lat:  41.88, lng: -87.63 }, end: { lat:  51.51, lng:  -0.13 } }, // Chicago → London
];

// ── CSS mask (fade top and bottom edges) ──────────────────────────────────────

const maskStyle: CSSProperties = {
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
      window.matchMedia('(prefers-reduced-motion: reduce)').matches,
    );
  }, []);

  if (!mounted || reducedMotion) return null;

  const isDark = resolvedTheme !== 'light';
  const lineColor = isDark ? '#818cf8' : '#C2410C';

  return (
    <div
      className="hidden lg:block absolute inset-0 z-0 pointer-events-none w-full h-full"
      style={maskStyle}
    >
      <WorldMap dots={DOTS} lineColor={lineColor} />
    </div>
  );
}
```

- [ ] **Step 2: Verify TypeScript compiles**

```bash
cd src/HeimdallWeb.Next && npx tsc --noEmit
```

Expected: No errors.

- [ ] **Step 3: Commit**

```bash
git add src/HeimdallWeb.Next/src/components/ui/globe-heimdall.tsx
git commit -m "feat: replace ThreeGlobe with Aceternity WorldMap in hero background"
```

---

## Chunk 3: Cleanup + verification

### Task 4: Delete orphaned files

**Files:**
- Delete: `src/HeimdallWeb.Next/src/components/ui/globe.tsx`
- Delete: `src/HeimdallWeb.Next/src/data/globe.json`

- [ ] **Step 1: Confirm nothing else imports globe.tsx or globe.json**

```bash
grep -r "globe" src/HeimdallWeb.Next/src --include="*.ts" --include="*.tsx" | grep -v node_modules | grep -v "globe-heimdall\|world-map\|globe\.tsx\|globe\.json"
```

Expected: No output (only `globe-heimdall.tsx` used to import from `./globe`, and after Chunk 2 it no longer does).

- [ ] **Step 2: Confirm nothing else imports globe.json**

```bash
grep -r "globe\.json" src/HeimdallWeb.Next/src --include="*.ts" --include="*.tsx" | grep -v node_modules
```

Expected: No output.

- [ ] **Step 3: Delete the files**

```bash
rm src/HeimdallWeb.Next/src/components/ui/globe.tsx
rm src/HeimdallWeb.Next/src/data/globe.json
```

- [ ] **Step 4: Verify build still compiles**

```bash
cd src/HeimdallWeb.Next && npx tsc --noEmit
```

Expected: No errors.

- [ ] **Step 5: Commit**

```bash
git add src/HeimdallWeb.Next/src/components/ui/globe.tsx src/HeimdallWeb.Next/src/data/globe.json
git commit -m "chore: remove obsolete globe.tsx and globe.json"
```

---

### Task 5: Remove unused packages from package.json

**Files:**
- Modify: `src/HeimdallWeb.Next/package.json`

Remove `three-globe`, `@react-three/fiber`, `@react-three/drei` — exclusively used by the now-deleted `globe.tsx`. **Do NOT remove `three`** — still used by `webgl-background.tsx` and `hero-vortex.tsx`.

- [ ] **Step 1: Confirm three is still needed**

```bash
grep -r "from ['\"]three['\"]" src/HeimdallWeb.Next/src --include="*.ts" --include="*.tsx" | grep -v node_modules | grep -v globe
```

Expected: Lines from `webgl-background.tsx` and/or `hero-vortex.tsx`.

- [ ] **Step 2: Uninstall the three packages**

```bash
cd src/HeimdallWeb.Next && npm uninstall three-globe @react-three/fiber @react-three/drei
```

Expected: All three removed from `package.json`. `three` remains.

- [ ] **Step 3: Verify build**

```bash
npx tsc --noEmit
```

Expected: No errors.

- [ ] **Step 4: Commit**

```bash
git add src/HeimdallWeb.Next/package.json src/HeimdallWeb.Next/package-lock.json
git commit -m "chore: remove three-globe and @react-three/* (replaced by dotted-map world map)"
```

---

### Task 6: Visual verification with Playwright

- [ ] **Step 1: Start the frontend**

```bash
cd src/HeimdallWeb.Next && npm run dev
```

Wait for `Ready` message on port 3000. (Or use `docker compose up -d frontend` if running in Docker.)

- [ ] **Step 2: Screenshot desktop — dark mode**

Navigate to `http://localhost:3000` and take a screenshot at 1280×800. Verify:
- World map SVG is visible in the hero background
- Animated arc lines are drawing in with indigo color (`#818cf8`)
- Pulsing dots visible at city connection points
- Hero content (headline, CTAs, ScanPreviewCard) visible above the map
- No console errors

- [ ] **Step 3: Screenshot desktop — light mode**

Toggle to light mode. Take screenshot. Verify:
- Map dots are dark (black/gray on white)
- Arc lines are orange (`#C2410C`)
- Background is transparent (hero background color shows through)

- [ ] **Step 4: Screenshot mobile (375px)**

Resize to 375px width. Verify:
- World map is **not visible** (hidden on mobile via `hidden lg:block`)
- Hero content renders correctly without the map
- No layout issues

- [ ] **Step 5: Check console for errors**

Verify no JavaScript errors in the browser console. Common issues to watch for:
- `DottedMap is not a constructor` → check `dotted-map` install
- `Cannot read properties of undefined (reading 'getSVG')` → same

- [ ] **Step 6: Final commit (if any fixes were needed)**

If visual fixes were applied, commit them. Otherwise no commit needed.
