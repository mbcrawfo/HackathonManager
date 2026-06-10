import createClient from "openapi-fetch";

import type { paths } from "./schema.gen";

const client = createClient<paths>();

export default client;
