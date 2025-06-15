import { readFileSync } from "fs";
import { fileURLToPath, URL } from "node:url";
import vue from "@vitejs/plugin-vue";
import { defineConfig, loadEnv, ServerOptions } from "vite";
import vueDevTools from "vite-plugin-vue-devtools";

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
    process.env = { ...process.env, ...loadEnv(mode, process.cwd()) };

    if (!process.env.VITE_CERT_PATH || !process.env.VITE_KEY_PATH) {
        throw new Error("VITE_CERT_PATH and VITE_KEY_PATH must be set.");
    }

    const port = parseInt(process.env.VITE_PORT ?? "44300", 10);
    if (isNaN(port) || port <= 0 || port > 65535) {
        throw new Error(`Invalid VITE_PORT: ${process.env.VITE_PORT}`);
    }

    const apiPort = parseInt(process.env.VITE_API_PORT ?? "44301", 10);
    if (isNaN(apiPort) || apiPort <= 0 || apiPort > 65535) {
        throw new Error(`Invalid VITE_API_PORT: ${process.env.VITE_API_PORT}`);
    }

    const sharedServerAndPreviewOptions: Pick<ServerOptions, "https" | "port" | "proxy"> = {
        port,
        https: {
            cert: readFileSync(process.env.VITE_CERT_PATH),
            key: readFileSync(process.env.VITE_KEY_PATH),
        },
        proxy: {
            "/api": {
                target: `https://localhost:${apiPort}`,
                changeOrigin: true,
            },
        },
    };

    return {
        plugins: [vue(), vueDevTools()],
        resolve: {
            alias: {
                "@": fileURLToPath(new URL("./src", import.meta.url)),
            },
        },
        server: { ...sharedServerAndPreviewOptions },
        preview: { ...sharedServerAndPreviewOptions },
    };
});
