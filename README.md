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

The `Middleware` components have access to the request, the response, and the next middleware component in the request-response cycle. This pattern is used in frameworks, such as [Express](http://expressjs.com/en/guide/using-middleware.html) and [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware?tabs=aspnetcore2x).

The `CloudBlobContainer` provides a grouping of a set of [blobs](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-dotnet-how-to-use-blobs). This is part of the [WindowsAzure.Storage library](https://www.nuget.org/packages/WindowsAzure.Storage/).

Now that we have our `DocumentContainer`, let's store an entity.

```C#
var entity = new MyClass()
{
    Name = "Mario",
    Profession = "Plumber",
    FavoriteColor = "Red"
};

var id = "mario"

await documentCollection.StoreAsync(entity, id);
```

The entity has been serialized to `JSON`, then written to the blob `mario` inside the container `entities`. Like `entities\mario`;

Now lets read our entity

```C#
var entity = documentCollection.GetAsync<MyClass>("mario");

Console.WriteLine($"I have {entity.Name}");
// "I have Mario"
```

The blob has been read from `entities\mario`, then deserialized as type `MyClass`.

Now lets update our entity

```C#
enity.Profession = "Adventurer";

await documentCollection.StoreAsync(entity, "mario");
```

The blob `entities\mario` is overwritten with our updated entity.

Now lets delete our entity

```C#
await documentCollection.DeleteAsync("mario");
```

Now we've deleted the blob `entities\mario`.
