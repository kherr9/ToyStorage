using System.Threading.Tasks;

namespace ToyStorage
{
    public interface IMiddlewarePipeline
    {
        Task Run(RequestContext requestContext);
    }
}