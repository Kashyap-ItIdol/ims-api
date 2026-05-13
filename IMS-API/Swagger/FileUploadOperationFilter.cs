using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Http;
using IMS_Application.DTOs;

namespace IMS_API.Swagger
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileUploadMime = "multipart/form-data";
            if (context.ApiDescription.HttpMethod != "POST" && context.ApiDescription.HttpMethod != "PUT")
                return;

            // Check for UploadAttachmentRequest parameter
            var uploadRequestParameters = context.ApiDescription.ActionDescriptor.Parameters
                .Where(p => p.ParameterType == typeof(UploadAttachmentRequest))
                .ToList();

            if (uploadRequestParameters.Count < 1)
                return;

            // Remove the request parameter from operation parameters
            foreach (var requestParam in uploadRequestParameters)
            {
                var parameter = operation.Parameters.FirstOrDefault(p => p.Name == requestParam.Name);
                if (parameter != null)
                {
                    operation.Parameters.Remove(parameter);
                }
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    {
                        fileUploadMime,
                        new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Required = new HashSet<string> { "file" },
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["file"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary"
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
