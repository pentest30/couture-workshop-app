import type { NextConfig } from 'next';

const apiUrl = process.env.NEXT_PRIVATE_API_URL || 'http://localhost:5000';

const nextConfig: NextConfig = {
  output: 'standalone',
  async rewrites() {
    return [
      { source: '/api/:path*', destination: `${apiUrl}/api/:path*` },
      { source: '/uploads/:path*', destination: `${apiUrl}/uploads/:path*` },
    ];
  },
};

export default nextConfig;
