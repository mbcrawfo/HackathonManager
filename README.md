## Hackathon Manager

![Build Status](https://github.com/mbcrawfo/HackathonManager/actions/workflows/ci.yml/badge.svg?branch=main)

### Running Locally

You can run the app and explore it locally using docker compose.  Run `docker compose up -d`, and after all containers have been pulled/built and are running, access the app at http://localhost:5000.

This [default configuration](./compose.yml) uses the .Net web application to serve the React SPA files in a single container.  An [alternate configuration](./compose-separate.yml) is provided that simulates hosting the SPA separately from the .Net app, with a reverse proxy that presents the two as a single web app.  You can run this configuration with `docker compose -f compose-separate.yml up -d`.

### Development

Prerequisites: .Net 9 SDK, Node.js v24, and Docker (or a compatible tool such as Orbstack).


1. From root of the repository, run `dotnet tool restore && dotnet restore && npm ci` to restore all tools and packages used by the project.
2. Run `dotnet build` to build the back end and `dotnet test` to ensure that its tests pass.
3. Run `npm run spa test` to ensure that the front end tests pass.
4. Run `docker compose -f dependencies.yml up -d` to start the database and other dependencies required by the app.  By default the back end will automatically update the database when it starts.
5. TODO: E2E tests

By default the back end writes debug logs to the console.  It is highly recommend that you set up [OpenTelemetry Logging](#opentelemetry-logging).  You can also examine the [appsettings.example file](./src/HackathonManager/appsettings.example.json5) to see what other settings are available (such as file logs).  You may customize the settings for your local dev environment by using [.Net User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-9.0) or by creating a `.env.local` file in the [HackathonManager project](./src/HackathonManager).

Once you make any configuration changes, run the back end using `dotnet run`, Visual Studio, Jetbrains Rider, etc.  You can access its Swagger UI at http://localhost:5001/swagger.

Run the Vite dev server for the SPA app using `npm run spa dev`.  It is available at http://localhost:5000, and by default the Vite dev server is used to proxy requests to the back end API.

### OpenTelemetry Logging

The app fully supports OpenTelemetry (OTel) logs, traces, and metrics.  Taking advantage of these tools during local development provides a huge improvement in developer experience compared to the traditionally approach of grepping console output or log files.  Any OTel tools can be used, but the [Aspire Dashboard](#aspire-dashboard) and [Seq](#seq) are two good options that can be run using docker and are easy to start with.

Note that when using OpenTelemetry I recommend enabling verbose logging, at least for the app's logs (verbose logs from Asp.Net, EF, etc. produce a _lot_ of noise).  Good structured logging tools make it easy to find what you need, so it's better to capture a lot data in case you need it rather than wasting time trying to reproduce a rare bug because your log level preventing getting the info you need.

You may also want to set the following environment variables in `.env.local` to prevent OTel from redacting query string parameters.  These must be environment variables, they can not go in user secrets.

```
OTEL_DOTNET_EXPERIMENTAL_ASPNETCORE_DISABLE_URL_QUERY_REDACTION=true
OTEL_DOTNET_EXPERIMENTAL_HTTPCLIENT_DISABLE_URL_QUERY_REDACTION=true
```

#### Aspire Dashboard

The [Microsoft Aspire Dashboard](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/standalone) can be run independently from Aspire projects.  It has an easy to use UI and supports all OTel data, but its search options are somewhat limited and it does not persist data (everything will be wiped when the container restarts).  The following docker command runs the dashboard with auth disabled, exposing its UI on port 5050 and gRPC ingestion on port 4317.

```sh
docker run \
--name aspire-dashboard \
--env "DASHBOARD__FRONTEND__AUTHMODE=Unsecured" \
--env "DASHBOARD__OTLP__AUTHMODE=Unsecured" \
--publish 5050:18888 \
--publish 4317:18889 \
--restart unless-stopped \
--detach \
mcr.microsoft.com/dotnet/aspire-dashboard:9.0
```

Set the following configuration for the backend app in user secrets or `.env.local`:

```json
{
    "OpenTelemetry:AllExporters:Enabled": true,
    "OpenTelemetry:AllExporters:Endpoint": "http://localhost:4317",
    "OpenTelemetry:AllExporters:Protocol": "Grpc"
}
```

#### Seq

[Seq](https://datalust.co/seq) is a powerful tool with more robust capabilities than Aspire Dashboard, but it's more complex and can have a learning curve.  It can persist your logs and traces, but does not yet support OTel metrics.  The following docker command runs Seq with the default user name and password set to `admin`, the UI on port 5050, and the ingestion port 4317.  Note that Seq does not support gRPC unless you set up HTTPS, so this container must use Protobuf ingestion.

```sh
docker run \
--name seq \
--env "ACCEPT_EULA=Y" \
--env "SEQ_FIRSTRUN_ADMINPASSWORDHASH=QNwxcXb5uVV9ZJma8mOK/ZbQzRTEDIrvupQHFnCkEYFzktF1UYWwa1DfjG7tyy4ceGbq3yrfqd2aUrli0EC/Izlyoh/AfJyuYZFD9WE/zAOw" \
--env "SEQ_API_CANONICALURI=http://localhost:5050" \
--publish 5050:80 \
--publish 4317:5341 \
--volume seq-data:/data \
--restart unless-stopped \
--detach \
datalust/seq
```

Set the following configuration for the backend app in user secrets or `.env.local`:

```json
{
    "OpenTelemetry:LogExporter:Enabled": true,
    "OpenTelemetry:LogExporter:Endpoint": "http://localhost:4317/ingest/otlp/v1/logs",
    "OpenTelemetry:LogExporter:Protocol": "HttpProtobuf",
    "OpenTelemetry:TraceExporter:Enabled": true,
    "OpenTelemetry:TraceExporter:Endpoint": "http://localhost:4317/ingest/otlp/v1/traces",
    "OpenTelemetry:TraceExporter:Protocol": "HttpProtobuf"
}
```
