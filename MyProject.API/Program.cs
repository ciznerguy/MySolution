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

            // מחרוזות החיבור נשמרות ב-User Secrets (dotnet user-secrets) ולא בקוד/ב-git.
            // מחרוזת חיבור למסד נתונים מקומי (לפיתוח בלבד) - מוגדרת תחת המפתח ConnectionStrings:Local
            // מחרוזת חיבור פעילה - מסד הנתונים המרוחק (Aiven Cloud MySQL), מוגדרת תחת המפתח ConnectionStrings:Remote
            string? connectionString = builder.Configuration.GetConnectionString("Remote")
                ?? builder.Configuration.GetConnectionString("Local");

            // הרצה ראשונה לאחר שכפול מגיט: אין עדיין מחרוזת חיבור שמורה - נבקש מהתלמיד את פרטי ה-MySQL המקומי שלו
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = FirstRunSetup.PromptAndSaveLocalConnectionString();
            }

            // ודא שבסיס הנתונים, הטבלאות ונתוני הדוגמה קיימים - יוצר אותם אם חסרים
            DatabaseInitializer.EnsureDatabaseReady(connectionString);

            // 1. הזרקת מחלקות ה-DAL והעברת מחרוזת החיבור לבנאי
            builder.Services.AddScoped<PurchaseDB>(sp => new PurchaseDB(connectionString));
            builder.Services.AddScoped<PersonDB>(sp => new PersonDB(connectionString));

            // 2. הגדרת מדיניות CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseCors("AllowAll");
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}