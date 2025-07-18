import { execSync } from "child_process";
import net from "net";

export const findAvailablePort = (): Promise<number> => {
    return new Promise((resolve, reject) => {
        const server = net.createServer();
        server.unref();
        server.on("error", reject);
        server.listen(0, () => {
            const { port } = server.address() as net.AddressInfo;
            server.close(() => {
                resolve(port);
            });
        });
    });
};

export const cleanupContainer = (containerId: string): void => {
    try {
        console.log(`Cleaning up container ${containerId}...`);
        execSync(`docker stop ${containerId}`, { stdio: "ignore" });
        execSync(`docker rm ${containerId}`, { stdio: "ignore" });
    } catch (error) {
        console.error(`Error: Failed to cleanup container ${containerId}`);
    }
};

export const sleep = (ms: number): Promise<void> => new Promise((resolve) => setTimeout(resolve, ms));
