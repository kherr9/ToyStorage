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
    }
}