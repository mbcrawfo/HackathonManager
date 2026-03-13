// Generates TypeScript types from the backend OpenAPI spec using openapi-typescript.

import { spawn } from "child_process";

const baseUrl = process.argv[2] ?? "http://localhost:5000";
const swaggerUrl = `${baseUrl}/swagger/v1/swagger.json`;
const outputPath = "src/hackathon-spa/src/api/schema.gen.ts";

const npxCommand = process.platform === "win32" ? "npx.cmd" : "npx";
const child = spawn(npxCommand, ["openapi-typescript", swaggerUrl, "--output", outputPath], {
    stdio: "inherit",
});

child.on("error", (error) => {
    console.error("Failed to start process:", error.message);
    process.exit(1);
});

child.on("close", (exitCode) => {
    process.exit(exitCode ?? 1);
});
