import { format } from "date-fns";

const App = () => {
    const today = format(new Date(), "PPPP");

    return (
        <>
            <h1>Hello World</h1>
            <p>Today is {today}</p>
        </>
    );
};

export default App;
