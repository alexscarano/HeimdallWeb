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
