using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApplication2.Models;

namespace WebApplication2.Swagger;

public class SchemaExamples : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(Player))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiInteger(1),
                ["name"] = new OpenApiString("Juan")
            };
        }

        if (context.Type == typeof(PlayerDto))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiInteger(2),
                ["name"] = new OpenApiString("María")
            };
        }
    }
}
