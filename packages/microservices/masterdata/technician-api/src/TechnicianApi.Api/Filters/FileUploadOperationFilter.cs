using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TechnicianApi.Api.Filters;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var allParams = context.MethodInfo.GetParameters();
        var fileParams = allParams
            .Where(p => p.ParameterType == typeof(IFormFile) ||
                       p.ParameterType == typeof(IEnumerable<IFormFile>))
            .ToList();

        if (!fileParams.Any()) return;

        // Clear parameters to avoid conflicts
        operation.Parameters?.Clear();

        // Get all parameters including non-file ones
        var properties = new Dictionary<string, OpenApiSchema>();
        var required = new HashSet<string>();

        foreach (var param in allParams)
        {
            var paramName = param.Name ?? "unknown";

            if (param.ParameterType == typeof(IFormFile) ||
                param.ParameterType == typeof(IEnumerable<IFormFile>))
            {
                properties[paramName] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                };
            }
            else if (param.ParameterType == typeof(string))
            {
                properties[paramName] = new OpenApiSchema
                {
                    Type = "string"
                };
            }

            required.Add(paramName);
        }

        operation.RequestBody = new OpenApiRequestBody
        {
            Required = true,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = properties,
                        Required = required
                    }
                }
            }
        };
    }
}
