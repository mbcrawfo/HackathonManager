import { spawnSync } from "child_process";
import path from "path";
import process from "process";

import { sleep } from "./utilities.ts";

// Manages the Docker Compose lifecycle for running E2E tests against the full application stack.
// Starts the compose stack, waits for services to be healthy, runs Playwright, then tears down.

const composeDir = path.join(process.cwd(), "tests", "e2e");
const composeFile = path.join(composeDir, "compose.yml");
const webPort = process.env.WEB_PORT ?? "5100";
const baseUrl = `http://localhost:${webPort}`;
const healthUrl = `${baseUrl}/health`;

const run = (command: string, args: string[], env?: NodeJS.ProcessEnv): number => {
    const result = spawnSync(command, args, {
        stdio: "inherit",
        shell: true,
        env: env ?? process.env,
    });

    if (result.error) {
        throw result.error;
    }

    return result.status ?? 1;
};

const compose = (...args: string[]): void => {
    const exitCode = run("docker", ["compose", "--file", composeFile, "--env-file", "/dev/null", ...args]);
    if (exitCode !== 0) {
        throw new Error(`docker compose ${args[0]} failed with exit code ${exitCode}`);
    }
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
        } else {
            upArgs.push("--build");
        }
        console.log("Starting E2E Docker stack...");
        compose(...upArgs);

        // Verify the app is responding to HTTP requests
        console.log("Waiting for app to become healthy...");
        await waitForHealthy(healthUrl, 30, 1000);

        console.log(`E2E stack is ready at ${baseUrl}`);

        // Run Playwright, forwarding extra CLI args
        const playwrightArgs = process.argv.slice(2);
        const args = ["--workspace", "tests/e2e", "playwright", "test", ...playwrightArgs];

        console.log(`Running: npx ${args.join(" ")}`);
        exitCode = run("npx", args, {
            ...process.env,
            BASE_URL: baseUrl,
        });
    } catch (err) {
        console.error("Error:", (err as Error).message);
        exitCode = 1;
    } finally {
        cleanup();
    }

    process.exit(exitCode);
};

main().catch(console.error);
