import { fileURLToPath, URL } from "node:url";
import react from "@vitejs/plugin-react";
import { defineConfig, loadEnv, ServerOptions, UserConfig } from "vite";

const isValidPort = (port: number) => !isNaN(port) && port > 0 && port <= 65535;

// https://vite.dev/config/
export default defineConfig(({ command, mode }) => {
    process.env = { ...process.env, ...loadEnv(mode, process.cwd()) };

    const baseConfig: UserConfig = {
        plugins: [react()],
        resolve: {
            alias: {
                "@": fileURLToPath(new URL("./src", import.meta.url)),
            },
        },
    };

    if (mode === "test" || command !== "serve") {
        return baseConfig;
    }

    const port = parseInt(process.env.VITE_PORT ?? "", 10);
    if (!isValidPort(port)) {
        throw new Error(`Invalid VITE_PORT: ${process.env.VITE_PORT}`);
    }

    const apiPort = parseInt(process.env.VITE_API_PORT ?? "", 10);
    if (!isValidPort(apiPort)) {
        throw new Error(`Invalid VITE_API_PORT: ${process.env.VITE_API_PORT}`);
    }

    const commonOptions: Pick<ServerOptions, "port" | "https" | "proxy"> = {
        port,
        proxy: {
            "/api": `http://localhost:${apiPort}`,
        },
    };

    return {
        ...baseConfig,
        server: { ...commonOptions },
        preview: { ...commonOptions },
    };
});
