using Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DAL
{
    // מחלקת גישה לנתונים עבור טבלת הרכישות, היורשת ממחלקת הבסיס
    public class PurchaseDB : BaseDB
    {
        // הפעולה הבונה מעבירה את מחרוזת החיבור לפעולה הבונה של מחלקת האב
        public PurchaseDB(string connectionString) : base(connectionString)
        {
        }

        // שליפת כל רשומות הרכישה באופן אסינכרוני כולל פרטי המוצר
        public async Task<List<Purchase>> SelectAllAsync()
        {
            // יצירת רשימה ריקה לאחסון אובייקטי הרכישות שיישלפו
            List<Purchase> list = new List<Purchase>();

            try
            {
                // יצירת חיבור למסד הנתונים ודאגה לשחרור המשאב בסיום הביצוע
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    // פתיחת החיבור באופן אסינכרוני מבלי לחסום את תהליך ההרצה הראשי
                    await conn.OpenAsync();
                    // הגדרת שאילתת השליפה הכוללת צירוף של טבלת המוצרים לקבלת שם המוצר
                    string query = @"SELECT pu.purchase_id, pu.customer_id, pu.product_id, pu.purchase_date, pu.quantity, 
                                            pr.product_name 
                                     FROM PURCHASES pu 
                                     INNER JOIN PRODUCTS pr ON pu.product_id = pr.product_id";

                    // יצירת אובייקט הפקודה המקשר בין השאילתה לחיבור הפעיל
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // הרצת השאילתה באופן אסינכרוני לקבלת קורא נתונים
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            // מעבר אסינכרוני שורה אחר שורה על כל תוצאות השליפה
                            while (await reader.ReadAsync())
                            {
                                // יצירת אובייקט רכישה והשמת הערכים כולל יצירת אובייקט מוצר מוכל
                                list.Add(new Purchase
                                {
                                    Id = reader.GetInt32("purchase_id"),
                                    CustomerId = reader.GetInt32("customer_id"),
                                    ProductId = reader.GetInt32("product_id"),
                                    PurchaseDate = reader.GetDateTime("purchase_date"),
                                    Quantity = reader.GetInt32("quantity"),
                                    Product = new Product
                                    {
                                        Id = reader.GetInt32("product_id"),
                                        ProductName = reader.GetString("product_name")
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // רישום פרטי השגיאה בחלון האבחון בזמן הרצה
                Debug.WriteLine($"Database Error in SelectAllAsync (PurchaseDB): {ex.Message}");
                // זריקת השגיאה מחדש לטיפול בשכבות העליונות
                throw;
            }

            // החזרת רשימת הרכישות המלאה
            return list;
        }

        // שליפת כל הרכישות של לקוח מסוים לפי מזהה הלקוח כולל פרטי המוצר
        public async Task<List<Purchase>> SelectByCustomerIdAsync(int customerId)
        {
            // יצירת רשימה ריקה לאחסון אובייקטי הרכישות שיישלפו
            List<Purchase> list = new List<Purchase>();

            try
            {
                // יצירת חיבור למסד הנתונים ודאגה לשחרור המשאב בסיום הביצוע
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    // פתיחת החיבור באופן אסינכרוני מבלי לחסום את תהליך ההרצה הראשי
                    await conn.OpenAsync();
                    // הגדרת שאילתת שליפה ממוקדת לפי קוד לקוח הכוללת צירוף של טבלת המוצרים
                    string query = @"SELECT pu.purchase_id, pu.customer_id, pu.product_id, pu.purchase_date, pu.quantity, 
                                            pr.product_name 
                                     FROM PURCHASES pu 
                                     INNER JOIN PRODUCTS pr ON pu.product_id = pr.product_id 
                                     WHERE pu.customer_id = @customerId";

                    // יצירת אובייקט הפקודה המקשר בין השאילתה לחיבור הפעיל
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // השמת ערך מזהה הלקוח לפרמטר באופן מאובטח למניעת הזרקת SQL
                        cmd.Parameters.AddWithValue("@customerId", customerId);

                        // הרצת השאילתה באופן אסינכרוני לקבלת קורא נתונים
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            // מעבר אסינכרוני שורה אחר שורה על תוצאות השליפה
                            while (await reader.ReadAsync())
                            {
                                // יצירת אובייקט רכישה והשמת הערכים כולל יצירת אובייקט מוצר מוכל
                                list.Add(new Purchase
                                {
                                    Id = reader.GetInt32("purchase_id"),
                                    CustomerId = reader.GetInt32("customer_id"),
                                    ProductId = reader.GetInt32("product_id"),
                                    PurchaseDate = reader.GetDateTime("purchase_date"),
                                    Quantity = reader.GetInt32("quantity"),
                                    Product = new Product
                                    {
                                        Id = reader.GetInt32("product_id"),
                                        ProductName = reader.GetString("product_name")
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // רישום פרטי השגיאה בחלון האבחון בזמן הרצה
                Debug.WriteLine($"Database Error in SelectByCustomerIdAsync (PurchaseDB): {ex.Message}");
                // זריקת השגיאה מחדש לטיפול בשכבות העליונות
                throw;
            }

            // החזרת רשימת הרכישות של הלקוח
            return list;
        }
    }
}