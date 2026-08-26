using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace MyProject.API
{
    // רץ רק בהרצה הראשונה לאחר שכפול מגיט, כאשר אין עדיין מחרוזת חיבור שמורה:
    // מבקש מהתלמיד את פרטי ההתחברות ל-MySQL המקומי שלו ושומר אותם ב-User Secrets לשימוש בהרצות הבאות
    public static class FirstRunSetup
    {
        public static string PromptAndSaveLocalConnectionString()
        {
            // באנגלית בכוונה: קונסולת Windows לא מציגה עברית (RTL) כראוי ומציגה טקסט הפוך
            Console.WriteLine();
            Console.WriteLine("=== First run: no database connection string found ===");
            Console.WriteLine("Enter your local MySQL connection details (press Enter to accept the default in brackets):");

            string server = Ask("Server", "localhost");
            string port = Ask("Port", "3306");
            string database = Ask("Database name", "online_store");
            string userId = Ask("Username", "root");
            string password = Ask("Password", "");

            string connectionString =
                $"server={server};port={port};user id={userId};password={password};database={database}";

            SaveToUserSecrets(connectionString);

            Console.WriteLine("Saved to User Secrets (outside the project folder) - you won't be asked again next run.");
            Console.WriteLine();

            return connectionString;
        }

        private static string Ask(string label, string defaultValue)
        {
            string suffix = string.IsNullOrEmpty(defaultValue) ? "" : $" [{defaultValue}]";
            Console.Write($"{label}{suffix}: ");
            string? input = Console.ReadLine();
            return string.IsNullOrWhiteSpace(input) ? defaultValue : input.Trim();
        }

        private static void SaveToUserSecrets(string connectionString)
        {
            string? secretsId = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<UserSecretsIdAttribute>()?.UserSecretsId;

            if (secretsId is null)
            {
                return;
            }

            string secretsPath = GetSecretsFilePath(secretsId);

            Dictionary<string, string> secrets = File.Exists(secretsPath)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(secretsPath)) ?? new()
                : new();

            secrets["ConnectionStrings:Local"] = connectionString;

            Directory.CreateDirectory(Path.GetDirectoryName(secretsPath)!);
            File.WriteAllText(secretsPath, JsonSerializer.Serialize(secrets, new JsonSerializerOptions { WriteIndented = true }));
        }

        // נתיב קובץ ה-User Secrets - זהה לזה שבו משתמש dotnet user-secrets
        private static string GetSecretsFilePath(string userSecretsId)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string root = OperatingSystem.IsWindows()
                ? Path.Combine(appData, "Microsoft", "UserSecrets")
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".microsoft", "usersecrets");

            return Path.Combine(root, userSecretsId, "secrets.json");
        }
    }
}
