using DAL;
using Model;

namespace MyProject.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // הוספת שירותי בקרים ו-OpenAPI
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            // 1. הזרקת מחלקת ה-DAL למנגנון התלויות במערכת
            builder.Services.AddScoped<PurchaseDB>();

            // 2. הגדרת מדיניות CORS המאפשרת לקוח Blazor/View לקרוא ל-API
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
            });

            var app = builder.Build();

            // הגדרת צינור עיבוד הבקשות (Pipeline)
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            // הפעלת מדיניות ה-CORS
            app.UseCors("AllowAll");

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}