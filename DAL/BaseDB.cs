using MySql.Data.MySqlClient;

namespace DAL
{
    public class BaseDB
    {
        // מחרוזת התקשרות למסד הנתונים MySQL
        protected string connectionString = "server=localhost;user id=root;password=myPassword;database=online_store";

    }
}