using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TravelBooking.Api.Swagger;

/// <summary>
/// OpenAPI dokumanina Servers listesi ekler. "Failed to fetch" / localhost sorunlarinda
/// Swagger UI'dan "HTTPS (127.0.0.1)" secilebilir.
/// </summary>
public sealed class ServersDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Servers ??= new List<OpenApiServer>();
        swaggerDoc.Servers.Clear();

        swaggerDoc.Servers.Add(new OpenApiServer
            { Url = "https://localhost:7283", Description = "HTTPS (localhost)" });
        swaggerDoc.Servers.Add(new OpenApiServer
            { Url = "https://127.0.0.1:7283", Description = "HTTPS (127.0.0.1)" });
        swaggerDoc.Servers.Add(new OpenApiServer
            { Url = "http://localhost:5273", Description = "HTTP (localhost)" });
    }
}
