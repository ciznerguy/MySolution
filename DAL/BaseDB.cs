using MySql.Data.MySqlClient;
namespace DAL
{
    // מחלקת בסיס לשכבת הגישה לנתונים, המכילה את מחרוזת החיבור המשותפת
    public class BaseDB
    {
        // מחרוזת ההתקשרות למסד הנתונים המוזרקת מבחוץ ואינה נשמרת בקוד הקבוע
        protected readonly string connectionString;

        // פעולה בונה המקבלת את מחרוזת ההתקשרות ושומרת אותה לשימוש המחלקות היורשות
        protected BaseDB(string connectionString)
        {
            this.connectionString = connectionString;
        }
    }
}