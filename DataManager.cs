using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace OrderOnline
{
    public class AppDbContext : DbContext
    {
        private string dbConnection;

        public DbSet<Commodity> Commodities { get; set; }

        public DbSet<User> Users { get; set; }

        public AppDbContext(string dbConnection) : base()
        {
            this.dbConnection = dbConnection;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // 配置 SQLite 数据库连接
            optionsBuilder.UseSqlite(this.dbConnection);
        }
    }

    public class DataManager
    {
        public static void CheckDBExist()
        {
            string dbPath = GetDBPath();
            string connectionStr = $"Data Source={dbPath}";
            if (!File.Exists(dbPath))
            {
                using (var dbContext = new AppDbContext(connectionStr))
                {
                    dbContext.Database.EnsureCreated();
                }
            }

            // Console.WriteLine(dbPath);

        }

        public static string GetDBPath()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            var dbPath = configuration["DBPath"];

            if (dbPath == null)
            {
                Console.WriteLine("Database Path is Null!");
                dbPath = "";
            }
            return dbPath;
        }
    }

    public class CommoditiesManager
    {

        public static bool CheckName(string name)
        {
            var dbPath = DataManager.GetDBPath();
            using(var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();

                string selectSql = "SELECT * FROM Commodities WHERE Name = @Name";

                using (var command = new SqliteCommand(selectSql, connection))
                {
                    command.Parameters.AddWithValue("@Name", name);
                    using(var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            connection.Close();
                            return true;
                        }
                        else
                        {
                            connection.Close();
                            return false;
                        }
                    }
                }
            }
        }

        public static void AddCommodity(Commodity commodity)
        {
            var dbPath = DataManager.GetDBPath();
            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                string insertSql = "INSERT INTO Commodities (Name, Description, Price, ImagePaths, Sales, Inventory) VALUES (@Name, @Description, @Price, @ImagePaths, @Sales, @Inventory)";

                using(var command = new SqliteCommand(insertSql, connection))
                {
                    command.Parameters.AddWithValue("@Name", commodity.Name);
                    command.Parameters.AddWithValue("@Description", commodity.Description);
                    command.Parameters.AddWithValue("@Price", commodity.Price);
                    command.Parameters.AddWithValue("@ImagePaths", commodity.ImagePathsJson);
                    command.Parameters.AddWithValue("@Sales", commodity.Sales);
                    command.Parameters.AddWithValue("@Inventory", commodity.Inventory);

                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        public static void ModifyCommodity(Commodity commodity)
        {
            var dbPath = DataManager.GetDBPath();
            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                string updateSql = "UPDATE Commodities SET Name = @Name, Description = @Description, Price = @Price,  ImagePaths = @ImagePaths,  Sales = @Sales, Inventory = @Inventory WHERE Id = @Id";
                using(var command = new SqliteCommand(updateSql, connection))
                {
                    command.Parameters.AddWithValue("@Id", commodity.Id);
                    command.Parameters.AddWithValue("@Name", commodity.Name);
                    command.Parameters.AddWithValue("@Description", commodity.Description);
                    command.Parameters.AddWithValue("@Price", commodity.Price);
                    command.Parameters.AddWithValue("@ImagePaths", commodity.ImagePathsJson);
                    command.Parameters.AddWithValue("@Sales", commodity.Sales);
                    command.Parameters.AddWithValue("@Inventory", commodity.Inventory);

                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        public static void DeleteCommodity(int id)
        {
            var dbPath = DataManager.GetDBPath();
            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                string deleteSql = "DELETE FROM Commodities WHERE Id = @Id";
                using(var command = new SqliteCommand(deleteSql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        public static Commodity[] GetCommodities(int limit, int skip)
        {
            var dbPath = DataManager.GetDBPath();
            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                string selectSql = "SELECT * FROM Commodities LIMIT @Limit OFFSET @Skip";
                var commoditiesList = new List<Commodity>();
                using(var command = new SqliteCommand(selectSql, connection))
                {
                    command.Parameters.AddWithValue("@Limit", limit);
                    command.Parameters.AddWithValue("@Skip", skip);
                    using(var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string name = reader.GetString(1);
                            string description = reader.GetString(2);
                            decimal price = reader.GetDecimal(3);
                            string imagePaths = reader.GetString(4);
                            int sales = reader.GetInt32(5);
                            int inventory = reader.GetInt32(6);
                            Commodity commodity = new Commodity
                            {
                                Id = id,
                                Name = name,
                                Description = description,
                                Price = price,
                                ImagePathsJson = imagePaths,
                                Sales = sales,
                                Inventory = inventory
                            };
                            commoditiesList.Add(commodity);
                        }
                    }
                }
                connection.Close();
                return commoditiesList.ToArray();
            }
        }

    }

    public class UsersManager
    {
        public static Result RegisterUser(UserDto user)
        {
            var dbPath = DataManager.GetDBPath();
            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                var checkSql = "SELECT * FROM Users WHERE PhoneNumber = @PhoneNumber";
                using (var command = new SqliteCommand(checkSql, connection))
                {
                    command.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            connection.Close();
                            var res = new Result
                            {
                                Success = false,
                                Code = 405,
                                Message = "PhoneNumber Exists."
                            };
                            return res;
                        }
                        else
                        {
                            var insertSql = "INSERT INTO Users (PhoneNumber, Name, Password, Adress) VALUES (@PhoneNumber, @Name, @Password, @Adress)";
                            using(var insertCommand = new SqliteCommand(insertSql, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                                insertCommand.Parameters.AddWithValue("@Name", user.Name);
                                insertCommand.Parameters.AddWithValue("@Password", user.Password);
                                insertCommand.Parameters.AddWithValue("@Adress", user.Adress);

                                insertCommand.ExecuteNonQuery();
                            }
                            connection.Close();
                            return new Result { Success = true, Code = 200 };
                        }
                    }
                }
            }
        }

        public static Result CheckLogin(string phoneNumber, string password)
        {
            var dbPath = DataManager.GetDBPath();
            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();

                string selectSql = "SELECT * FROM Users WHERE PhoneNumber = @PhoneNumber";
                using (var command = new SqliteCommand(selectSql, connection))
                {
                    command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string localPassword = reader.GetString(3);
                            if (localPassword != password)
                            {
                                return new Result
                                {
                                    Code = 501,
                                    Success = false,
                                    Message = "Invalid Password."
                                };
                            }
                            var data = new UserResult
                            {
                                Id = reader.GetInt32(0),
                                PhoneNumber = reader.GetString(1),
                                Name = reader.GetString(2),
                                Adress = reader.GetString(4),
                            };
                            connection.Close();
                            return new ResultWithDataAndToken<UserResult>
                            {
                                Success = true,
                                Code = 200,
                                Data = data
                            };
                        }
                        else
                        {
                            connection.Close();
                            return new Result
                            {
                                Code = 503,
                                Success = false,
                                Message = "Invalid PhoneNumber"
                            };
                        }
                    }
                }
            }
        }

        public static string GetRefreshToken(string phoneNumber)
        {
            var dbPath = DataManager.GetDBPath();
            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                string selectSql = "SELECT * FROM Users WHERE PhoneNumber = @PhoneNumber";
                using (var command = new SqliteCommand(selectSql, connection))
                {
                    command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string refreshToken = reader.GetString(5);
                            connection.Close();
                            return refreshToken;
                        }
                        else
                        {
                            connection.Close() ;
                            return null;
                        }
                    }
                }
            }
                return "";
        }

        public static void WriteRefreshToken(string phoneNumber ,string refreshToken)
        {
            var dbPath = DataManager.GetDBPath();
            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                string updateSql = "UPDATE Users SET RefreshToken = @NewRefreshToken WHERE PhoneNumber = @PhoneNumber";
                using (var command = new SqliteCommand(updateSql, connection))
                {
                    command.Parameters.AddWithValue("@NewRefreshToken", refreshToken);
                    command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }
    }

    public class Encrypt
    {
        public readonly static string PublicKey  = "<RSAKeyValue><Modulus>0J2awFKP4dpLivyeTWyLzxz59K7tYa63b4V54FpgvzfzwBHSpbcvV2RB+zoG/8pLRwzX4jIC3gcPSOrkA7iOayat1yKO4q31d6Vc/nzrUVOqydui9b9r8+f7VB4uMbKq551l+dcNeT/jDzYX0VMXYrc4UFeLdyu7M/WcqM/cNDjskT+oynBFpRYV0TZI92gjzdC1MfqZw9TQL3dNkeZPJQ/WrVCPu9G7P/3WmQfHtpGtSPwpUOoFqoBQiPWFJ2eh4R1W/JFKUJ8J8ve1LUOK13Xm4G210k9uBJCj5dusNvKeiznUmyCzeKJ+o15ba5vLkg8cA+OOuPgs6Fvh8rJwRQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        public readonly static string PrivateKey = "<RSAKeyValue><Modulus>0J2awFKP4dpLivyeTWyLzxz59K7tYa63b4V54FpgvzfzwBHSpbcvV2RB+zoG/8pLRwzX4jIC3gcPSOrkA7iOayat1yKO4q31d6Vc/nzrUVOqydui9b9r8+f7VB4uMbKq551l+dcNeT/jDzYX0VMXYrc4UFeLdyu7M/WcqM/cNDjskT+oynBFpRYV0TZI92gjzdC1MfqZw9TQL3dNkeZPJQ/WrVCPu9G7P/3WmQfHtpGtSPwpUOoFqoBQiPWFJ2eh4R1W/JFKUJ8J8ve1LUOK13Xm4G210k9uBJCj5dusNvKeiznUmyCzeKJ+o15ba5vLkg8cA+OOuPgs6Fvh8rJwRQ==</Modulus><Exponent>AQAB</Exponent><P>6ZjTcZMTEsHVcT6DmY950d6DWIwJwCvYhfWXzlV9rP2qU/FkAS6J59iLaCoG0gMcQh32gEdittHdIP/tEnucV7zFVAtsY+8/VGgBshl9twm8pgrFjp/uGW2Hv92/NLGroNjdlIooKVbHS4e6sWrAAH3zHDTWyJUExDCZYSpBjHM=</P><Q>5J9yidouxLlPeTpe/vj3Sa9hk2AXKJoLrcwBKcX3FE8A0Ex3EKduFQt3Pv+imiZoZsy8QRWUV7LZcA1ylzkE434Oray1BT3Cm/TpUaMQee3UElvjkiJRR0TeBIOH+Mv8hdgAHE8d5Rcx0+Pvs7+Z+qGgXmb8IKNwTc38Uh8j2mc=</Q><DP>UM+d1lya7JJB1Ltbq3QHIKNprOhFN0Xz0eP0cF0C7SWUFxYbEo2XB63SDGb9lQmebQEQlbAcZkKTzSa8TPiWTYPwf/KzvJ63ueuBKqvG4dtsd8SiM4UASauqmqWL0B7m0O41OX3SBvsOVwLNgzL62TC5ObjN1PY4f+aQKR0FMCE=</DP><DQ>zuEaHHQ67fliWSjg3Ykc/Kife2Twj+UVvGdmhg6FzvLOoa8P7xoTGygM+A0LbsJipuONVrfYTKOi7yq8Duuh6NohHjeydtO0TrwIhb8xIaR8y0ArZgl30y4WWa9MU28DS3pyXyuYub2LcVpJhjZTd+DZ5ZL7g+1hqoZDVWyoXPE=</DQ><InverseQ>xSSBTsImEZ2qRIV3rAif17sifk1wjEoEKShqCh5f63pec83gbwZZucFWThDJwh0qIWWqPD2843+12EGV6F2pWxQcT57Lz7DFVw/ClYZlf4XZewEsQYAqNHnvm7JsJStqii5YSrHHhgVcIHH9/muzV9QSXGCNWrQtCvloKZLmToQ=</InverseQ><D>NX1zxIRm6B406HpFjMycPIrNfHOt4jIOTsYGrgP+colCMqlfPaZuuRW35VbHnKaeDqW4ZQM1wQBGZwfzVxDnU4ojYNo3kN/R5M+9vHMDU2MEk6WBb0mZwHxm8PacGZoaQDEXiKwfhEthSPExwjZv9JvOEKuh0vifk38SoCZL7AeF/IJodVAGZzKzTn6G6qzkrJ8kiIh83V/ofDNZ7njHSgjnx7R0h48BRExvqTqkdtIzoIlp4h8WR9YnBuUx04gToeeDuXyGWb9aZCL1DFUzqjj1ZbuGDQxQnhToIvKAW7pAaPjfIkGRt94d0AaQycK6U+BnTmuWRBFZwT9Tj411MQ==</D></RSAKeyValue>";
        public static void MakeRSAKey()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                // 获取生成的公钥
                string publicKey = rsa.ToXmlString(false);
                string privateKey = rsa.ToXmlString(true);

                // 输出生成的公钥
                Console.WriteLine($"Public Key: {publicKey}");
                Console.WriteLine($"Private Key: {privateKey}");
            }
        }

        public static string EncryptPW(string password)
        {
            using(var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(Encrypt.PublicKey);
                byte[] data = rsa.Encrypt(Encoding.UTF8.GetBytes(password), RSAEncryptionPadding.OaepSHA256);
                return Convert.ToBase64String(data);
            }
        }

        public static string DecryptPW(string encrptedPassWord)
        {
            using( var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(Encrypt.PrivateKey);
                byte[] data = rsa.Decrypt(Convert.FromBase64String(encrptedPassWord), RSAEncryptionPadding.OaepSHA256);
                return Convert.ToBase64String(data);
            }
        }

    }
}
