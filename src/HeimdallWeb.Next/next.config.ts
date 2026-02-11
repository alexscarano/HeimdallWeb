import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  /* config options here */
  reactCompiler: true,
  
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: "http://localhost:5110/api/:path*",
      },
    ];
  },
};

export default nextConfig;
