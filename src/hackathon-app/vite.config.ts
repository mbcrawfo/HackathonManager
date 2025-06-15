import { readFileSync } from "fs";
import { fileURLToPath, URL } from "node:url";
import vue from "@vitejs/plugin-vue";
import { defineConfig, loadEnv, ServerOptions, UserConfig } from "vite";
import vueDevTools from "vite-plugin-vue-devtools";

const defaultPort = "44300";
const defaultApiPort = "44301";

const isValidPort = (port: number) => !isNaN(port) && port > 0 && port <= 65535;

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
    process.env = { ...process.env, ...loadEnv(mode, process.cwd()) };

    const baseConfig: UserConfig = {
        plugins: [vue(), vueDevTools()],
        resolve: {
            alias: {
                "@": fileURLToPath(new URL("./src", import.meta.url)),
            },
        },
    };

    if (mode === "test") {
        return baseConfig;
    }

    if (!process.env.VITE_CERT_PATH || !process.env.VITE_KEY_PATH) {
        throw new Error("VITE_CERT_PATH and VITE_KEY_PATH must be set.");
    }

    const port = parseInt(process.env.VITE_PORT ?? defaultPort, 10);
    if (!isValidPort(port)) {
        throw new Error(`Invalid VITE_PORT: ${process.env.VITE_PORT}`);
    }

    const apiPort = parseInt(process.env.VITE_API_PORT ?? defaultApiPort, 10);
    if (!isValidPort(apiPort)) {
        throw new Error(`Invalid VITE_API_PORT: ${process.env.VITE_API_PORT}`);
    }

    const commonOptions: Pick<ServerOptions, "port" | "https" | "proxy"> = {
        port,
        https: {
            cert: readFileSync(process.env.VITE_CERT_PATH),
            key: readFileSync(process.env.VITE_KEY_PATH),
        },
        proxy: {
            "/api": `https://localhost:${apiPort}`,
        },
    };

    return {
        ...baseConfig,
        server: { ...commonOptions },
        preview: { ...commonOptions },
    };
});
