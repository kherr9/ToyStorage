using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ToyStorage.Foo
{
    public delegate Task MiddlewareCallback(RequestContext requestContext, RequestDelegate requestDelegate);

    public delegate Task RequestDelegate();

    public interface IMiddlewareComponent
    {
        Task Invoke(RequestContext context, RequestDelegate next);
    }

    public interface IMiddlewarePipeline
    {
        Task Run(RequestContext requestContext);
    }

    public interface IMiddlewarePipelineBuilder
    {
        IMiddlewarePipelineBuilder Add<T>() where T : IMiddlewareComponent, new();

        IMiddlewarePipelineBuilder Add(MiddlewareCallback callback);

        IMiddlewarePipelineBuilder Add(IMiddlewareComponent component);

        IMiddlewarePipeline Build();
    }

    public class MiddlewarePipelineBuilder : IMiddlewarePipelineBuilder
    {
        private readonly List<IMiddlewareComponent> _pipeline = new List<IMiddlewareComponent>();

        public IMiddlewarePipelineBuilder Add<T>() where T : IMiddlewareComponent, new()
        {
            return Add(new T());
        }

        public IMiddlewarePipelineBuilder Add(MiddlewareCallback callback)
        {
            return Add(new AnonymousMiddlewareComponent(callback));
        }

        public IMiddlewarePipelineBuilder Add(IMiddlewareComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            _pipeline.Add(component);

            return this;
        }

        public IMiddlewarePipeline Build()
        {
            return new MiddlewarePipeline(_pipeline);
        }
    }

    public class MiddlewarePipeline : IMiddlewarePipeline
    {
        private readonly IMiddlewareComponent[] _pipeline;

        public MiddlewarePipeline(IEnumerable<IMiddlewareComponent> pipeline)
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));

            _pipeline = pipeline.ToArray();
        }

        public Task Run(RequestContext requestContext)
        {
            return Compose(_pipeline)(requestContext);
        }

        private static Func<RequestContext, Task> Compose(IMiddlewareComponent[] pipeline)
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

    internal sealed class AnonymousMiddlewareComponent : IMiddlewareComponent
    {
        private readonly MiddlewareCallback _callback;

        public AnonymousMiddlewareComponent(MiddlewareCallback callback)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        public Task Invoke(RequestContext context, RequestDelegate next)
        {
            return _callback(context, next);
        }
    }
}
