# Toy Storage

Toy Storage is a lightweight and extensible data access framework for Azure Blob Storage.

[![Build Status](https://ci.appveyor.com/api/projects/status/github/kherr9/toystorage?svg=true)](https://ci.appveyor.com/project/kherr9/toystorage)
[![NuGet](https://img.shields.io/nuget/v/ToyStorage.svg)](https://www.nuget.org/packages/ToyStorage/)
[![MyGet (dev)](https://img.shields.io/myget/kherr9/vpre/ToyStorage.svg)](https://myget.org/feed/kherr9/package/nuget/ToyStorage)

## Get Packages

Toy Storage is still in early development and has not been published to NuGet, but if you're feeling adventurous, [continuous integration builds are on MyGet](https://www.myget.org/feed/kherr9/package/nuget/ToyStorage).

## Get Started

Super-duper quick start:

Create a `DocumentCollection` with `MiddlewarePipeline` and `CloudBlobContainer`.

```C#
// create middleware pipeline and configure middleware components
var pipeline = new MiddlewarePipeline()
                    .Use<JsonFormaterMiddleware>()
                    .Use<BlobStorageMiddleware>();

// create container
var container = CloudStorageAccount.Parse("UseDevelopmentStorage=true")
                    .CreateCloudBlobClient()
                    .GetContainerReference("entities");

// ensure container
await container.CreateIfNotExistsAsync();

// create client
var documentCollection = new DocumentCollection(container, pipeline);
```

The `Middleware` components have access to the request, the response, and the next middleware component in the request-response cycle. This pattern is used in frameworks, such as [Express](http://expressjs.com/en/guide/using-middleware.html) and [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware?tabs=aspnetcore2x).

The `CloudBlobContainer` provides a grouping of a set of [blobs](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-dotnet-how-to-use-blobs). This is part of the [WindowsAzure.Storage library](https://www.nuget.org/packages/WindowsAzure.Storage/).

Now that we have our `DocumentContainer`, let's save an entity.

```C#
var entity = new MyClass()
{
    Name = "Mario",
    Profession = "Plumber",
    FavoriteColor = "Red"
};

var id = "mario"

await documentCollection.PutAsync(entity, id);
```

The entity has been serialized to `JSON`, then written to the blob `mario` inside the container `entities`. Like `entities\mario`;

Now read our entity

```C#
var entity = await documentCollection.GetAsync<MyClass>("mario");

Console.WriteLine($"I have {entity.Name}");
// "I have Mario"
```

The blob has been read from `entities\mario`, then deserialized as type `MyClass`.

Now update our entity

```C#
enity.Profession = "Adventurer";

await documentCollection.PutAsync(entity, "mario");
```

The blob `entities\mario` is overwritten with our updated entity.

Now delete our entity

```C#
await documentCollection.DeleteAsync("mario");
```

The blob `entities\mario` as been deleted.

## Middleware

Middleware components can perform the following tasks:
* Execute any code.
* Make changes to the request and the response objects.
* End the request-response cycle.
* Call the next middleware component in the stack.

The minimal requirements for middleware to accomplish:
* Format request object to a binary representation (serialize) and format binary response to object (deserialize).
* Handle Read/Write/Delete commands to blob storage.

You can add other middleware components to do just about anything required for your application, like validation, compression, optimistic concurrency, caching, logging, security, etc.

#### Ordering

The order that middleware components are added in `Use` method defines the order in which they are invoked on requests, and the reverse order for the response. This ordering is critical for security, performance, and functionality.

#### Built-in middleware

Toy Storage comes with some basic middleware components that cover some common use cases.
* **BlobStorageMiddleware** - Handles the actual Read/Write/Delete commands to blobs.
* **GZipMiddleware** - Compress requests and decompress responses.
* **IfMatchConditionOnChangeMiddleware** - Add unobtrusive optimistic concurrency to data changing requests using the http precondition `If-Match` header.
* **JsonFormaterMiddleware** - Formats request to JSON and response from JSON.

#### Writing middleware

You can easily write your own middleware components.

Write a lambda

```C#
middleware.Use(async (ctx, next) =>
{
    Console.WriteLine($"Starting {ctx.RequestMethod} to {ctx.CloudBlockBlob.Name}");

    await next();

    Console.WriteLine("Completed");
});
```

Implement `IMiddlewareComponent`

```C#
public class LoggerMiddleware : IMiddlewareComponent
{
    public async Task Invoke(RequestContext context, RequestDelegate next)
    {
        Console.WriteLine($"Starting {context.RequestMethod} to {context.CloudBlockBlob.Name}");

        await next();

        Console.WriteLine("Completed");
    }
}

middleware.Use<LoggerMiddleware>();

```
