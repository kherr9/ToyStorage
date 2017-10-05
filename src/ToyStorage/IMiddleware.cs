using System.Threading.Tasks;

namespace ToyStorage
{
    public delegate Task RequestDelegate(RequestContext context);

    public interface IMiddleware
    {
        Task Invoke(RequestContext context, RequestDelegate next);
    }
}
