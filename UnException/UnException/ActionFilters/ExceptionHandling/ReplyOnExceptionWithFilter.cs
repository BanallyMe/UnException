using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

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
            if (context != null && context.Exception != null)
            {
                var filterAttribute = GetReplyAttributeForExceptionTypeAndAction(context.Exception.GetType(), context.ActionDescriptor);
                if (filterAttribute != null)
                {
                    var resultMessage = filterAttribute.ReplyMessage ?? context.Exception.Message;
                    var statusCode = filterAttribute.HttpStatusCode;
                    if (filterAttribute.LogException)
                    {
                        loggerApi.LogError(context.Exception, resultMessage);
                    }
                    context.Result = new ObjectResult(resultMessage) { StatusCode = statusCode };
                    context.Exception = null;
                }
            }
        }

        /// <summary>
        /// No Action will be performed before the action.
        /// </summary>
        /// <param name="context">Context in which the action is executed.</param>
        public void OnActionExecuting(ActionExecutingContext context)
        { }

        private static ReplyOnExceptionWithAttribute? GetReplyAttributeForExceptionTypeAndAction(Type exceptionType, ActionDescriptor action)
        {
            var allFilters = GetReplyAttributesForAction(action);
            var filterForExplicitExceptionType = allFilters.FirstOrDefault(filter => filter.ExceptionType == exceptionType);

            return filterForExplicitExceptionType ?? allFilters.FirstOrDefault(filter => exceptionType.IsSubclassOf(filter.ExceptionType));
        }

        private static IEnumerable<ReplyOnExceptionWithAttribute> GetReplyAttributesForAction(ActionDescriptor action)
        {
            if (action.FilterDescriptors is null) return Array.Empty<ReplyOnExceptionWithAttribute>();

            return action.FilterDescriptors
                .OrderBy(filterDescriptor => filterDescriptor.Order)
                .Select(filterDescriptor => filterDescriptor.Filter)
                .OfType<ReplyOnExceptionWithAttribute>();
        }
    }
}
