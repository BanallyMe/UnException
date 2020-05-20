using BanallyMe.UnException.ActionFilters.ExceptionHandling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BanallyMe.UnException.DependencyInjection
{
    /// <summary>
    /// Provides extension methods to add the UnException library to the .NET Core
    /// dependency injection container.
    /// </summary>
    public static class UnExceptionLoader
    {
        /// <summary>
        /// Adds the UnException action filters to the ASP.NET Core MVC framework.
        /// </summary>
        /// <param name="services">Collection of services for building a dependency injection container.</param>
        public static void AddUnException(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add<ReplyOnExceptionWithFilter>();
            });
        }
    }
}
