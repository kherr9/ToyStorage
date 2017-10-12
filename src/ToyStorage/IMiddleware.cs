using System.Threading.Tasks;

namespace ToyStorage
{
    public delegate Task RequestDelegate();

    public interface IMiddleware
    {
        Task Invoke(RequestContext context, RequestDelegate next);
    }
}
