import { spawn } from "child_process";

const command = process.argv[2];
if (!command) {
    console.error("Please provide a command");
    process.exit(1);
}

let args = [];
if (process.argv.length > 3) {
    args = process.argv.slice(3);
}

spawn("npm", ["run", "--workspace", "src/hackathon-spa", command, "--", ...args], {
    stdio: "inherit",
});
