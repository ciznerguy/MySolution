using MySql.Data.MySqlClient;
namespace DAL
{
    // מוודא בכל הרצה שבסיס הנתונים, הטבלאות ונתוני הדוגמה קיימים - יוצר אותם אם חסרים
    public static class DatabaseInitializer
    {
        // הפעולה הראשית המוודאת כי בסיס הנתונים מוגדר ומוכן לפעולה
        public static void EnsureDatabaseReady(string connectionString)
        {
            // פירוק מחרוזת ההתקשרות לרכיבים בודדים לצורך שינוי פרטי החיבור
            // מאפשר הוצאת שם בסיס הנתונים מהחיבור הראשוני כדי ליצור אותו אם אינו קיים
            var csBuilder = new MySqlConnectionStringBuilder(connectionString);
            string databaseName = csBuilder.Database;

            // שלב 1: יצירת בסיס הנתונים אם אינו קיים - חיבור לשרת ללא ציון שם בסיס נתונים
            // ללא שם בסיס הנתונים, החיבור מתבצע לשרת עצמו ומאפשר יצירת בסיס הנתונים
            csBuilder.Database = "";
            // ניהול משאבים אוטומטי המבטיח את סגירת החיבור בסיום השימוש
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

        // פעולה האחראית על יצירת טבלאות המערכת
        private static void CreateTables(MySqlConnection conn)
        {
            // יצירת טבלת
            // PERSON
            // עם עמודות המייצגות את פרטי המשתמשים, כולל הגבלות ייחודיות ובדיקות תקינות

            CreateTableIfMissing(conn, "PERSON", @"
                CREATE TABLE PERSON (
                    person_id INT AUTO_INCREMENT PRIMARY KEY,
                    full_name VARCHAR(100) NOT NULL,
                    email VARCHAR(100) UNIQUE NOT NULL,
                    role_code INT CHECK (role_code BETWEEN 1 AND 3),
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );");

            // יצירת טבלת
            // PRODUCTS
            // עם עמודות המייצגות את פרטי המוצרים, כולל מחיר ומלאי
            CreateTableIfMissing(conn, "PRODUCTS", @"
                CREATE TABLE PRODUCTS (
                    product_id INT AUTO_INCREMENT PRIMARY KEY,
                    product_name VARCHAR(100) NOT NULL,
                    price DECIMAL(10, 2) NOT NULL,
                    stock_quantity INT DEFAULT 0
                );");

            // יצירת טבלת
            // PURCHASES
            // עם עמודות המייצגות את פרטי הרכישות,
            // כולל קשרים לטבלאות
            // PERSON ו-PRODUCTS

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

        // בדיקה בטבלאות המערכת האם הטבלה כבר קיימת במטרה למנוע יצירה כפולה
        private static void CreateTableIfMissing(MySqlConnection conn, string tableName, string createTableSql)
        {
            // שאילתה המופנית לטבלת האבחון של בסיס הנתונים כדי לספור טבלאות קיימות
            using (var checkCmd = new MySqlCommand(
                "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = @tableName;",
                conn))
            {
                // העברת פרמטר באופן מאובטח למניעת הזרקת קוד זדוני
                checkCmd.Parameters.AddWithValue("@tableName", tableName);
                // הרצת השאילתה וקבלת ערך יחיד המייצג את מספר הטבלאות שנמצאו
                long exists = Convert.ToInt64(checkCmd.ExecuteScalar());
                if (exists > 0)
                {
                    return;
                }
            }

            ExecuteNonQuery(conn, createTableSql);
        }

        // מזין נתוני דוגמה (4 משתמשים, 4 מוצרים, 3 הזמנות) רק אם טבלת המשתמשים ריקה
        private static void SeedDataIfEmpty(MySqlConnection conn)
        {
            // בדיקה האם כבר קיימים נתונים בטבלת המשתמשים
            using (var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM PERSON;", conn))
            {
                long existingCount = Convert.ToInt64(checkCmd.ExecuteScalar());
                if (existingCount > 0)
                {
                    return;
                }
            }

            // הכנסת נתוני התחלה לטבלת המשתמשים
            ExecuteNonQuery(conn, @"
                INSERT INTO PERSON (full_name, email, role_code) VALUES
                    ('Dani Din', 'dani@gmail.com', 1),
                    ('Gadi Sales', 'gadi@store.com', 2),
                    ('Big Boss', 'admin@store.com', 3),
                    ('Noa Cohen', 'noa@store.com', 1);");

            // הכנסת נתוני התחלה לטבלת המוצרים
            ExecuteNonQuery(conn, @"
                INSERT INTO PRODUCTS (product_name, price, stock_quantity) VALUES
                    ('מקלדת אלחוטית', 89.90, 50),
                    ('עכבר אופטי', 39.90, 120),
                    ('מסך 24 אינץ', 649.00, 15),
                    ('אוזניות Bluetooth', 129.00, 40);");

            // הכנסת נתוני התחלה לטבלת הרכישות המקשרת בין משתמשים למוצרים
            ExecuteNonQuery(conn, @"
                INSERT INTO PURCHASES (customer_id, product_id, quantity) VALUES
                    (1, 1, 1),
                    (1, 2, 1),
                     (1, 3, 1),
                     (1, 4, 1),
                    (4, 2, 2);");
        }

        // פעולת עזר להרצת פקודות שלא מחזירות תוצאות טבלאיות (כמו הכנסת נתונים או יצירה)
        private static void ExecuteNonQuery(MySqlConnection conn, string sql)
        {
            using var cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }
    }
}