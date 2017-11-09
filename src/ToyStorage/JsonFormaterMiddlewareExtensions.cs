using System;

namespace ToyStorage
{
    public static class JsonFormaterMiddlewareExtensions
    {
        public static IMiddlewarePipelineBuilder UseJsonFormatter(this IMiddlewarePipelineBuilder pipelineBuilder, Action<JsonOptions> setupAction = null)
        {
            var jsonOptions = new JsonOptions();

            setupAction?.Invoke(jsonOptions);

            pipelineBuilder.Use(new JsonFormaterMiddleware(jsonOptions.SerializerSettings));

            return pipelineBuilder;
        }
    }
}