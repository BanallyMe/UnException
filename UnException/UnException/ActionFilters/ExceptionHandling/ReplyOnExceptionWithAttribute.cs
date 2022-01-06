using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace BanallyMe.UnException.ActionFilters.ExceptionHandling
{
    /// <summary>
    /// Defines a simple HTTP-reply that is sent to the client if the decorated action threw
    /// an exception of the specified type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ReplyOnExceptionWithAttribute : Attribute, IFilterMetadata
    {
        /// <summary>
        /// Type of exception that will be automatically replied on.
        /// </summary>
        public Type ExceptionType { get; }

        /// <summary>
        /// HTTP-Statuscode of the automatically generated reply.
        /// </summary>
        public int HttpStatusCode { get; }

        /// <summary>
        /// Message of the automatically generated reply. If this is set to null, the message of the
        /// occured exception will be used (default behaviour).
        /// </summary>
        public string? ReplyMessage { get; set; } = null;

        /// <summary>
        /// A description of when and why this Exception will occur. This can be used by
        /// API descriptors to document this exception response.
        /// </summary>
        public string? ErrorDescription { get; set; } = null;

        /// <summary>
        /// If set to true, the exception will be passed to the ASP.NET Core logging API, otherwise
        /// the exception will simply be discarded.
        /// </summary>
        public bool LogException { get; set; } = true;


        /// <summary>
        /// Defines a simple HTTP-reply for the decorated action in case of the specified exception.
        /// </summary>
        /// <param name="exceptionType">Type of exception that will be automatically replied on.</param>
        /// <param name="httpStatusCode">HTTP-Statuscode of the automatically generated reply.</param>
        public ReplyOnExceptionWithAttribute(Type exceptionType, int httpStatusCode)
        {
            if (exceptionType is null) throw new ArgumentNullException(nameof(exceptionType));
            if (!TypeIsException(exceptionType)) throw new ArgumentException("Exception type must derive of System.Exception", nameof(exceptionType));
            if (!StatusCodeIsInValidRange(httpStatusCode)) throw new ArgumentOutOfRangeException(nameof(httpStatusCode), httpStatusCode, "Statuscode must be a value between 100 and 599.");

            ExceptionType = exceptionType;
            HttpStatusCode = httpStatusCode;
        }

        private bool TypeIsException(Type typeCandidate)
            => typeCandidate == typeof(Exception) || typeCandidate.IsSubclassOf(typeof(Exception));

        private bool StatusCodeIsInValidRange(int statusCode)
            => statusCode >= 100 && statusCode < 600;
    }
}
