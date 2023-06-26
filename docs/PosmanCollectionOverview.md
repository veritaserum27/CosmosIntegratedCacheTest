# Overview

This document describes how to use Postman to with the Azure Cosmos DB with Integrated Cache Sample project.

## Initial Project Setup

Please complete the ["Getting Started"](../README.md#getting-started) tasks before proceeding.

## Postman Setup

* Open Postman and import the [Postman collection](./attachments/AzureCosmosDBIntegratedCacheDemo.postman_collection.json) in this repo.

The Postman Collection has a set of variables, which you can view if you click on the collection name `AzureCosmosDBIntegratedCacheDemo` and then click on the `Variables` tab. Verify that the url of the locally-running instance of your function project matches the value stored for `baseUrl`. If your url is different, enter the correct url value under `CURRENT VALUE` for `baseUrl`. Be sure to save the collection by clicking `Save` (the disk icon to the right of the collection name).

You can change the number of `refId` values by supplying a new `CURRENT VALUE` for `refIdsCount`. The default count is `5`.

When you expand the collection, you will see two folders (`QueryCacheRequests` and `ItemCacheRequests`), each of which includes a list of requests.

## Executing the Requests in the QueryCacheRequests Folder

Make sure your functions project is running.

Please read the explanation for each request before executing.

### Create Documents for RefIds Associated with Cached Empty Response

* **Name:** `Create TestItem documents with RefIds.`
* **FunctionExecuted**: CreateTestItemsByRefids
* **Description:** This request sends the `refIds` generated in the Pre-Request script and creates an item in Azure Cosmos DB for each `refId` in the payload.
* **Expected Behavior:** The function will send the request to Azure Cosmos DB via the direct connection and return with a `201` status code as well as a list of the new `TestItem` objects that were created and stored.
* **Pre-Request Script:** This is where the `refIds` are programmatically generated and saved.
* **Tests**:
  
  * Status Code is `201`.
  * Created expected number of `TestItem` objects.
  * Created `TestItem` objects have correct `RefId` values.

Execute this request. All tests should pass.

### Verify Items Exist in the Database

* **Name:** `List Test Items by RefIds using direct connection returns items.`
* **FunctionExecuted**: ListRefidsDirectConnection
* **Description:** This request sends the same payload of `refIds` as in previous requests to retrieve documents that match on `refId`. Requests are sent using a direct connection string, which means the database will be queried directly without checking the cache.
* **Expected Behavior:** A `TestItemQueryResult` object will be returned with a non-zero value for `requestUnits` and a list of `TestItem` objects associated with the `refId` values in the request payload.
* **Pre-Request Script:** n/a
* **Tests**:
  
  * Status Code is 200.
  * Request unit charge is greater than 0.
  * Response returned expected number of items.

Execute this request. All tests should pass.

### Cache Items using Dedicated Gateway

* **Name:** `List Test Items by RefIds using dedicated gateway.`
* **FunctionExecuted**: ListRefidsDedicatedGateway
* **Description:** This request sends the same payload of `refIds` as in previous requests to retrieve documents that match on `refId`. Requests are sent using the dedicated gateway, which means the cache will be checked for matches on this query.
* **Expected Behavior:** The `TestItem` objects that match the `refId` values in the payload will be returned on the `TestItemQueryResult` object with a `requestUnits` value of greater than `0` because this query has not been cached previously.
* **Pre-Request Script:** n/a
* **Tests**:
  
  * Status Code is 200.
  * Request Unit charge is greater than 0.
  * Response returned expected number of items.

Execute this request. All tests should pass.

### Retrieve Items from Query Cache

* **Name:** `List Test Items by RefIds using dedicated gateway after items are cached.`
* **FunctionExecuted**: ListRefidsDedicatedGateway
* **Description:** This request sends the same payload of `refIds` as in previous requests to retrieve documents that match on `refId`. Requests are sent using the dedicated gateway, which means the cache will be checked for matches on this query.
* **Expected Behavior:** The `TestItem` objects that match the `refId` values in the payload will be returned on the `TestItemQueryResult` object with a `requestUnits` value `0` because this query has been cached.
* **Pre-Request Script:** n/a
* **Tests**:
  
  * Status Code is 200.
  * Request Unit charge is 0.
  * Response returned expected number of items.

Execute this request. All tests should pass.

You can send this request multiple times in a row and get the same result of status `200`, `0` RUs incurred, and a return of the expected items.

## Executing Requests in the ItemCacheRequests Folder

Make sure your functions project is running.

Please read the explanation for each request before executing.

### Create Document for RefId Associated with Item Cache

* **Name:** `Create TestItem document via direct connection and save id, partitionKey.`
* **FunctionExecuted**: CreateTestItemsByRefids
* **Description:** This request sends the `refIdsForItemQueries` that are created in the pre-request script and creates items in Azure Cosmos DB for each `refId` in the payload.
* **Expected Behavior:** The function will send the request to Azure Cosmos DB via the direct connection and return with a `201` status code as well as a list of the new `TestItem` objects that were created and stored. For this request we only create a single new `TestItem`.
* **Pre-Request Script:** This is where the `refIdsForItemQueries` are programmatically generated and saved.
* **Tests**:
  
  * Status Code is `201`.
  * Created `TestItem` objects have correct values.

Execute this request. All tests should pass.

### Get Item from Database Using Point Read

* **Name:** `Get item from database using id and partition key.`
* **FunctionExecuted**: GetSingleRefIdDedicatedGateway
* **Description:** This request route includes the `id` and `paritionKey` (called `customKey` in our project) values from the item created in the `Create TestItem document via direct connection and save id, partitionKey.` request.
* **Expected Behavior:** A `TestItemQueryResult` object will be returned with a non-zero value for `requestUnits` and a single item in the `TestItems` list that corresponds to the `id` and `partitionKey` values sent in the request.
* **Pre-Request Script:** This is where the `id` and `partitionKey` values are retrieved after they were saved in the previous request.
* **Tests**:
  
  * Status Code is 200.
  * Request unit charge is greater than 0.
  * Response returned expected number of items.
  * Response item has expected values.

Execute this request. All tests should pass.

### Get Item from Item Cache Using Point Read

* **Name:** `Get item from cache using id and partition key.`
* **FunctionExecuted**: GetSingleRefIdDedicatedGateway
* **Description:** This request route includes the `id` and `paritionKey` (called `customKey` in our project) values from the item created in the `Create TestItem document via direct connection and save id, partitionKey.` request.
* **Expected Behavior:** A `TestItemQueryResult` object will be returned with a 0 value for `requestUnits` and a single item in the `TestItems` list that corresponds to the `id` and `partitionKey` values sent in the request.
* **Pre-Request Script:** This is where the `id` and `partitionKey` values are retrieved after they were saved in a previous request.
* **Tests**:
  
  * Status Code is 200.
  * Request unit charge is 0.
  * Response returned expected number of items.
  * Response item has expected values.

Execute this request. All tests should pass.

You can send this request multiple times in a row and get the same result of status `200`, `0` RUs incurred, and a return of the expected item.
