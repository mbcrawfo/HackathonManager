import pluginVitest from "@vitest/eslint-plugin";
import prettierConfig from "eslint-config-prettier";
import pluginPlaywright from "eslint-plugin-playwright";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";
import { globalIgnores } from "eslint/config";
import tseslint from "typescript-eslint";

export default tseslint.config(
    {
        name: "app/files-to-lint",
        files: ["**/*.{ts,tsx}"],
    },

    globalIgnores(["**/dist/**", "**/dist-ssr/**", "**/coverage/**"]),

    ...tseslint.configs.recommended,

    {
        plugins: {
            "react-hooks": reactHooks,
            "react-refresh": reactRefresh,
        },
        rules: {
            ...reactHooks.configs.recommended.rules,
            "react-refresh/only-export-components": ["warn", { allowConstantExport: true }],
        },
    },

    {
        ...pluginVitest.configs.recommended,
        files: ["src/**/__tests__/*"],
    },

    {
        ...pluginPlaywright.configs["flat/recommended"],
        files: ["e2e/**/*.{test,spec}.{js,ts,jsx,tsx}"],
    },

    prettierConfig,
);
