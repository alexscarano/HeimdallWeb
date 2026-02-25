'use client';

import { useEffect, useRef } from 'react';
import { useTheme } from 'next-themes';
import * as THREE from 'three';

const COLORS = {
  light: { nodes: 0xC2410C, lines: 0xC2410C },
  dark: { nodes: 0x818cf8, lines: 0x818cf8 },
};

const OPACITY = {
  light: { nodes: 0.25, lines: 0.09 },
  dark: { nodes: 0.22, lines: 0.09 },
};

function getParticleCount(width: number) {
  if (width < 768) return 20;
  if (width < 1024) return 40;
  return 60;
}

function getConnectionDistance(width: number) {
  if (width < 768) return 0;
  if (width < 1024) return 100;
  return 120;
}

export function WebGLBackground() {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const { resolvedTheme } = useTheme();
  const sceneRef = useRef<{
    renderer: THREE.WebGLRenderer;
    scene: THREE.Scene;
    camera: THREE.OrthographicCamera;
    points: THREE.Points;
    lines: THREE.LineSegments | null;
    positions: Float32Array;
    velocities: Float32Array;
    particleCount: number;
    connectionDistance: number;
    raf: number;
    isDark: boolean;
  } | null>(null);

  useEffect(() => {
    if (typeof window === 'undefined') return;

    // Respect prefers-reduced-motion
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;

    const canvas = canvasRef.current;
    if (!canvas) return;

    // Proactive WebGL support check — avoids Three.js logging its own error
    const testCanvas = document.createElement('canvas');
    const gl = testCanvas.getContext('webgl2') ?? testCanvas.getContext('webgl');
    if (!gl) return;

    const renderer = new THREE.WebGLRenderer({
      canvas,
      alpha: true,
      antialias: false,
      powerPreference: 'low-power',
    });

    const isDark = resolvedTheme === 'dark';
    const width = window.innerWidth;
    const height = window.innerHeight;
    const particleCount = getParticleCount(width);
    const connectionDistance = getConnectionDistance(width);
    const color = isDark ? COLORS.dark : COLORS.light;
    const opacity = isDark ? OPACITY.dark : OPACITY.light;

    renderer.setSize(width, height);
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    renderer.setClearColor(0x000000, 0);

    const scene = new THREE.Scene();
    const camera = new THREE.OrthographicCamera(
      -width / 2, width / 2, height / 2, -height / 2, 0.1, 100
    );
    camera.position.z = 10;

    // Particles
    const positions = new Float32Array(particleCount * 3);
    const velocities = new Float32Array(particleCount * 3);

    for (let i = 0; i < particleCount; i++) {
      positions[i * 3] = (Math.random() - 0.5) * width;
      positions[i * 3 + 1] = (Math.random() - 0.5) * height;
      positions[i * 3 + 2] = 0;
      velocities[i * 3] = (Math.random() - 0.5) * 0.4;
      velocities[i * 3 + 1] = (Math.random() - 0.5) * 0.4;
      velocities[i * 3 + 2] = 0;
    }

    const pointsGeo = new THREE.BufferGeometry();
    pointsGeo.setAttribute('position', new THREE.BufferAttribute(positions, 3));

    const pointsMat = new THREE.PointsMaterial({
      color: color.nodes,
      size: 4,
      transparent: true,
      opacity: opacity.nodes,
      sizeAttenuation: false,
    });

    const points = new THREE.Points(pointsGeo, pointsMat);
    scene.add(points);

    // Lines
    let lines: THREE.LineSegments | null = null;
    let linePositions: Float32Array | null = null;

    if (connectionDistance > 0) {
      const maxLines = particleCount * (particleCount - 1) / 2;
      linePositions = new Float32Array(maxLines * 6);
      const linesGeo = new THREE.BufferGeometry();
      linesGeo.setAttribute('position', new THREE.BufferAttribute(linePositions, 3));

      const linesMat = new THREE.LineBasicMaterial({
        color: color.lines,
        transparent: true,
        opacity: opacity.lines,
      });

      lines = new THREE.LineSegments(linesGeo, linesMat);
      scene.add(lines);
    }

    let raf: number;

    const animate = () => {
      raf = requestAnimationFrame(animate);

      const hw = width / 2;
      const hh = height / 2;

      // Update positions
      for (let i = 0; i < particleCount; i++) {
        positions[i * 3] += velocities[i * 3];
        positions[i * 3 + 1] += velocities[i * 3 + 1];

        // Bounce off edges
        if (Math.abs(positions[i * 3]) > hw) velocities[i * 3] *= -1;
        if (Math.abs(positions[i * 3 + 1]) > hh) velocities[i * 3 + 1] *= -1;
      }
      pointsGeo.attributes.position.needsUpdate = true;

      // Update lines
      if (lines && linePositions && connectionDistance > 0) {
        let lineIndex = 0;
        for (let i = 0; i < particleCount; i++) {
          for (let j = i + 1; j < particleCount; j++) {
            const dx = positions[i * 3] - positions[j * 3];
            const dy = positions[i * 3 + 1] - positions[j * 3 + 1];
            const dist = Math.sqrt(dx * dx + dy * dy);

            if (dist < connectionDistance) {
              linePositions[lineIndex++] = positions[i * 3];
              linePositions[lineIndex++] = positions[i * 3 + 1];
              linePositions[lineIndex++] = 0;
              linePositions[lineIndex++] = positions[j * 3];
              linePositions[lineIndex++] = positions[j * 3 + 1];
              linePositions[lineIndex++] = 0;
            } else {
              // Degenerate segment (won't render)
              linePositions[lineIndex++] = 0;
              linePositions[lineIndex++] = 0;
              linePositions[lineIndex++] = 0;
              linePositions[lineIndex++] = 0;
              linePositions[lineIndex++] = 0;
              linePositions[lineIndex++] = 0;
            }
          }
        }
        lines.geometry.attributes.position.needsUpdate = true;
      }

      renderer.render(scene, camera);
    };

    animate();

    const handleResize = () => {
      const w = window.innerWidth;
      const h = window.innerHeight;
      renderer.setSize(w, h);
      camera.left = -w / 2;
      camera.right = w / 2;
      camera.top = h / 2;
      camera.bottom = -h / 2;
      camera.updateProjectionMatrix();
    };

    window.addEventListener('resize', handleResize);

    sceneRef.current = {
      renderer,
      scene,
      camera,
      points,
      lines,
      positions,
      velocities,
      particleCount,
      connectionDistance,
      raf,
      isDark,
    };

    return () => {
      window.removeEventListener('resize', handleResize);
      cancelAnimationFrame(raf);
      renderer.dispose();
      pointsGeo.dispose();
      pointsMat.dispose();
      if (lines) {
        lines.geometry.dispose();
        (lines.material as THREE.Material).dispose();
      }
      sceneRef.current = null;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [resolvedTheme]);

  return (
    <canvas
      ref={canvasRef}
      className="pointer-events-none fixed inset-0 -z-10"
      aria-hidden="true"
    />
  );
}
