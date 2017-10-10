namespace ToyStorage
{
    public static class JsonFormaterMiddlewareExtensions
    {
        public static void UseJson(this Middleware middleware)
        {
            middleware.Use<JsonFormaterMiddleware>();
        }
    }
}