# Toy Storage

Toy Storage is a lightweight and extensible data access framework for Azure Blob Storage.

[![Build Status](https://ci.appveyor.com/api/projects/status/github/kherr9/toystorage)](https://ci.appveyor.com/project/kherr9/toystorage)

## Get Packages

Toy Storage is still in early development and has not been published to NuGet, but if you're feeling adventurous, [continous integration builds are on MyGet](https://www.myget.org/feed/kherr9/package/nuget/ToyStorage).

## Get Started

Super-duper quick start:

Create a `DocumentCollection` with `Middleware` and `CloudBlobContainer`.

```C#
// create middleware
var middleware = new Middleware();

// add components to middleware
middleware.Add<JsonFormaterMiddleware>();
middleware.Add<BlobStorageMiddleware>();

// create container
var container = CloudStorageAccount.Parse("UseDevelopmentStorage=true")
                    .CreateCloudBlobClient()
                    .GetContainerReference("entities");

await container.CreateIfNotExistsAsync();

// create client
var documentCollection = new DocumentCollection(container, middleware);
```

The `Middleware` components have access to the request, the response, and the next middleware component in the request-response cycle. This model is used in popluar frameworks, such as [Express](http://expressjs.com/en/guide/using-middleware.html) and [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware?tabs=aspnetcore2x).

The `CloudBlobContainer` provides a grouping of a set of [blobs](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-dotnet-how-to-use-blobs). This is part of the [WindowsAzure.Storage library](https://www.nuget.org/packages/WindowsAzure.Storage/).

```C#

```