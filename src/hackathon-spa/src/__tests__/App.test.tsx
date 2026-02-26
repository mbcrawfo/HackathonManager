import { MantineProvider } from "@mantine/core";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { createRouter, RouterProvider } from "@tanstack/react-router";
import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { routeTree } from "../routeTree.gen";

const createTestQueryClient = () =>
    new QueryClient({
        defaultOptions: {
            queries: {
                retry: false,
            },
        },
    });

describe("App", () => {
    it("renders Hello World", async () => {
        const queryClient = createTestQueryClient();
        const router = createRouter({ routeTree });

        render(
            <QueryClientProvider client={queryClient}>
                <MantineProvider>
                    <RouterProvider router={router} />
                </MantineProvider>
            </QueryClientProvider>,
        );

        const heading = await screen.findByRole("heading", { level: 1 });
        expect(heading).toHaveTextContent("Hello World");
    });
});
