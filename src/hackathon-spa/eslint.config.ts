import { recommended as eslintCommentsRecommended } from "@eslint-community/eslint-plugin-eslint-comments/configs";
import eslintReact from "@eslint-react/eslint-plugin";
import pluginVitest from "@vitest/eslint-plugin";
import prettierConfig from "eslint-config-prettier";
import { flatConfigs as importXConfigs } from "eslint-plugin-import-x";
import jsxA11y from "eslint-plugin-jsx-a11y";
import { configs as perfectionistConfigs } from "eslint-plugin-perfectionist";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefreshPlugin from "eslint-plugin-react-refresh";
import { configs as regexpConfigs } from "eslint-plugin-regexp";
import testingLibrary from "eslint-plugin-testing-library";
import unicorn from "eslint-plugin-unicorn";
import zodPlugin from "eslint-plugin-zod";
import { globalIgnores } from "eslint/config";
import { config as tseslintConfig, configs as tseslintConfigs } from "typescript-eslint";

export default tseslintConfig(
    {
        files: ["**/*.{ts,tsx}"],
        name: "app/files-to-lint",
    },

    globalIgnores(["**/dist/**", "**/dist-ssr/**", "**/coverage/**"]),

    ...tseslintConfigs.recommended,

    eslintReact.configs["recommended-typescript"],

    reactHooks.configs.flat.recommended,

    jsxA11y.flatConfigs.recommended,

    importXConfigs.recommended,
    importXConfigs.typescript,
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

    perfectionistConfigs["recommended-natural"],
    { rules: { "perfectionist/sort-imports": "off" } },

    regexpConfigs.recommended,

    {
        plugins: { "react-refresh": reactRefreshPlugin },
        rules: {
            "react-refresh/only-export-components": ["warn", { allowConstantExport: true }],
        },
    },

    {
        ...pluginVitest.configs.recommended,
        files: ["src/**/__tests__/*", "src/**/*.test.{ts,tsx}"],
    },

    {
        ...testingLibrary.configs["flat/react"],
        files: ["src/**/__tests__/*", "src/**/*.test.{ts,tsx}"],
    },

    zodPlugin.configs.recommended,

    prettierConfig,
);
