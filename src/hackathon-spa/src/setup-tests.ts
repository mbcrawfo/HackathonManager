/// <reference types="@testing-library/jest-dom" />
import * as matchers from "@testing-library/jest-dom/matchers";
import { expect } from "vitest";

expect.extend(matchers);

// Mantine requires window.matchMedia which jsdom does not implement
if (typeof globalThis.matchMedia !== "function") {
    Object.defineProperty(globalThis, "matchMedia", {
        configurable: true,
        value: (query: string): MediaQueryList =>
            ({
                addEventListener: () => {},
                addListener: () => {},
                dispatchEvent: () => false,
                matches: false,
                media: query,
                onchange: null,
                removeEventListener: () => {},
                removeListener: () => {},
            }) as MediaQueryList,
        writable: true,
    });
}
