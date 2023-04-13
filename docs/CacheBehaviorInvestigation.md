# Overview

This document describes how to use Postman to investigate cache behavior when a list of items is queried before one or more of those items exists in the Cosmos Db instance. When queries are made using a Dedicated Gateway Connection String, the result of that query is cached with a `MaxIntegratedCacheStaleness` of some time value. In this test project I use hours. After the set amount of time, the cached value is evicted from the cache.

When queries are made using a direct connection, those results are not cached.

Questions for investigation:

1. Does the `not found` result of the query get cached?
2. If the `not found` query is sent again with a `MaxIntegratedCacheStaleness` set to `0`, does that "reset" the value in the cache for that query?

Answers based on observations from this experiment:

1. Yes, queries that retrieve no documents from the database are cached.
2. Yes, after this "retry" request, the cache stores the non-empty response. Additional repeated queries hit the cache and receive back the expected items.

## Initial Project Setup

Please complete the ["Running Locally"](../README.md#running-locally) tasks before proceeding.

## Postman Setup

* Download and install [Postman](https://www.postman.com/downloads/).
* Open Postman and import the [Postman collection](./attachments/CosmosIntegratedCacheTest.postman_collection.json) from this project.

The Postman Collection has a set of variables, which you can view if you click on the collection name `CosmosIntegratedCacheTest` and then click on the `Variables` tab. Verify that the url of your locally-running instance of your function project matches the value stored for `baseUrl`. If your url is different, enter the correct url value under `CURRENT VALUE` for `baseUrl`. Be sure to save the collection by clicking `Save` (the disk icon to the right of the collection name).

You can change the number of `refId` values by supplying a new `CURRENT VALUE` for `refIdsCount`. The default count is `5`.

When you expand the collection, you will see two folders (`QueryCacheRequests` and `ItemCacheRequests`), each of which includes a list of requests. The next section explains each request in the `QueryCacheRequests` folder.

## Executing the Requests in the QueryCacheRequests Folder

Make sure your functions project is running.

Please read the explanation for each request before executing.

### Cache an Empty Response

* **Name:** `List Test Items by RefIds when they don't exist in database.`
* **FunctionExecuted**: ListRefidsDedicatedGateway
* **Description:** This request programmatically generates `refIdsCount` number of UUIDs and saves the resulting array in the Collection Variables as `refIds`. This allows us to use the same set of `refIds` in later requests. Those `refIds` are then sent as a payload in a `POST` request to use the dedicated gateway.
* **Expected Behavior:** The function will send the request to Cosmos DB via the dedicated gateway with our default `MaxIntegratedCacheStaleness`. Since no items with those `refId` values exist in the database, the `not found` empty response for this query will be cached.
* **Pre-Request Script:** This is where the `refIds` are programmatically generated and saved.
* **Tests**:
  
  * Status Code is `404`.

Execute this request. All tests should pass.

### Create Documents for RefIds Associated with Cached Empty Response

* **Name:** `Create TestItem documents with RefIds.`
* **FunctionExecuted**: CreateTestItemsByRefids
* **Description:** This request sends the `refIds` that were created in `List Test Items by RefIds when they don't exist in database.` request and creates items in Cosmos DB for each `refId` in the payload.
* **Expected Behavior:** The function will send the request to Cosmos DB via the direct connection and return with a `201` status code as well as a list of the new `TestItem` objects that were created and stored.
* **Pre-Request Script:** n/a
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

### Verify Cached Empty Response

* **Name:** `List Test Items by RefIds when empty reponse is cached.`
* **FunctionExecuted**: ListRefidsDedicatedGateway
* **Description:** This request sends the same payload of `refIds` as in previous requests to retrieve documents that match on `refId`. Requests are sent using the dedicated gateway, which means the cache will be checked for matches on this query.
* **Expected Behavior:** The response will show that no items were found *even though items with those `refId` values now exist*.
* **Pre-Request Script:** n/a
* **Tests**:
  
  * Status Code is `404`.
  * Request unit charge is 0.
  * Response returned no items.

Execute this request. All tests should pass.

### Attempt to Clear the "Empty Response" Query from the Cache

* **Name:** `List Test Items by RefIds with retry.`
* **FunctionExecuted**: ListRefidsDedicatedGatewayWithRetry
* **Description:** This request first hits the cache as usual with the settings-based `MaxIntegratedCacheStaleness` value. Upon receiving back no items, it sends another query via the dedicated gateway--this time with a `MaxIntegratedCacheStaleness` value of `0`.
* **Expected Behavior:** The response will include two `TestItemQueryResult` objects. The `TestItemQueryResult` object associated with the cached call will have 0 items and incur 0 RUs. The `TestItemQueryResult` associated with the "retry" call will have the expected number of `TestItem` objects and should incur an RU charge. The data is cached upon retrieval from the database.
* **Pre-Request Script:** n/a
* **Tests**:
  
  * Status Code is 200.
  * Two TestItemQueryResult objects in response.
  * TestItemQueryResult from cached query returned no items.
  * TestItemQueryResult from cached query incurred no RUs.
  * TestItemQueryResult from non-cached query returned expected number of items.
  * TestItemQueryResult from non-cached query incurred RUs.

Execute this request. All tests should pass.

### Verify Item Response is Cached with Data

* **Name:** `List Test Items by RefIds after empty response is "cleared" from cache.`
* **FunctionExecuted**: ListRefidsDedicatedGateway
* **Description:** This request sends the same payload of `refIds` as in previous requests to retrieve documents that match on `refId`. Requests are sent using the dedicated gateway, which means the cache will be checked for matches on this query.
* **Expected Behavior:** The `TestItem` objects that match the `refId` values in the payload will be returned on the `TestItemQueryResult` object with a `requestUnits` value of greater than `0` (because we expect to hit the database directly since we told it not to cache upon the previous request).
* **Pre-Request Script:** n/a
* **Tests**:
  
  * Status Code is 200.
  * TestItemQueryResult incurred no RUs.
  * Response returned expected number of items.

Execute this request. All tests pass except for `TestItemQueryResult incurred RUs`.

You can send this request multiple times in a row and get the same result of status `200`, `0` RUs incurred, and a return of the expected items.
