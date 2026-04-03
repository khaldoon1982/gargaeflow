namespace GarageFlow.Api.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _apiKey;

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
        _apiKey = Environment.GetEnvironmentVariable("GARAGEFLOW_API_KEY") ?? "";
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip auth for health endpoint
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-Api-Key", out var extractedApiKey) ||
            string.IsNullOrEmpty(_apiKey) ||
            extractedApiKey != _apiKey)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Ongeldige API-sleutel" });
            return;
        }

        await _next(context);
    }
}
