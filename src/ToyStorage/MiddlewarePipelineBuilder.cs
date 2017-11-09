using System;
using System.Collections.Generic;

namespace ToyStorage
{
    public class MiddlewarePipelineBuilder : IMiddlewarePipelineBuilder
    {
        private readonly List<MiddlewareCallback> _pipeline = new List<MiddlewareCallback>();

        public IMiddlewarePipelineBuilder Use<T>() where T : IMiddlewareComponent, new()
        {
            return Use(new T());
        }

        public IMiddlewarePipelineBuilder Use(IMiddlewareComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            return Use(component.Invoke);
        }

        public IMiddlewarePipelineBuilder Use(MiddlewareCallback callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            _pipeline.Add(callback);

            return this;
        }

        public IMiddlewarePipeline Build()
        {
            return new MiddlewarePipeline(_pipeline);
        }
    }
}