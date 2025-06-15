import { fileURLToPath } from "node:url";
import { configDefaults, ConfigEnv, defineConfig, mergeConfig } from "vitest/config";

import viteConfig from "./vite.config";

export default (env: ConfigEnv) => {
    return mergeConfig(
        viteConfig(env),
        defineConfig({
            test: {
                environment: "jsdom",
                exclude: [...configDefaults.exclude, "e2e/**"],
                root: fileURLToPath(new URL("./", import.meta.url)),
                reporters: ["default", "junit"],
                outputFile: "test-results/unit-tests.xml",
            },
        }),
    );
};
