// Helper script to run commands in the hackathon SPA workspace.

import { spawn } from "node:child_process";
import { readFileSync } from "node:fs";
import { join } from "node:path";

const spaPath = "src/hackathon-spa";

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
    const path = join(process.cwd(), spaPath, "package.json");
    const packageJson = readFileSync(path, "utf8");
    scripts = JSON.parse(packageJson)?.scripts ?? {};
} catch (error) {
    console.error(`Failed to load package.json from ${spaPath}:`, (error as Error).message);
    process.exit(1);
}

const workspace = ["--workspace", spaPath];

const child = scripts[command]
    ? spawn("npm", ["run", ...workspace, command, "--", ...args], {
          stdio: "inherit",
      })
    : spawn("npm", [command, ...args, ...workspace], {
          stdio: "inherit",
      });

child.on("error", (err) => {
    console.error("Failed to start process:", err.message);
    process.exit(1);
});
child.on("close", (code) => {
    process.exit(code ?? 1);
});
