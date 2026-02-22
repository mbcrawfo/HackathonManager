import { fileURLToPath } from "node:url";
import { configDefaults, ConfigEnv, defineConfig, mergeConfig } from "vitest/config";

import viteConfig from "./vite.config";

const vitestConfig = (env: ConfigEnv) => {
    const reporters = ["default", "junit"];
    if (process.env.GITHUB_ACTIONS) {
        reporters.push("github-actions");
    }

    const coverageReporters = ["text", "json", "html"];
    if (process.env.GITHUB_ACTIONS) {
        coverageReporters.push("json-summary");
    }

    return mergeConfig(
        viteConfig(env),
        defineConfig({
            test: {
                coverage: {
                    reporter: coverageReporters,
                    reportOnFailure: true,
                },
                environment: "jsdom",
                exclude: configDefaults.exclude,
                outputFile: "test-results/unit-tests.xml",
                reporters,
                root: fileURLToPath(new URL("./", import.meta.url)),
                setupFiles: ["./src/setup-tests.ts"],
            },
        }),
    );
};

export default vitestConfig;
