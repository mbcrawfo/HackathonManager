import type { SpawnSyncReturns } from "child_process";

import { execSync } from "child_process";
import path from "path";
import process from "process";

import { sleep } from "./utilities.ts";

// Manages the Docker Compose lifecycle for running E2E tests against the full application stack.
// Starts the compose stack, waits for services to be healthy, runs Playwright, then tears down.

const composeFile = path.join(process.cwd(), "tests", "e2e", "compose.yml");
const webPort = process.env.WEB_PORT ?? "5100";
const baseUrl = `http://localhost:${webPort}`;
const healthUrl = `${baseUrl}/health`;

const compose = (...args: string[]): void => {
    execSync(["docker", "compose", "--file", composeFile, ...args].join(" "), {
        stdio: "inherit",
    });
};

const cleanup = (): void => {
    console.log("Tearing down E2E Docker stack...");
    try {
        compose("down", "--volumes");
    } catch {
        console.error("Warning: Failed to tear down Docker stack.");
    }
};

const waitForHealthy = async (url: string, retries: number, intervalMs: number): Promise<void> => {
    for (let i = 0; i < retries; i++) {
        try {
            const response = await fetch(url);
            if (response.ok) {
                return;
            }
        } catch {
            // Connection refused or other network error -- keep retrying
        }
        await sleep(intervalMs);
    }
    throw new Error(`Health check failed after ${retries} attempts: ${url}`);
};

const main = async (): Promise<void> => {
    // Register cleanup handlers
    process.on("SIGINT", () => {
        cleanup();
        process.exit(1);
    });
    process.on("SIGTERM", () => {
        cleanup();
        process.exit(1);
    });

    let exitCode = 0;

    try {
        // Clean any stale state
        compose("down", "--volumes");

        // Start the stack
        const upArgs = ["up", "--detach", "--wait"];
        if (process.env.CI) {
            upArgs.push("--no-build");
        }
        console.log("Starting E2E Docker stack...");
        compose(...upArgs);

        // Verify the app is responding to HTTP requests
        console.log("Waiting for app to become healthy...");
        await waitForHealthy(healthUrl, 30, 1000);

        console.log(`E2E stack is ready at ${baseUrl}`);

        // Run Playwright, forwarding extra CLI args
        const playwrightArgs = process.argv.slice(2);
        const playwrightCommand = ["npx", "--workspace", "tests/e2e", "playwright", "test", ...playwrightArgs].join(
            " ",
        );

        console.log(`Running: ${playwrightCommand}`);
        try {
            execSync(playwrightCommand, {
                stdio: "inherit",
                env: {
                    ...process.env,
                    BASE_URL: baseUrl,
                },
            });
        } catch (err) {
            exitCode = (err as SpawnSyncReturns<string>).status ?? 1;
        }
    } catch (err) {
        console.error("Error:", (err as Error).message);
        exitCode = 1;
    } finally {
        cleanup();
    }

    process.exit(exitCode);
};

main().catch(console.error);
