import "@mantine/core/styles.layer.css";
import "@mantine/dates/styles.layer.css";
import "@mantine/dropzone/styles.layer.css";
import "@mantine/notifications/styles.layer.css";
import "@mantine/tiptap/styles.layer.css";

import { MantineProvider } from "@mantine/core";
import { ModalsProvider } from "@mantine/modals";
import { Notifications } from "@mantine/notifications";
import { StrictMode } from "react";
import { createRoot } from "react-dom/client";

import App from "./App";

createRoot(document.querySelector("#app")!).render(
    <StrictMode>
        <MantineProvider>
            <ModalsProvider>
                <Notifications />
                <App />
            </ModalsProvider>
        </MantineProvider>
    </StrictMode>,
);
