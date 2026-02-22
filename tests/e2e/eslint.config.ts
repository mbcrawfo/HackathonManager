import prettierConfig from "eslint-config-prettier";
import pluginPlaywright from "eslint-plugin-playwright";
import { config as tseslintConfig, configs as tseslintConfigs } from "typescript-eslint";

export default tseslintConfig(
    {
        files: ["**/*.ts"],
        name: "e2e/files-to-lint",
    },

    ...tseslintConfigs.recommended,

    {
        ...pluginPlaywright.configs["flat/recommended"],
        files: ["specs/**/*.{test,spec}.{js,ts,jsx,tsx}"],
    },

    prettierConfig,
);
