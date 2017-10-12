namespace ToyStorage
{
    public static class RequestContextExtensions
    {
        public static bool IsRead(this RequestContext context)
        {
            return context.RequestMethod == RequestMethods.Get;
        }

        public static bool IsWrite(this RequestContext context)
        {
            return context.RequestMethod == RequestMethods.Put;
        }

        public static bool IsDelete(this RequestContext context)
        {
            return context.RequestMethod == RequestMethods.Delete;
        }
    }
}