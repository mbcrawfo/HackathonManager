// Helper script to run commands in project workspaces via pnpm.

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
    commandArgs = process.argv.slice(4);
}

let packageName: string | undefined;
let scripts: Record<string, string>;
try {
    const path = join(process.cwd(), workspace, "package.json");
    const packageJson = JSON.parse(readFileSync(path, "utf8"));
    packageName = packageJson?.name;
    scripts = packageJson?.scripts ?? {};
    if (!packageName) {
        throw new Error(`package.json in ${workspace} is missing a "name" field`);
    }
} catch (error) {
    console.error(`Failed to load package.json from ${workspace}:`, (error as Error).message);
    process.exit(1);
}

// When the command matches a script, run it via `pnpm --filter <name> run <command> <args>`.
// Otherwise pass the command straight through to pnpm scoped to the workspace,
// e.g. `pnpm --filter <name> <command> <args>` (mirrors the old `npm <command> --workspace`).
// Note: no `--` separator is used. Unlike npm, pnpm forwards a literal `--` to the script,
// which downstream tools (e.g. vite) would treat as an end-of-options marker.
const pnpmArgs = scripts[command]
    ? ["--filter", packageName, "run", command, ...commandArgs]
    : ["--filter", packageName, command, ...commandArgs];

const pnpmProcess = spawn("pnpm", pnpmArgs, {
    stdio: "inherit",
    shell: true,
});

pnpmProcess.on("error", (error) => {
    console.error("Failed to start process:", error.message);
    process.exit(1);
});

pnpmProcess.on("close", (exitCode) => {
    process.exit(exitCode ?? 1);
});
