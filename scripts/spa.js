import { spawn } from "child_process";
import { readFileSync } from "fs";
import { join } from "path";

const spaPath = "src/hackathon-spa";

const command = process.argv[2];
if (!command) {
    console.error("Please provide a command");
    process.exit(1);
}

let args = [];
if (process.argv.length > 3) {
    args = process.argv.slice(3);
}

let scripts;
try {
    const path = join(process.cwd(), spaPath, "package.json");
    const packageJson = readFileSync(path, "utf8");
    scripts = JSON.parse(packageJson)?.scripts ?? {};
} catch (error) {
    console.error(`Failed to load package.json from ${spaPath}:`, error.message);
    process.exit(1);
}

const workspace = ["--workspace", spaPath];

if (scripts[command]) {
    spawn("npm", ["run", ...workspace, command, "--", ...args], {
        stdio: "inherit",
    });
} else {
    spawn("npm", [command, ...args, ...workspace], {
        stdio: "inherit",
    });
}
