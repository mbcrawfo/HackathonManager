// Helper script to run SQLFluff in a Docker container.

import { spawn } from "node:child_process";

let slqfluffArgs: string[] = [];
if (process.argv.length > 2) {
    slqfluffArgs = process.argv.slice(2);
}

let dockerArgs: string[] = ["--rm", "--volume", `${process.cwd()}:/sql:rw`, "sqlfluff/sqlfluff:4.0.4"];

dockerArgs = process.env.CI ? ["--user", "root", ...dockerArgs] : ["--interactive", "--tty", ...dockerArgs];

const dockerProcess = spawn("docker", ["run", ...dockerArgs, ...slqfluffArgs], {
    shell: true,
    stdio: "inherit",
});

dockerProcess.on("error", (error) => {
    console.error("Failed to start process:", error.message);
    process.exit(1);
});

dockerProcess.on("close", (exitCode) => {
    process.exit(exitCode ?? 1);
});
