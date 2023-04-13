# Cosmos Integrated Cache Test

**Author:** Laura Lund

## Project Overview

To read a broader overview of my learnings around Cosmos with Integrated Cache, please [click here](./docs/ExploringCosmosWithIntegratedCache.md).

I initially built this as a test project to [investigate cache behavior](/docs/CacheBehaviorInvestigation.md) when a list of items is queried before one or more of those items exists in the Cosmos Db instance. When queries are made using a Dedicated Gateway Connection String, the result of that query is cached with a `MaxIntegratedCacheStaleness` of some time value. In this test project I use hours. After the set amount of time, the cached value is evicted from the cache.

When queries are made using a direct connection, those results are not cached.

## Running Locally

### Dependencies

* [.Net 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
* [A Cosmos DB account with a dedicated gateway](https://learn.microsoft.com/en-us/azure/cosmos-db/dedicated-gateway)

  * Create a database
  * Create a container with the partition key set to "/customKey".

* [Visual Studio Code](https://code.visualstudio.com/Download)

  * Recommended VS Code Extensions

* [Postman](https://www.postman.com/downloads/)

### Local Settings

You will need a file called `local.settings.json` in the same folder as the `Startup.cs` file. Your `local.settings.json` file should look like the example below. I have also included an [`example.settings.json` file](./src/example.settings.json) that you can copy, rename, and replace with your values.

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

### How to Run

Once your save your `local.settings.json` file with the necessary values, you can run the project from the command line with:

> func start

**Note:** This command only works if your terminal is in the directory that holds the `csproj` file.

You can also run using the VS Code debugger.

**Note:** If you get the error `Failed to verify "AzureWebJobsStorage" connection specified in "local.settings.json". Is the local emulator installed and running?`, use the Command Palette (`View > Command Palette`) and select `Azurite: Start`.

## Swagger

This project includes Swagger definitions. When you run the project locally, you will see a list of endpoints in the terminal, including Swagger endpoints. The `/swagger/ui` endpoint exposes an interface to call the the available Emission Factor Library Service functions. The `Try It Out` feature lets you send requests that hit your locally-running project. You can retrieve valid `refId` values from documents in your Cosmos DB container to send in the bodies of your requests.

The Swagger annotations for each function explain a function's purpose as well as the structure of the request, what status codes are returned, and the structure of the response.

## Postman Collection

Open Postman and import the Postman collection from this project. This collection contains requests that walk through the investigations for the purpose of this project. See the [PostmanCollectionOverview](./docs/PosmanCollectionOverview.md) document for general exploration of Cosmos with Integrated cache and [CacheBehaviorInvestigation](./docs/CacheBehaviorInvestigation.md) for understanding what happens if a query is sent via the dedicated gateway before the items exist in the database.
