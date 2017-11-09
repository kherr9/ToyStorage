using System.Threading.Tasks;

namespace ToyStorage
{
    public delegate Task MiddlewareCallback(RequestContext requestContext, RequestDelegate requestDelegate);

    public interface IMiddlewarePipelineBuilder
    {
        IMiddlewarePipelineBuilder Use<T>() where T : IMiddlewareComponent, new();

        IMiddlewarePipelineBuilder Use(MiddlewareCallback callback);

        IMiddlewarePipelineBuilder Use(IMiddlewareComponent component);

        IMiddlewarePipeline Build();
    }
}