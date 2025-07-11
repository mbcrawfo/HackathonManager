import { spawn } from "child_process";

let slqfluffArgs = [];
if (process.argv.length > 2) {
    slqfluffArgs = process.argv.slice(2);
}

let dockerArgs = ["--rm", "--volume", `${process.cwd()}:/sql:rw`, "sqlfluff/sqlfluff:3.4.1"];

if (process.env.CI) {
    dockerArgs = ["--user", "root", "--tty", ...dockerArgs];
} else {
    dockerArgs = ["--interactive", "--tty", ...dockerArgs];
}

spawn("docker", ["run", ...dockerArgs, ...slqfluffArgs], {
    stdio: "inherit",
});
