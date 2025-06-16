const { spawn } = require("child_process");
const command = process.argv[2];

if (!command) {
  console.error("Please provide a command");
  process.exit(1);
}

spawn("npm", ["run", "--workspace", "src/hackathon-spa", command ], {
  stdio: "inherit",
});
