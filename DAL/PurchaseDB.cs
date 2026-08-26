using Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DAL
{
    public class PurchaseDB : BaseDB
    {
        public async Task<List<Purchase>> SelectAllAsync()
        {
            List<Purchase> list = new List<Purchase>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT purchase_id, customer_id, product_id, purchase_date, quantity FROM PURCHASES";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
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

            return list;
        }
    }
}