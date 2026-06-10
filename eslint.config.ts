import { recommended as eslintCommentsRecommended } from "@eslint-community/eslint-plugin-eslint-comments/configs";
import eslintReact from "@eslint-react/eslint-plugin";
import pluginQuery from "@tanstack/eslint-plugin-query";
import pluginRouter from "@tanstack/eslint-plugin-router";
import pluginVitest from "@vitest/eslint-plugin";
import prettierConfig from "eslint-config-prettier";
import { flatConfigs as importXConfigs } from "eslint-plugin-import-x";
import jsxA11y from "eslint-plugin-jsx-a11y";
import { configs as perfectionistConfigs } from "eslint-plugin-perfectionist";
import pluginPlaywright from "eslint-plugin-playwright";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefreshPlugin from "eslint-plugin-react-refresh";
import { configs as regexpConfigs } from "eslint-plugin-regexp";
import testingLibrary from "eslint-plugin-testing-library";
import unicorn from "eslint-plugin-unicorn";
import zodPlugin from "eslint-plugin-zod";
import { globalIgnores } from "eslint/config";
import { config as tseslintConfig, configs as tseslintConfigs } from "typescript-eslint";

// Shared lint standards applied to every TypeScript file in the repo (SPA, scripts, and e2e tests).
// The SPA layers React-specific configs on top of this base; everything else uses it as-is.
const sharedBase = [
    ...tseslintConfigs.recommended,

    importXConfigs.recommended,
    importXConfigs.typescript,
    // perfectionist owns import ordering, so import-x's own ordering rule stays off.
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

    // perfectionist sorts imports (replacing the former Prettier import-sort plugin).
    perfectionistConfigs["recommended-natural"],

    regexpConfigs.recommended,
];

const spaTestFiles = ["src/hackathon-spa/src/**/__tests__/*", "src/hackathon-spa/src/**/*.test.{ts,tsx}"];

export default tseslintConfig(
    globalIgnores([
        "**/dist/**",
        "**/dist-ssr/**",
        "**/coverage/**",
        "**/playwright-report/**",
        "**/test-results/**",
        "**/*.gen.ts",
    ]),

    // Node tooling scripts and the ESLint config itself.
    {
        extends: sharedBase,
        files: ["scripts/**/*.ts", "eslint.config.ts"],
        name: "scripts",
        // CLI scripts legitimately terminate the process.
        rules: { "unicorn/no-process-exit": "off" },
    },

    // React SPA.
    {
        extends: [
            ...sharedBase,
            eslintReact.configs["recommended-typescript"],
            reactHooks.configs.flat.recommended,
            jsxA11y.flatConfigs.recommended,
            zodPlugin.configs.recommended,
            ...pluginQuery.configs["flat/recommended"],
            ...pluginRouter.configs["flat/recommended"],
        ],
        files: ["src/hackathon-spa/**/*.{ts,tsx}"],
        name: "spa",
        plugins: { "react-refresh": reactRefreshPlugin },
        rules: {
            "react-refresh/only-export-components": ["warn", { allowConstantExport: true }],
        },
    },
    {
        files: ["src/hackathon-spa/src/routes/**/*.tsx"],
        rules: { "react-refresh/only-export-components": "off" },
    },
    {
        ...pluginVitest.configs.recommended,
        files: spaTestFiles,
    },
    {
        ...testingLibrary.configs["flat/react"],
        files: spaTestFiles,
    },
    {
        // __tests__ is a conventional directory name that fails unicorn's directory casing check.
        files: spaTestFiles,
        rules: { "unicorn/filename-case": "off" },
    },

    // Playwright e2e tests.
    {
        extends: sharedBase,
        files: ["tests/e2e/**/*.ts"],
        name: "e2e",
    },
    {
        ...pluginPlaywright.configs["flat/recommended"],
        files: ["tests/e2e/specs/**/*.{test,spec}.{js,ts,jsx,tsx}"],
    },

    prettierConfig,
);
