using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ToyStorage
{
    public class Middleware
    {
        private readonly List<IMiddleware> _pipeline;

        public Middleware()
        {
            _pipeline = new List<IMiddleware>();
        }

        public void Use(Func<RequestContext, RequestDelegate, Task> func)
        {
            Use(new AnonymousMiddleware(func));
        }

        public void Use(IMiddleware middleware)
        {
            if (middleware == null) throw new ArgumentNullException(nameof(middleware));

            _pipeline.Add(middleware);
        }

        public Task Run(RequestContext requestContext)
        {
            return Compose(_pipeline)(requestContext);
        }

        private static Func<RequestContext, Task> Compose(IEnumerable<IMiddleware> source)
        {
            var list = source.ToArray();
            // ReSharper disable once RedundantAssignment
            int exectionIndex = 0;

            return context =>
            {
                Task Next(RequestContext otherContext)
                {
                    exectionIndex++;
                    if (exectionIndex >= list.Length)
                    {
                        // else the last middleware function calls next() when no next middleware function exists,
                        // that nonexistent function will return null;
                        return Task.CompletedTask;
                    }

                    return list[exectionIndex].Invoke(context, Next);
                }

                if (list.Any())
                {
                    // first call
                    return list[0].Invoke(context, Next);
                }

                // when souce is empty
                return Task.CompletedTask;
            };
        }

        private sealed class AnonymousMiddleware : IMiddleware
        {
            private readonly Func<RequestContext, RequestDelegate, Task> _func;

            public AnonymousMiddleware(Func<RequestContext, RequestDelegate, Task> func)
            {
                _func = func ?? throw new ArgumentNullException(nameof(func));
            }

            public Task Invoke(RequestContext context, RequestDelegate next)
            {
                return _func(context, next);
            }
        }
    }
}