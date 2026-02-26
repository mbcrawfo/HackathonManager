import { MantineProvider } from "@mantine/core";
import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import App from "../App";

describe("App", () => {
    it("renders Hello World", () => {
        render(
            <MantineProvider>
                <App />
            </MantineProvider>,
        );
        expect(screen.getByRole("heading", { level: 1 })).toHaveTextContent("Hello World");
    });
});
