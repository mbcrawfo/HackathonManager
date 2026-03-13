import type { paths } from "./schema.gen";

import createClient from "openapi-fetch";

const client = createClient<paths>();

export default client;
