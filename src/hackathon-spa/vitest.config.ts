import { fileURLToPath } from "node:url";
import { configDefaults, ConfigEnv, defineConfig, mergeConfig } from "vitest/config";

import viteConfig from "./vite.config";

export default (env: ConfigEnv) => {
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
                environment: "jsdom",
                exclude: [...configDefaults.exclude, "e2e/**"],
                root: fileURLToPath(new URL("./", import.meta.url)),
                reporters,
                outputFile: "test-results/unit-tests.xml",
                coverage: {
                    reporter: coverageReporters,
                    reportOnFailure: true,
                },
            },
        }),
    );
};
