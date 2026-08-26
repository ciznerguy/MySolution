using MySql.Data.MySqlClient;

namespace DAL
{
    // מוודא בכל הרצה שבסיס הנתונים, הטבלאות ונתוני הדוגמה קיימים - יוצר אותם אם חסרים
    public static class DatabaseInitializer
    {
        public static void EnsureDatabaseReady(string connectionString)
        {
            var csBuilder = new MySqlConnectionStringBuilder(connectionString);
            string databaseName = csBuilder.Database;

            // שלב 1: יצירת בסיס הנתונים אם אינו קיים - חיבור לשרת ללא ציון Database
            csBuilder.Database = "";
            using (var serverConn = new MySqlConnection(csBuilder.ConnectionString))
            {
                serverConn.Open();
                ExecuteNonQuery(serverConn, $"CREATE DATABASE IF NOT EXISTS {databaseName};");
            }

            // שלב 2: יצירת הטבלאות והזנת נתוני דוגמה מול בסיס הנתונים עצמו
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            CreateTables(conn);
            SeedDataIfEmpty(conn);
        }

        private static void CreateTables(MySqlConnection conn)
        {
            // לא נעשה שימוש ב-"CREATE TABLE IF NOT EXISTS" מול טבלה עם CHECK ללא שם:
            // ל-MySQL יש תקלה ידועה שגורמת לו לנסות להוסיף את האילוץ מחדש גם כשהטבלה כבר קיימת
            // ("Duplicate check constraint name") - לכן בודקים קיום מפורשות לפני היצירה.
            CreateTableIfMissing(conn, "PERSON", @"
                CREATE TABLE PERSON (
                    person_id INT AUTO_INCREMENT PRIMARY KEY,
                    full_name VARCHAR(100) NOT NULL,
                    email VARCHAR(100) UNIQUE NOT NULL,
                    role_code INT CHECK (role_code BETWEEN 1 AND 3),
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );");

            CreateTableIfMissing(conn, "PRODUCTS", @"
                CREATE TABLE PRODUCTS (
                    product_id INT AUTO_INCREMENT PRIMARY KEY,
                    product_name VARCHAR(100) NOT NULL,
                    price DECIMAL(10, 2) NOT NULL,
                    stock_quantity INT DEFAULT 0
                );");

            CreateTableIfMissing(conn, "PURCHASES", @"
                CREATE TABLE PURCHASES (
                    purchase_id INT AUTO_INCREMENT PRIMARY KEY,
                    customer_id INT,
                    product_id INT,
                    purchase_date DATETIME DEFAULT CURRENT_TIMESTAMP,
                    quantity INT DEFAULT 1,
                    FOREIGN KEY (customer_id) REFERENCES PERSON(person_id),
                    FOREIGN KEY (product_id) REFERENCES PRODUCTS(product_id)
                );");
        }

        private static void CreateTableIfMissing(MySqlConnection conn, string tableName, string createTableSql)
        {
            using (var checkCmd = new MySqlCommand(
                "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = @tableName;",
                conn))
            {
                checkCmd.Parameters.AddWithValue("@tableName", tableName);
                long exists = Convert.ToInt64(checkCmd.ExecuteScalar());
                if (exists > 0)
                {
                    return;
                }
            }

            ExecuteNonQuery(conn, createTableSql);
        }

        // מזין נתוני דוגמה (4 משתמשים, 4 מוצרים, 3 הזמנות) רק אם טבלת PERSON ריקה
        private static void SeedDataIfEmpty(MySqlConnection conn)
        {
            using (var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM PERSON;", conn))
            {
                long existingCount = Convert.ToInt64(checkCmd.ExecuteScalar());
                if (existingCount > 0)
                {
                    return;
                }
            }

            ExecuteNonQuery(conn, @"
                INSERT INTO PERSON (full_name, email, role_code) VALUES
                    ('Dani Din', 'dani@gmail.com', 1),
                    ('Gadi Sales', 'gadi@store.com', 2),
                    ('Big Boss', 'admin@store.com', 3),
                    ('Noa Cohen', 'noa@store.com', 1);");

            ExecuteNonQuery(conn, @"
                INSERT INTO PRODUCTS (product_name, price, stock_quantity) VALUES
                    ('מקלדת אלחוטית', 89.90, 50),
                    ('עכבר אופטי', 39.90, 120),
                    ('מסך 24 אינץ', 649.00, 15),
                    ('אוזניות Bluetooth', 129.00, 40);");

            ExecuteNonQuery(conn, @"
                INSERT INTO PURCHASES (customer_id, product_id, quantity) VALUES
                    (1, 1, 1),
                    (1, 3, 1),
                    (4, 2, 2);");
        }

        private static void ExecuteNonQuery(MySqlConnection conn, string sql)
        {
            using var cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }
    }
}
