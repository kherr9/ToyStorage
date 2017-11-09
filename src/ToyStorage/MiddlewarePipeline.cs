using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ToyStorage
{
    public delegate Task MiddlewareCallback(RequestContext requestContext, RequestDelegate requestDelegate);

    public class MiddlewarePipeline : IMiddlewarePipeline
    {
        private readonly MiddlewareCallback[] _pipeline;

        public MiddlewarePipeline() : this(Enumerable.Empty<MiddlewareCallback>())
        {
        }

        public MiddlewarePipeline(IEnumerable<MiddlewareCallback> pipeline)
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

            _pipeline = pipeline.ToArray();
        }

        public MiddlewarePipeline Use<T>() where T : IMiddlewareComponent, new()
        {
            return Use(new T());
        }

        public MiddlewarePipeline Use(IMiddlewareComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return Use(component.Invoke);
        }

        public MiddlewarePipeline Use(MiddlewareCallback callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            return new MiddlewarePipeline(_pipeline.Concat(new[] { callback }));
        }

        public Task Run(RequestContext requestContext)
        {
            return Compose(_pipeline)(requestContext);
        }

        private static Func<RequestContext, Task> Compose(MiddlewareCallback[] pipeline)
        {
            // ReSharper disable once RedundantAssignment
            int exectionIndex = 0;

            return context =>
            {
                Task Next()
                {
                    exectionIndex++;
                    if (exectionIndex >= pipeline.Length)
                    {
                        // last middleware function calls next() when no next middleware function exists,
                        // that nonexistent function will return completed Task;
                        return TaskHelper.CompletedTask;
                    }

                    return pipeline[exectionIndex].Invoke(context, Next);
                }

                if (pipeline.Any())
                {
                    // first call
                    return pipeline[0].Invoke(context, Next);
                }

                // when souce is empty
                return TaskHelper.CompletedTask;
            };
        }
    }
}