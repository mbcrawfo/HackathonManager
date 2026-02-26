/// <reference types="@testing-library/jest-dom" />
import * as matchers from "@testing-library/jest-dom/matchers";
import { expect } from "vitest";

expect.extend(matchers);

// Mantine requires window.matchMedia which jsdom does not implement
Object.defineProperty(globalThis, "matchMedia", {
    value: (query: string) => ({
        addEventListener: () => {},
        addListener: () => {},
        dispatchEvent: () => false,
        matches: false,
        media: query,
        onchange: null,
        removeEventListener: () => {},
        removeListener: () => {},
    }),
    writable: true,
});
