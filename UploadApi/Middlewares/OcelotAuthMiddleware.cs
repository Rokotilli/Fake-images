namespace UploadApi.Middlewares
{
    public class OcelotAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public OcelotAuthMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            var ocelotCaller = context.Request.Headers["X-Ocelot-Key"].FirstOrDefault() ?? "";

            if (ocelotCaller == _configuration["OcelotHeaderKey"])
            {
                await _next(context);
            }
            else
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Access Denied");
            }
        }
    }
}
