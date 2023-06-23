# Cosmos Integrated Cache Test

A simple C# demo application that uses the Azure Cosmos DB with integrated cache feature.

## Features

* End-to-end demo application showing how to use `direct` and `gateway` modes when connecting to Azure Cosmos DB
* Working sample code for using the dedicated gateway and `MaxIntegratedCacheStaleness` mechanism
* Comparison of Request Unit charges when querying using a `direct` vs. `gateway` connection
* Demonstration of cache behavior when using the included Postman collection

## Getting Started

### Prerequisites

* [A Cosmos DB account with a dedicated gateway](https://learn.microsoft.com/en-us/azure/cosmos-db/dedicated-gateway)

  * Create a database
  * Create a container with the partition key set to `/customKey`.

### Installation

* [.Net 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
* [Visual Studio Code](https://code.visualstudio.com/Download)
* [Postman](https://www.postman.com/downloads/)

### Quickstart

1. Clone this repository
2. Open the root folder in VS Code
3. Copy `example.settings.json` and populate it with values from the Cosmos DB Account you created in [Prerequisites](#prerequisites). Save it as `local.settings.json`. The application settings are explaned below.

```jsonc
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "AzureWebJobsDashboard": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "COSMOS_DBNAME": "<the name of your database>", // this should match the database name on the Cosmos DB account you're using for local development
    "COSMOS_CONTAINER_NAME": "<the name of your container>", // this should match the container name on the database you're using for local development
    "QUERY_CACHE_DURATION_HOURS": 1 // you can change this if you want a longer duration
  },
  "ConnectionStrings": {
    "COSMOS_GATEWAY_CONNECTION_STRING": "<your primary dedicated gateway Comsos connection string>", // retrieve this value from the Cosmos DB Account you're using for local development
    "COSMOS_CONNECTION_STRING": "<your primary Cosmos connection string>"
  },
  "Host": {
    "CORS": "*"
  }
}
```

In order to hit the integrated cache in Cosmos, you must use the connection string value for the dedicated gateway. This value can be retrieved in Azure Portal on the Comsos DB account under `Settings > Keys > Read-only Keys`. Copy the value for `PRIMARY READ-ONLY DEDICATED GATEWAY CONNECTION STRING` and paste it into your `local.settings.json` file.

In order to seed data via the `CreateTestItemsByRefids` function, you will need a direct connection string with write permissions. This value can be retrieved in Azure Portal on the Cosmos DB account under `Settings > Keys > Read-write keys`. Copy the value for `PRIMARY CONNECTION STRING` and paste it into your `local.settings.json` file.

Values for `COSMOS_EFLS_DBNAME` and `COSMOS_EFLS_CONTAINER_NAME` can be located on your Cosmos DB account under `Data Explorer`.

## Demo

To run the demo, follow these steps:

(Add steps to start up the demo)

1. Create and save local settings as [described above](#quickstart)
2. On the command line navigate to the directory containing AzureCosmosDbWithIntegratedCacheSample.csproj and enter this command:

> func start

Alternatively, you can run using the VS Code debugger.

**Note:** If you get the error `Failed to verify "AzureWebJobsStorage" connection specified in "local.settings.json". Is the local emulator installed and running?`, use the Command Palette (`View > Command Palette`) and select `Azurite: Start`.

## Swagger

This project includes Swagger definitions. When you run the project locally, you will see a list of endpoints in the terminal, including Swagger endpoints. The `/swagger/ui` endpoint exposes an interface to call the the available Emission Factor Library Service functions. The `Try It Out` feature lets you send requests that hit your locally-running project. You can retrieve valid `refId` values from documents in your Cosmos DB container to send in the bodies of your requests.

The Swagger annotations for each function explain a function's purpose as well as the structure of the request, what status codes are returned, and the structure of the response.

## Postman Collection

See the [PostmanCollectionOverview](./docs/PosmanCollectionOverview.md) document to understand how the included Postman collection walks through a general exploration of Cosmos DB with Integrated cache.

## Resources

* [Azure Cosmos DB with Integrated Cache](https://learn.microsoft.com/en-us/azure/cosmos-db/integrated-cache)
