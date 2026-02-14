import type { NextConfig } from "next";

// NEXT_BACKEND_URL é injetado pelo docker-compose para apontar para o serviço
// "api" dentro da rede Docker (ex: http://api:5110).
// Em desenvolvimento local sem Docker, cai no fallback localhost:5110.
const backendUrl =
  process.env.NEXT_BACKEND_URL ?? "http://localhost:5110";

const nextConfig: NextConfig = {
  /* config options here */
  reactCompiler: true,

  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: `${backendUrl}/api/:path*`,
      },
    ];
  },
};

export default nextConfig;
