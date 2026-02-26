import { Button, Container, Stack, Text, Title } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { format } from "date-fns";
import * as z from "zod";

const userSchema = z.object({
    age: z.number().min(0).max(150),
    email: z.email(),
    name: z.string().min(1),
});

const validUser = userSchema.safeParse({ age: 30, email: "alice@example.com", name: "Alice" });
const invalidUser = userSchema.safeParse({ age: -1, email: "not-an-email", name: "" });

const App = () => {
    const today = format(new Date(), "PPPP");

    return (
        <Container py="xl" size="sm">
            <Stack>
                <Title order={1}>Hello World</Title>
                <Text>Today is {today}</Text>
                <Title order={2}>Zod Validation</Title>
                <Text>Valid user: {validUser.success ? "passed" : "failed"}</Text>
                <Text>Invalid user: {invalidUser.success ? "passed" : "failed"}</Text>
                <Button
                    onClick={() => {
                        notifications.show({
                            message: "This is a Mantine notification!",
                            title: "Hello",
                        });
                    }}
                >
                    Show Notification
                </Button>
            </Stack>
        </Container>
    );
};

export default App;
