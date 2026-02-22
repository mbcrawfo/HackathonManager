// Helper script to run commands in the e2e test workspace.

import { spawn } from "child_process";
import { readFileSync } from "fs";
import { join } from "path";

const e2ePath = "tests/e2e";

const command: string | undefined = process.argv[2];
if (!command) {
    console.error("Please provide a command");
    process.exit(1);
}

let args: string[] = [];
if (process.argv.length > 3) {
    args = process.argv.slice(3);
}

let scripts: Record<string, string>;
try {
    const path = join(process.cwd(), e2ePath, "package.json");
    const packageJson = readFileSync(path, "utf8");
    scripts = JSON.parse(packageJson)?.scripts ?? {};
} catch (error) {
    console.error(`Failed to load package.json from ${e2ePath}:`, (error as Error).message);
    process.exit(1);
}

const workspace = ["--workspace", e2ePath];

if (scripts[command]) {
    spawn("npm", ["run", ...workspace, command, "--", ...args], {
        stdio: "inherit",
    });
} else {
    spawn("npm", [command, ...args, ...workspace], {
        stdio: "inherit",
    });
}
