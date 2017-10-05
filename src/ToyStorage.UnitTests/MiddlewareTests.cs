using System.Threading.Tasks;
using Xunit;

namespace ToyStorage.UnitTests
{
    public class MiddlewareTests
    {
        [Fact]
        public async Task TestRunWhenEmpty()
        {
            // Arrange
            var context = new RequestContext();
            var middleware = new Middleware();

            // Act
            await middleware.Run(context);

            // Assert - no errors were thrown
        }

        [Fact]
        public async Task TestRunWhenSingleComponent()
        {
            // Arrange
            var context = new RequestContext();
            var middleware = new Middleware();
            middleware.Use(async (ctx, next) =>
            {
                ctx.Entity = "this is a string";
                await next(ctx);
                ctx.EntityType = typeof(string);
            });

            // Act
            await middleware.Run(context);

            // Assert
            Assert.Equal("this is a string", context.Entity);
            Assert.Equal(typeof(string), context.EntityType);
        }

        [Fact]
        public async Task TestRunComponentsOrdering()
        {
            // Arrange
            var context = new RequestContext();
            var middleware = new Middleware();
            middleware.Use(async (ctx, next) =>
            {
                ctx.Entity = "this is a string";
                await next(ctx);
                ctx.EntityType = typeof(string); // this will override the previous value
            });

            middleware.Use(async (ctx, next) =>
            {
                ctx.Entity = 45; // this will override the previous value
                await next(ctx);
                ctx.EntityType = typeof(int);
            });

            // Act
            await middleware.Run(context);

            // Assert
            Assert.Equal(45, context.Entity);
            Assert.Equal(typeof(string), context.EntityType);
        }

        [Fact]
        public async Task TestRunExplicitlyEndPipeline()
        {
            // Arrange
            var context = new RequestContext();
            var middleware = new Middleware();
            middleware.Use((ctx, next) =>
            {
                ctx.Entity = "this is a string";
                ctx.EntityType = typeof(string);
                // don't call next to end pipeline
                return Task.CompletedTask;
            });

            middleware.Use(async (ctx, next) =>
            {
                // this method should NOT be invoked
                ctx.Entity = 45;
                await next(ctx);
                ctx.EntityType = typeof(int);
            });

            // Act
            await middleware.Run(context);

            // Assert
            Assert.Equal("this is a string", context.Entity);
            Assert.Equal(typeof(string), context.EntityType);
        }
    }
}
