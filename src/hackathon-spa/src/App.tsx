import { Button, Container, Loader, NumberInput, Stack, Text, TextInput, Title } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useForm } from "@tanstack/react-form";
import { useQuery } from "@tanstack/react-query";
import { format } from "date-fns";
import * as z from "zod";

const userSchema = z.object({
    age: z.number().min(0).max(150),
    email: z.email(),
    name: z.string().min(1),
});

const App = () => {
    const today = format(new Date(), "PPPP");

    const demoQuery = useQuery({
        queryFn: async () => {
            await new Promise((resolve) => {
                setTimeout(resolve, 500);
            });
            return { message: "Data fetched successfully!" };
        },
        queryKey: ["demo"],
    });

    const form = useForm({
        defaultValues: {
            age: 0,
            email: "",
            name: "",
        },
        onSubmit: ({ value }) => {
            notifications.show({
                message: `Name: ${value.name}, Email: ${value.email}, Age: ${value.age}`,
                title: "Form Submitted",
            });
        },
        validators: {
            onSubmit: userSchema,
        },
    });

    return (
        <Container py="xl" size="sm">
            <Stack>
                <Title order={1}>Hello World</Title>
                <Text>Today is {today}</Text>
                <Title order={2}>Query Demo</Title>
                {demoQuery.isPending ? <Loader size="sm" /> : <Text>{demoQuery.data?.message}</Text>}
                <Title order={2}>User Form</Title>
                <form
                    onSubmit={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                        form.handleSubmit();
                    }}
                >
                    <Stack>
                        <form.Field name="name">
                            {(field) => (
                                <TextInput
                                    error={field.state.meta.errors.map((e) => e?.message).join(", ")}
                                    label="Name"
                                    onBlur={field.handleBlur}
                                    onChange={(e) => {
                                        field.handleChange(e.currentTarget.value);
                                    }}
                                    value={field.state.value}
                                />
                            )}
                        </form.Field>
                        <form.Field name="email">
                            {(field) => (
                                <TextInput
                                    error={field.state.meta.errors.map((e) => e?.message).join(", ")}
                                    label="Email"
                                    onBlur={field.handleBlur}
                                    onChange={(e) => {
                                        field.handleChange(e.currentTarget.value);
                                    }}
                                    value={field.state.value}
                                />
                            )}
                        </form.Field>
                        <form.Field name="age">
                            {(field) => (
                                <NumberInput
                                    error={field.state.meta.errors.map((e) => e?.message).join(", ")}
                                    label="Age"
                                    onBlur={field.handleBlur}
                                    onChange={(value) => {
                                        field.handleChange(typeof value === "number" ? value : 0);
                                    }}
                                    value={field.state.value}
                                />
                            )}
                        </form.Field>
                        <Button type="submit">Submit</Button>
                    </Stack>
                </form>
            </Stack>
        </Container>
    );
};

export default App;
