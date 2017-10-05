using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ToyStorage
{
    public class ValidationMiddleware : IMiddleware
    {
        public Task Invoke(RequestContext context, RequestDelegate next)
        {
            var entity = context.Entity;
            if (entity != null)
            {
                Validator.ValidateObject(entity, new ValidationContext(entity, null, null));
            }

            return next(context);
        }
    }
}
