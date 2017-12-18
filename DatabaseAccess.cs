using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Models
{
    internal class DatabaseAccess //Database Access
    {
        private const string _connectionString = @"Data Source=.\SQL2017XPS;Initial Catalog=afdemp_csharp_1;Integrated Security = true; Trusted_Connection = True;";

        public List<User> GetUsers()
        {
            var users = new List<User>();
            var selectQuery = $"SELECT id, username FROM users;";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(selectQuery, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id = (int)reader["id"];
                                string username = (string)reader["username"];

                                var user = new User()
                                {
                                    Id = id,
                                    Username = username
                                };

                                users.Add(user);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CatchException(ex);
            }
            return users;
        }

        public string GetPassword(string username)
        {
            string password = "";
            var users = GetUsers();
            var id = users.Find(i => i.Username == username).Id;
            var openKeyQuery = $"OPEN SYMMETRIC KEY SQLSymmetricKey DECRYPTION BY CERTIFICATE SelfSignedCertificate; ";
            var selectQuery = $"SELECT password, CONVERT(varchar, DecryptByKey(password)) AS 'decryptedpassword'FROM users WHERE id=(@userId);";
            var closeKeyQuery = $"CLOSE SYMMETRIC KEY SQLSymmetricKey;";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(openKeyQuery + selectQuery + closeKeyQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", id);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                password = (string)reader["decryptedpassword"];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CatchException(ex);
            }
            return password;
        }

        public List<Account> GetAccounts()
        {
            var accounts = new List<Account>();
            var users = GetUsers();

            var selectQuery = "SELECT * FROM accounts";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(selectQuery, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id = (int)reader["id"];
                                int userId = (int)reader["user_id"];
                                DateTime date = (DateTime)reader["transaction_date"];
                                decimal amount = (decimal)reader["amount"];

                                var user = users.Find(i => i.Id == userId);

                                var account = new Account()
                                {
                                    Id = id,
                                    UserId = userId,
                                    TransactionDate = date,
                                    Amount = amount,

                                    User = user
                                };

                                accounts.Add(account);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CatchException(ex);                
            }
            return accounts;
        }

        public void UpdateAccount(int userId1, decimal amount1, int userId2, decimal amount2)
        {
            var updateQuery1 = "UPDATE accounts SET amount=(@amount1), transaction_date=(@now) WHERE user_id=(@userId1)";
            var updateQuery2 = "UPDATE accounts SET amount=(@amount2), transaction_date=(@now) WHERE user_id=(@userId2)";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(updateQuery1 + updateQuery2, conn))
                    {
                        cmd.Parameters.AddWithValue("@amount1", amount1);
                        cmd.Parameters.AddWithValue("@now", DateTime.Now);
                        cmd.Parameters.AddWithValue("@userId1", userId1);

                        cmd.Parameters.AddWithValue("@amount2", amount2);
                        cmd.Parameters.AddWithValue("@userId2", userId2);
                        var rowsAffected = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                CatchException(ex);
            }
        }

        public void UpdateAllAccounts(int[] userId, decimal[] amount)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    for (var i = 0; i < userId.Length; i++)
                    {
                        var updateQuery = "UPDATE accounts SET amount=(@amount), transaction_date=(@now) WHERE user_id=(@userId)";
                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@amount", amount[i]);
                            cmd.Parameters.AddWithValue("@now", DateTime.Now);
                            cmd.Parameters.AddWithValue("@userId", userId[i]);

                            var rowsAffected = cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CatchException(ex);
            }

        }

        private void CatchException(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nA server connection error occurred.");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Press Enter to see Details about the error or any other button to continue..");
            var key = Console.ReadKey();
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine($"\nError Details: {ex.Message}");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("\nPress any button to continue ..");
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadKey();
            }

            Console.Clear();
            Console.Title = "Internal Bank System - Exit";
            Console.WriteLine("\nApplication is closing..");
            Console.WriteLine($"Goodbye.");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("\nPress any button to exit the application ..");
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadKey();
            Environment.Exit(0);
            Console.ReadKey();
        }

    }
}

