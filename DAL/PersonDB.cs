using Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DAL
{
    public class PersonDB : BaseDB
    {
        // הבנאי מעביר את מחרוזת החיבור לבנאי האב (BaseDB)
        public PersonDB(string connectionString) : base(connectionString)
        {
        }
        // שליפת כל הרשומות מטבלת המשתמשים באופן אסינכרוני והחזרתן כרשימה
        public async Task<List<Person>> SelectAllAsync()
        {
            // יצירת רשימה ריקה לאחסון האובייקטים שיישלפו מסיס הנתונים
            List<Person> list = new List<Person>();
            try
            {
                // יצירת חיבור למסד הנתונים ודאגה לשחרור המשאב בסיום הביצוע
                // ניהול משאבים אוטומטי: מבטיח שחרור וסגירה של הרכיב בסיום הפעולה, גם אם התרחשה שגיאה
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    // פתיחת החיבור באופן אסינכרוני מבלי לחסום את תהליך ההרצה הראשי
                    await conn.OpenAsync();
                    // הגדרת שאילתת השליפה עבור השדות המבוקשים
                    string query = "SELECT person_id, full_name, email, role_code, created_at FROM person";

                    // יצירת אובייקט הפקודה המקשר בין השאילתה לחיבור הפעיל
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // הרצת השאילתה וקבלת קורא נתונים אסינכרוני
                        using (var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            // מעבר אסינכרוני שורה אחר שורה על תוצאות השליפה
                            while (await reader.ReadAsync())
                            {
                                // יצירת אובייקט חדש והשמת הערכים מתוך העמודות לפי מיקומן בשאילתה
                                list.Add(new Person
                                {
                                    Id = reader.GetInt32(0),
                                    FullName = reader.GetString(1),
                                    Email = reader.GetString(2),
                                    RoleCode = reader.GetInt32(3),
                                    CreatedAt = reader.GetDateTime(4)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // הדפסת פרטי השגיאה למעקב בזמן פיתוח
                Console.WriteLine($"Database Error in SelectAllAsync: {ex.Message}");
                // זריקת השגיאה מחדש כדי לאפשר לשכבה הקוראת לטפל בה
                // בעצם , ניתן להוסיף טיפול מותאם אישית או לוגיקה נוספת לפני זריקת השגיאה מחדש
                throw;
            }
            // החזרת המבנה המלא של האובייקטים
            return list;
        }
        // שליפת אדם בודד לפי מזהה מטבלת
        // person
        // שליפת משתמש בודד לפי מזהה ייחודי באופן אסינכרוני
        //הפעולה יכולה להחזיר נאל אם לא נמצאה רשומה תואמת
        public async Task<Person?> SelectByIdAsync(int id)
        {
            // הגדרת אובייקט המטרה שיחזור (מאותחל כריק במידה ולא יימצא מכרז מתאים)
            Person? person = null;
            try
            {
                // יצירת חיבור למסד הנתונים ודאגה לשחרור המשאב בסיום הביצוע
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    // פתיחת החיבור באופן אסינכרוני מבלי לחסום את תהליך ההרצה הראשי
                    await conn.OpenAsync();
                    // הגדרת שאילתת שליפה ממוקדת לפי מזהה בעזרת משתנה פרמטרי
                    string query = "SELECT person_id, full_name, email, role_code, created_at FROM person WHERE person_id = @id";

                    // יצירת אובייקט הפקודה המקשר בין השאילתה לחיבור הפעיל
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // השמת ערך המזהה לפרמטר באופן מאובטח למניעת הזרקת קוד זדוני
                        cmd.Parameters.AddWithValue("@id", id);
                        // הרצת השאילתה באופן אסינכרוני לקבלת קורא נתונים
                        using (var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            // בדיקה אסינכרונית האם קיימת שורת תוצאה תואמת
                            if (await reader.ReadAsync())
                            {
                                // יצירת האובייקט והשמת הערכים מתוך העמודות לפי מיקומן בשאילתה
                                person = new Person
                                {
                                    Id = reader.GetInt32(0),
                                    FullName = reader.GetString(1),
                                    Email = reader.GetString(2),
                                    RoleCode = reader.GetInt32(3),
                                    CreatedAt = reader.GetDateTime(4)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // הדפסת הודעת השגיאה למעקב בזמן פיתוח
                Console.WriteLine($"Database Error in SelectByIdAsync: {ex.Message}");
                // זריקת השגיאה מחדש לטיפול בשכבות העליונות
                throw;
            }
            // החזרת האובייקט שנמצא או ערך ריק אם הרשומה לא הייתה קיימת
            return person;
        }
        /// <summary>
        /// שליפת כל הרכישות של לקוח מסוים לפי מזהה הלקוח
        /// </summary>
        /// <param name="customerId">מזהה הלקוח לחיפוש</param>
        /// <returns>רשימת אובייקטים מסוג Purchase</returns>
        public async Task<List<Purchase>> SelectByCustomerIdAsync(int customerId)
        {
            // יצירת רשימה לאחסון תוצאות הרכישה
            List<Purchase> list = new List<Purchase>();

            try
            {
                // פתיחת חיבור למסד הנתונים ושחרור משאבים אוטומטי בסיום
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    // שאילתת SQL פרמטרית למניעת הזרקת קוד (SQL Injection)
                    string query = "SELECT purchase_id, customer_id, product_id, purchase_date, quantity FROM PURCHASES WHERE customer_id = @customerId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // השמת המשתנה לפרמטר בשאילתה
                        cmd.Parameters.AddWithValue("@customerId", customerId);

                        // הרצת השאילתה באופן אסינכרוני וקבלת קורא הנתונים
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            // מעבר אסינכרוני על כל השורות שחזרו מהמסד
                            while (await reader.ReadAsync())
                            {
                                // המרת השורה מהמסד לאובייקט Purchase והוספתו לרשימה
                                list.Add(new Purchase
                                {
                                    Id = reader.GetInt32("purchase_id"),
                                    CustomerId = reader.GetInt32("customer_id"),
                                    ProductId = reader.GetInt32("product_id"),
                                    PurchaseDate = reader.GetDateTime("purchase_date"),
                                    Quantity = reader.GetInt32("quantity")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // הדפסת השגיאה בחלון האבחון בזמן ריצה
                Debug.WriteLine($"Database Error in SelectByCustomerIdAsync: {ex.Message}");
                throw;
            }

            return list;
        }
    }
}