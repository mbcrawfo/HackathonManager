import eslintReact from "@eslint-react/eslint-plugin";
import pluginVitest from "@vitest/eslint-plugin";
import prettierConfig from "eslint-config-prettier";
import jsxA11y from "eslint-plugin-jsx-a11y";
import pluginPlaywright from "eslint-plugin-playwright";
import reactRefresh from "eslint-plugin-react-refresh";
import testingLibrary from "eslint-plugin-testing-library";
import { globalIgnores } from "eslint/config";
import tseslint from "typescript-eslint";

export default tseslint.config(
    {
        name: "app/files-to-lint",
        files: ["**/*.{ts,tsx}"],
    },

    globalIgnores(["**/dist/**", "**/dist-ssr/**", "**/coverage/**"]),

    ...tseslint.configs.recommended,

    eslintReact.configs["recommended-typescript"],

    jsxA11y.flatConfigs.recommended,

    {
        plugins: { "react-refresh": reactRefresh },
        rules: {
            "react-refresh/only-export-components": ["warn", { allowConstantExport: true }],
        },
    },

    {
        ...pluginVitest.configs.recommended,
        files: ["src/**/__tests__/*"],
    },

    {
        ...testingLibrary.configs["flat/react"],
        files: ["src/**/__tests__/*"],
    },

    {
        ...pluginPlaywright.configs["flat/recommended"],
        files: ["e2e/**/*.{test,spec}.{js,ts,jsx,tsx}"],
    },

    prettierConfig,
);
