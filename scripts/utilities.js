import { execSync } from "child_process";
import net from "net";

export const findAvailablePort = () => {
    return new Promise((resolve, reject) => {
        const server = net.createServer();
        server.unref();
        server.on("error", reject);
        server.listen(0, () => {
            const { port } = server.address();
            server.close(() => {
                resolve(port);
            });
        });
    });
};

export const cleanupContainer = (containerId) => {
    try {
        console.log(`Cleaning up container ${containerId}...`);
        execSync(`docker stop ${containerId}`, { stdio: "ignore" });
        execSync(`docker rm ${containerId}`, { stdio: "ignore" });
    } catch (error) {
        console.error(`Error: Failed to cleanup container ${containerId}`);
    }
};

export const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));
