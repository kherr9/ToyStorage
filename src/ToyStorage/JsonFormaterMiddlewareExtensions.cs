using System;

namespace ToyStorage
{
    public static class JsonFormaterMiddlewareExtensions
    {
        public static MiddlewarePipeline UseJsonFormatter(this MiddlewarePipeline pipeline, Action<JsonOptions> setupAction = null)
        {
            var jsonOptions = new JsonOptions();

            setupAction?.Invoke(jsonOptions);

            return pipeline.Use(new JsonFormaterMiddleware(jsonOptions.SerializerSettings));
        }
    }
}