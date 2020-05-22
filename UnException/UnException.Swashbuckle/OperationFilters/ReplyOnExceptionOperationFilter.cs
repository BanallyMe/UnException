using BanallyMe.UnException.ActionFilters.ExceptionHandling;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace BanallyMe.UnException.Swashbuckle.OperationFilters
{
    public class ReplyOnExceptionOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation is null || context is null) return;

            AddAutomaticRepliesToOperation(operation, context);
        }

        private static void AddAutomaticRepliesToOperation(OpenApiOperation operation, OperationFilterContext context)
        {
            var replyAttributes = GetReplyAttributesFromContext(context);
            foreach (var replyAttribute in replyAttributes)
            {
                AddAttributeSpecificationsToOperationInContext(replyAttribute, operation, context);
            }
        }

        private static IEnumerable<ReplyOnExceptionWithAttribute> GetReplyAttributesFromContext(OperationFilterContext context)
            => context.MethodInfo.GetCustomAttributes<ReplyOnExceptionWithAttribute>(true);

        private static void AddAttributeSpecificationsToOperationInContext(ReplyOnExceptionWithAttribute attribute, OpenApiOperation operation, OperationFilterContext context)
        {
            var apiResponse = OperationResponse.FromReplyAttributeForContext(attribute, context);
            if (OperationHasStatuscodeResponse(operation, apiResponse.Statuscode))
            {
                MergeResponseForOperation(apiResponse, operation);
            }
            else
            {
                AddResponseToOperation(apiResponse, operation);
            }
        }

        private static bool OperationHasStatuscodeResponse(OpenApiOperation operation, string httpStatuscode)
            => operation.Responses.ContainsKey(httpStatuscode);

        private static void AddResponseToOperation(OperationResponse response, OpenApiOperation operation)
        {
            operation.Responses.Add(response.Statuscode, response.ToOpenApiResponse());
        }

        private static void MergeResponseForOperation(OperationResponse response, OpenApiOperation operation)
        {
            operation.Responses[response.Statuscode].Description += $" / { response.Description }";
        }


        private class OperationResponse
        {
            private OperationResponse(string statuscode, string description, Dictionary<string, OpenApiMediaType> content)
            {
                Statuscode = statuscode;
                Description = description;
                Content = content;
            }

            public string Statuscode { get; }
            public string Description { get; }
            public Dictionary<string, OpenApiMediaType> Content { get; }

            public OpenApiResponse ToOpenApiResponse()
                => new OpenApiResponse { Description = Description, Content = Content };

            public static OperationResponse FromReplyAttributeForContext(ReplyOnExceptionWithAttribute replyAttribute, OperationFilterContext context)
                => new OperationResponse(
                    replyAttribute.HttpStatusCode.ToString(CultureInfo.InvariantCulture),
                    replyAttribute.ReplyMessage ?? "No description provided",
                    new Dictionary<string, OpenApiMediaType> { ["text/plain"] = GetStringMediaTypeForContext(context) }
                );

            private static OpenApiMediaType GetStringMediaTypeForContext(OperationFilterContext context)
            {
                var stringSchema = context.SchemaGenerator.GenerateSchema(typeof(string), context.SchemaRepository);

                return new OpenApiMediaType { Schema = stringSchema };
            }
        }
    }
}
