import { recommended as eslintCommentsRecommended } from "@eslint-community/eslint-plugin-eslint-comments/configs";
import eslintReact from "@eslint-react/eslint-plugin";
import pluginVitest from "@vitest/eslint-plugin";
import prettierConfig from "eslint-config-prettier";
import importXPlugin from "eslint-plugin-import-x";
import jsxA11y from "eslint-plugin-jsx-a11y";
import perfectionist from "eslint-plugin-perfectionist";
import pluginPlaywright from "eslint-plugin-playwright";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefreshPlugin from "eslint-plugin-react-refresh";
import regexp from "eslint-plugin-regexp";
import testingLibrary from "eslint-plugin-testing-library";
import unicorn from "eslint-plugin-unicorn";
import { globalIgnores } from "eslint/config";
import tseslint from "typescript-eslint";

export default tseslint.config(
    {
        files: ["**/*.{ts,tsx}"],
        name: "app/files-to-lint",
    },

    globalIgnores(["**/dist/**", "**/dist-ssr/**", "**/coverage/**"]),

    ...tseslint.configs.recommended,

    eslintReact.configs["recommended-typescript"],

    reactHooks.configs.flat.recommended,

    jsxA11y.flatConfigs.recommended,

    importXPlugin.flatConfigs.recommended,
    importXPlugin.flatConfigs.typescript,
    { rules: { "import-x/order": "off" } },

    eslintCommentsRecommended,

    unicorn.configs.recommended,
    {
        rules: {
            "unicorn/filename-case": ["error", { cases: { camelCase: true, kebabCase: true, pascalCase: true } }],
            "unicorn/no-array-for-each": "off",
            "unicorn/no-array-reduce": "off",
            "unicorn/no-null": "off",
            "unicorn/prevent-abbreviations": "off",
        },
    },

    perfectionist.configs["recommended-natural"],
    { rules: { "perfectionist/sort-imports": "off" } },

    regexp.configs.recommended,

    {
        plugins: { "react-refresh": reactRefreshPlugin },
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
