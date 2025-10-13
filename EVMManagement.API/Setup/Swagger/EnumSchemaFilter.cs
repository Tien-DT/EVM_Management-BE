using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Serialization;

namespace EVMManagement.API.Setup.Swagger
{
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var type = context.Type;
            if (!type.IsEnum && !(Nullable.GetUnderlyingType(type)?.IsEnum ?? false)) return;

            var enumType = Nullable.GetUnderlyingType(type) ?? type;
            var names = Enum.GetNames(enumType);

            schema.Enum = new List<Microsoft.OpenApi.Any.IOpenApiAny>();
            foreach (var name in names)
            {
                schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(name));
            }
            schema.Type = "string";
            schema.Format = null;
        }
    }
}
