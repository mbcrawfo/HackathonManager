// Helper script to run SQLFluff in a Docker container.

import { spawn } from "child_process";

let slqfluffArgs: string[] = [];
if (process.argv.length > 2) {
    slqfluffArgs = process.argv.slice(2);
}

let dockerArgs: string[] = ["--rm", "--volume", `${process.cwd()}:/sql:rw`, "sqlfluff/sqlfluff:4.0.4"];

if (process.env.CI) {
    dockerArgs = ["--user", "root", ...dockerArgs];
} else {
    dockerArgs = ["--interactive", "--tty", ...dockerArgs];
}

const dockerProcess = spawn("docker", ["run", ...dockerArgs, ...slqfluffArgs], {
    stdio: "inherit",
    shell: true,
});

dockerProcess.on("error", (error) => {
    console.error("Failed to start process:", error.message);
    process.exit(1);
});

dockerProcess.on("close", (exitCode) => {
    process.exit(exitCode ?? 1);
});
