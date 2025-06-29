import { execSync } from "child_process";
import fs from "fs";
import path, { join } from "path";

import { cleanupContainer, findAvailablePort, sleep } from "./utilities.js";

const postgresImage = "postgres:18beta1";
const tblsImage = "k1low/tbls";
const databaseDocsPath = path.join(process.cwd(), "docs", "database");

console.log("Ensuring docs directory exists...");
if (!fs.existsSync(databaseDocsPath)) {
    fs.mkdirSync(databaseDocsPath, { recursive: true });
}

const main = async () => {
    let error = false;
    let containerId = null;

    try {
        const port = await findAvailablePort();

        const dockerRunCommand = [
            "docker",
            "run",
            "--detach",
            "--publish",
            `${port}:5432`,
            "--env",
            "POSTGRES_PASSWORD=postgres",
            "--env",
            "POSTGRES_DB=hackathon",
            postgresImage,
        ];

        console.log("Starting Postgres container...");
        containerId = execSync(dockerRunCommand.join(" "), { encoding: "utf8" }).trim();

        console.log("Waiting for Postgres to be ready...");
        let retries = 30;
        while (retries > 0) {
            try {
                execSync(`docker exec ${containerId} pg_isready -U postgres`, { stdio: "ignore" });
                break;
            } catch {
                retries -= 1;
                if (retries === 0) {
                    // Need to throw so that cleanup can run on error.
                    // noinspection ExceptionCaughtLocallyJS
                    throw new Error("Postgres failed to start within timeout");
                }
                await sleep(1000);
            }
        }

        const dotnetRunCommand = [
            "dotnet",
            "run",
            `"Host=localhost;Port=${port};Database=hackathon;Username=postgres;Password=postgres"`,
        ];

        console.log("Running database migrations...");
        execSync(dotnetRunCommand.join(" "), {
            stdio: "inherit",
            cwd: join(process.cwd(), "src", "HackathonManager.Migrator"),
        });

        const tblsRunCommand = [
            "docker",
            "run",
            ...(process.env.CI ? ["--user", "root"] : []),
            "--rm",
            "--network",
            "host",
            "--volume",
            `${databaseDocsPath}:/docs:rw`,
            tblsImage,
            "doc",
            `"postgres://postgres:postgres@localhost:${port}/hackathon?sslmode=disable"`,
            "/docs",
            "--adjust-table",
        ];

        console.log("Generating database documentation...");
        execSync(tblsRunCommand.join(" "), { stdio: "inherit" });

        console.log("Database documentation generated successfully!");
    } catch (error) {
        error = true;
        console.error("Error:", error.message);
    } finally {
        if (containerId) {
            cleanupContainer(containerId);
        }

        if (error) {
            process.exit(1);
        }
    }
};

main().catch(console.error);
