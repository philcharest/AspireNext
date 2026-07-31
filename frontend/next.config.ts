import type { NextConfig } from "next";

// Aspire injects the backend URL based on the resource name "server" used in AppHost.cs
const serverUrl =
    process.env["services__server__http__0"] ||
    process.env["services__server__https__0"];

const nextConfig: NextConfig = {
    output: 'standalone',
    async rewrites() {
        if (!serverUrl) return [];

        return [
            {
                source: "/api/:path*",
                destination: `${serverUrl}/api/:path*`,
            },
        ];
    },
};

export default nextConfig;
