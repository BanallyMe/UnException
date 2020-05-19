using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;

namespace BanallyMe.UnException.ActionFilters.ExceptionHandling
{
    /// <summary>
    /// Implementation of an action filter that can handle exceptions thrown by actions
    /// decorated with <see cref="ReplyOnExceptionWithAttribute"/>.
    /// </summary>
    public class ReplyOnExceptionWithFilter : IActionFilter
    {
        private readonly ILogger<ReplyOnExceptionWithAttribute> loggerApi;

        public ReplyOnExceptionWithFilter(ILogger<ReplyOnExceptionWithAttribute> loggerApi)
        {
            this.loggerApi = loggerApi;
        }

        /// <summary>
        /// Checks if the action threw an exception and has attributes of type <see cref="ReplyOnExceptionWithAttribute"/>.
        /// If appropriate the filter handles the exception and returns an automatically generated reply.
        /// </summary>
        /// <param name="context">Context in which the action was executed.</param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // TODO Implementation :-)
            throw new NotImplementedException();
        }

        /// <summary>
        /// No Action will be performed before the action.
        /// </summary>
        /// <param name="context">Context in which the action is executed.</param>
        public void OnActionExecuting(ActionExecutingContext context)
        { }
    }
}
