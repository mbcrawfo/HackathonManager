import { MantineProvider } from "@mantine/core";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import App from "../App";

const createTestQueryClient = () =>
    new QueryClient({
        defaultOptions: {
            queries: {
                retry: false,
            },
        },
    });

describe("App", () => {
    it("renders Hello World", () => {
        const queryClient = createTestQueryClient();
        render(
            <QueryClientProvider client={queryClient}>
                <MantineProvider>
                    <App />
                </MantineProvider>
            </QueryClientProvider>,
        );
        expect(screen.getByRole("heading", { level: 1 })).toHaveTextContent("Hello World");
    });
});
