// Helper script to run commands in project workspaces.

import { spawn } from "node:child_process";
import { readFileSync } from "node:fs";
import { join } from "node:path";

const workspace: string | undefined = process.argv[2];
if (!workspace) {
    console.error("Please specify a workspace");
    process.exit(1);
}

const command: string | undefined = process.argv[3];
if (!command) {
    console.error("Please provide a command");
    process.exit(1);
}

let commandArgs: string[] = [];
if (process.argv.length > 4) {
    commandArgs = process.argv.slice(3);
}

let scripts: Record<string, string>;
try {
    const path = join(process.cwd(), workspace, "package.json");
    const packageJson = readFileSync(path, "utf8");
    scripts = JSON.parse(packageJson)?.scripts ?? {};
} catch (error) {
    console.error(`Failed to load package.json from ${workspace}:`, (error as Error).message);
    process.exit(1);
}

const workspaceArgs = ["--workspace", workspace];
const npmArgs = scripts[command]
    ? ["run", ...workspaceArgs, command, "--", ...commandArgs]
    : [command, ...commandArgs, ...workspaceArgs];

const npmProcess = spawn("npm", npmArgs, {
    stdio: "inherit",
});

npmProcess.on("error", (error) => {
    console.error("Failed to start process:", error.message);
    process.exit(1);
});

npmProcess.on("close", (exitCode) => {
    process.exit(exitCode ?? 1);
});
