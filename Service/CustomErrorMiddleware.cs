using hsinchugas_efcs_api.Model;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;


namespace hsinchugas_efcs_api.Service
{
    
    public class CustomErrorMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomErrorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;

            using var newBody = new MemoryStream();
            context.Response.Body = newBody;

            await _next(context);

            if (context.Response.StatusCode == 415 || context.Response.StatusCode >300)
            {
                newBody.SetLength(0); // 睲 ProblemDetails

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 200;

                var errorResponse = new
                {
                    DOCDATA = new
                    {
                        HEAD = new
                        {
                            ICCHK_CODE = "S998",
                            ICCHK_CODE_DESC = "ユ传戈Αぃ才セ竚ま┮﹚竡ぇ戈挡篶"
                        }
                    }
                };

                var json = JsonSerializer.Serialize(errorResponse);
                await newBody.WriteAsync(Encoding.UTF8.GetBytes(json));

                newBody.Seek(0, SeekOrigin.Begin); // 」」」 岿粇家Αタ絋
            }
            else
            {
                newBody.Seek(0, SeekOrigin.Begin); // 」」」 タ絋家Α﹚璶硂︽
            }

            await newBody.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }

    }



}
