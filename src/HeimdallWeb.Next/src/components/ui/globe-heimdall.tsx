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
        globeConfig={globeConfig}
        data={arcs}
      />
    </div>
  );
}
