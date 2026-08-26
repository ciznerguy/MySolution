using MySql.Data.MySqlClient;

namespace DAL
{
    public class BaseDB
    {
        // מחרוזת ההתקשרות למסד הנתונים MySQL מוזרקת מבחוץ (ראו MyProject.API/Program.cs) ואינה נשמרת בקוד
        protected readonly string connectionString;

        protected BaseDB(string connectionString)
        {
            this.connectionString = connectionString;
        }
    }
}