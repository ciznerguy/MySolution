using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Model;

namespace DAL
{
    public class PersonDB : BaseDB
    {
        // שליפת כל האנשים מטבלת person (אותיות קטנות)
        public async Task<List<Person>> SelectAllAsync()
        {
            List<Person> list = new List<Person>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT person_id, full_name, email, role_code, created_at FROM person";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
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
                Console.WriteLine($"Database Error in SelectAllAsync: {ex.Message}");
                throw;
            }
            return list;
        }

        // שליפת אדם בודד לפי מזהה מטבלת person (אותיות קטנות)
        public async Task<Person?> SelectByIdAsync(int id)
        {
            Person? person = null;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    string query = "SELECT person_id, full_name, email, role_code, created_at FROM person WHERE person_id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
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
                Console.WriteLine($"Database Error in SelectByIdAsync: {ex.Message}");
                throw;
            }
            return person;
        }
    }
}