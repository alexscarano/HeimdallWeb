'use client';

import { useEffect, useRef } from 'react';
import { useTheme } from 'next-themes';
import * as THREE from 'three';

interface HeroVortexProps {
  className?: string;
}

const VERTEX_SHADER = `
  varying vec2 vUv;
  void main() {
    vUv = uv;
    gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
  }
`;

const FRAGMENT_SHADER = `
  uniform vec3 u_color;
  uniform float u_intensity;
  varying vec2 vUv;

  void main() {
    // Origin at bottom-center
    vec2 origin = vec2(0.5, 0.0);
    float dist = distance(vUv, origin);

    // Cone-shaped falloff: narrow at origin, wider at top
    float angle = atan(abs(vUv.x - 0.5), vUv.y);
    float coneMask = smoothstep(1.0, 0.2, angle / (3.14159 * 0.5));

    // Exponential falloff with distance
    float glow = exp(-dist * 3.5) * coneMask;
    glow = pow(glow, 1.2);

    gl_FragColor = vec4(u_color * glow, glow * u_intensity);
  }
`;

const DARK_COLOR = new THREE.Color(0x818cf8);   // indigo-400
const LIGHT_COLOR = new THREE.Color(0x059669);  // emerald-600

function getParticleCount(width: number): number {
  if (width < 768) return 0;
  if (width < 1024) return 300;
  return 600;
}

export function HeroVortex({ className }: HeroVortexProps) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const { resolvedTheme } = useTheme();

  useEffect(() => {
    if (typeof window === 'undefined') return;

    // Respect prefers-reduced-motion
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;

    const canvas = canvasRef.current;
    if (!canvas) return;

    const container = canvas.parentElement;
    if (!container) return;

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
    const themeColor = isDark ? DARK_COLOR : LIGHT_COLOR;
    const intensity = isDark ? 0.12 : 0.15;

    const width = container.offsetWidth || window.innerWidth;
    const height = container.offsetHeight || window.innerHeight;

    renderer.setSize(width, height);
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 1.5));
    renderer.setClearColor(0x000000, 0);

    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(60, width / height, 0.1, 1000);
    camera.position.z = 5;

    // --- Layer 1: GLSL Glow Shader ---
    const glowGeo = new THREE.PlaneGeometry(2, 2);
    const glowMat = new THREE.ShaderMaterial({
      vertexShader: VERTEX_SHADER,
      fragmentShader: FRAGMENT_SHADER,
      uniforms: {
        u_color: { value: themeColor },
        u_intensity: { value: intensity },
      },
      transparent: true,
      depthWrite: false,
      blending: THREE.AdditiveBlending,
    });

    const glowMesh = new THREE.Mesh(glowGeo, glowMat);
    // Scale to fill the camera view
    glowMesh.scale.set(camera.aspect * 5.5, 5.5, 1);
    glowMesh.position.z = -1;
    scene.add(glowMesh);

    // --- Layer 2: Helical Cone Particles ---
    const particleCount = getParticleCount(window.innerWidth);
    let particlesMesh: THREE.Points | null = null;
    let baseAngles: Float32Array | null = null;
    let baseRadii: Float32Array | null = null;
    let baseHeights: Float32Array | null = null;

    if (particleCount > 0) {
      const positions = new Float32Array(particleCount * 3);
      const sizes = new Float32Array(particleCount);
      baseAngles = new Float32Array(particleCount);
      baseRadii = new Float32Array(particleCount);
      baseHeights = new Float32Array(particleCount);

      const maxRadius = 2.5 * camera.aspect;
      const totalHeight = 5.0;

      for (let i = 0; i < particleCount; i++) {
        const t = i / particleCount;
        const angle = t * Math.PI * 8;
        const radius = t * maxRadius;
        const y = (1 - t) * totalHeight - totalHeight / 2;

        baseAngles[i] = angle;
        baseRadii[i] = radius;
        baseHeights[i] = y;

        positions[i * 3] = radius * Math.cos(angle);
        positions[i * 3 + 1] = y;
        positions[i * 3 + 2] = radius * Math.sin(angle);

        // Particles near origin are larger/brighter
        sizes[i] = THREE.MathUtils.lerp(4, 1.5, t);
      }

      const geo = new THREE.BufferGeometry();
      geo.setAttribute('position', new THREE.BufferAttribute(positions, 3));
      geo.setAttribute('size', new THREE.BufferAttribute(sizes, 1));

      const mat = new THREE.PointsMaterial({
        color: themeColor,
        size: 0.05,
        sizeAttenuation: true,
        transparent: true,
        opacity: 0.7,
        blending: THREE.AdditiveBlending,
        depthWrite: false,
      });

      particlesMesh = new THREE.Points(geo, mat);
      scene.add(particlesMesh);
    }

    let raf: number;
    let startTime = performance.now();

    const animate = () => {
      raf = requestAnimationFrame(animate);
      const elapsed = (performance.now() - startTime) / 1000;

      // Animate particles
      if (particlesMesh && baseAngles && baseRadii && baseHeights) {
        const positions = particlesMesh.geometry.attributes.position.array as Float32Array;
        const count = baseAngles.length;

        for (let i = 0; i < count; i++) {
          const t = i / count;
          const angle = baseAngles[i] + elapsed * 0.4;
          const radius = baseRadii[i] + Math.sin(elapsed * 0.3 + i * 0.1) * 0.05;
          const y = baseHeights[i];

          positions[i * 3] = radius * Math.cos(angle);
          positions[i * 3 + 1] = y;
          positions[i * 3 + 2] = radius * Math.sin(angle);
        }
        particlesMesh.geometry.attributes.position.needsUpdate = true;

        // Gentle overall rotation
        particlesMesh.rotation.y = elapsed * 0.05;
      }

      // Subtle glow pulse
      const pulse = 1 + Math.sin(elapsed * 0.8) * 0.05;
      glowMesh.scale.set(camera.aspect * 5.5 * pulse, 5.5, 1);

      renderer.render(scene, camera);
    };

    animate();

    const handleResize = () => {
      const w = container.offsetWidth || window.innerWidth;
      const h = container.offsetHeight || window.innerHeight;
      renderer.setSize(w, h);
      camera.aspect = w / h;
      camera.updateProjectionMatrix();
      glowMesh.scale.set(camera.aspect * 5.5, 5.5, 1);
    };

    window.addEventListener('resize', handleResize);

    return () => {
      window.removeEventListener('resize', handleResize);
      cancelAnimationFrame(raf);
      renderer.dispose();
      glowGeo.dispose();
      glowMat.dispose();
      if (particlesMesh) {
        particlesMesh.geometry.dispose();
        (particlesMesh.material as THREE.Material).dispose();
      }
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [resolvedTheme]);

  return (
    <canvas
      ref={canvasRef}
      className={className}
      aria-hidden="true"
      style={{
        maskImage:
          'linear-gradient(to bottom, transparent 0%, black 10%, black 85%, transparent 100%)',
        WebkitMaskImage:
          'linear-gradient(to bottom, transparent 0%, black 10%, black 85%, transparent 100%)',
      }}
    />
  );
}
