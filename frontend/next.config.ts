import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Minimal, self-contained runtime output for the Docker image (frontend/Dockerfile) — bundles
  // only the files `server.js` needs instead of shipping the full node_modules.
  output: "standalone",
};

export default nextConfig;
