using System.Threading.Tasks;

namespace ToyStorage
{
    public delegate Task RequestDelegate();

    public interface IMiddlewareComponent
    {
        Task Invoke(RequestContext context, RequestDelegate next);
    }
}