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
        <>
            <h1>Hello World</h1>
            <p>Today is {today}</p>
            <h2>Zod Validation</h2>
            <p>Valid user: {validUser.success ? "passed" : "failed"}</p>
            <p>Invalid user: {invalidUser.success ? "passed" : "failed"}</p>
        </>
    );
};

export default App;
