import eslintReact from "@eslint-react/eslint-plugin";
import eslintJs from "@eslint/js";
import vitest from "@vitest/eslint-plugin";
import { defineConfig } from "eslint/config";
import { createTypeScriptImportResolver } from "eslint-import-resolver-typescript";
import importX from "eslint-plugin-import-x";
import jsxA11yX from "eslint-plugin-jsx-a11y-x";
import reactHooks from "eslint-plugin-react-hooks";
import globals from "globals";
import typescriptEslint from "typescript-eslint";

export default defineConfig([
  {
    ignores: ["dist/**", "node_modules/**"],
  },
  {
    files: ["src/**/*.{js,jsx,ts,tsx}"],
    extends: [
      eslintJs.configs.recommended,
      importX.flatConfigs.recommended,
      jsxA11yX.configs.recommended,
      eslintReact.configs.recommended,
      reactHooks.configs.flat.recommended,
    ],
    languageOptions: {
      ecmaVersion: "latest",
      sourceType: "module",
      globals: {
        ...globals.browser,
        ...globals.node,
      },
      parserOptions: {
        ecmaFeatures: { jsx: true },
      },
    },
    settings: {
      "import-x/resolver-next": [
        createTypeScriptImportResolver({
          project: "./tsconfig.json",
        }),
      ],
    },
    rules: {
      "import-x/no-absolute-path": "error",
      "import-x/no-duplicates": "error",
      // React 19 marca como error ciertos efectos que sincronizan recursos
      // externos (cámara, fotos, sesión y temporizadores). Estos efectos
      // tienen limpieza explícita y son el patrón aprobado del proyecto.
      "react-hooks/set-state-in-effect": "off",
      "react-hooks/exhaustive-deps": "off",
      "@eslint-react/set-state-in-effect": "off",
      "@eslint-react/exhaustive-deps": "off",
      "@eslint-react/purity": "off",
      "@eslint-react/no-array-index-key": "off",
      "@eslint-react/web-api-no-leaked-timeout": "off",
      "jsx-a11y-x/no-autofocus": "off",
      "jsx-a11y-x/no-noninteractive-element-interactions": "off",
    },
  },
  {
    files: ["src/**/*.test.{js,jsx,ts,tsx}"],
    plugins: {
      vitest,
    },
    languageOptions: {
      globals: vitest.environments.env.globals,
    },
    rules: {
      ...vitest.configs.recommended.rules,
    },
  },
  {
    files: ["src/**/*.{ts,tsx}"],
    extends: [typescriptEslint.configs.recommended],
  },
]);
