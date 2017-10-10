using System;

namespace ToyStorage
{
    public static class JsonFormaterMiddlewareExtensions
    {
        public static void UseJsonFormatter(this Middleware middleware, Action<JsonOptions> setupAction = null)
        {
            var jsonOptions = new JsonOptions();

            setupAction?.Invoke(jsonOptions);

            middleware.Use(new JsonFormaterMiddleware(jsonOptions.SerializerSettings));
        }
    }
}