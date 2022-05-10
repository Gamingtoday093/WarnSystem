using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using WarnSystem.Models;
using Logger = Rocket.Core.Logging.Logger;
using System.Reflection;
using Rocket.Core.Utils;

namespace WarnSystem.Storage
{
    public class SQLStorage<T> where T : class
    {
        private string ConnectionString { get; set; }
        private string TableName { get; set; }
        public SQLStorage(string connectionString)
        {
            ConnectionString = GetConnectionValue(connectionString, "TABLENAME=", true);
            TableName = connectionString.Contains("TABLENAME=") ? GetConnectionValue(connectionString, "TABLENAME=", false) : "warnsystem";
        }

        private string GetConnectionValue(string connectionString, string Value, bool Remove)
        {
            if (Remove && !connectionString.Contains(Value)) return connectionString;
            if (Remove) return connectionString.Replace(connectionString.Substring(
                    connectionString.IndexOf(Value),
                    connectionString.IndexOf(";", connectionString.IndexOf(Value))
                    - (connectionString.IndexOf(Value) - 1)), "");

            return connectionString.Substring(
                connectionString.IndexOf(Value) + Value.Length,
                connectionString.IndexOf(";", connectionString.IndexOf(Value))
                - (connectionString.IndexOf(Value) + Value.Length));
        }

        public void CreateDatabase()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(GetConnectionValue(ConnectionString, "DATABASE=", true)))
                {
                    connection.Open();

                    MySqlCommand command = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{GetConnectionValue(ConnectionString, "DATABASE=", false)}`;", connection);
                    command.ExecuteNonQuery();

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                TaskDispatcher.QueueOnMainThread(() =>
                {
                    Logger.LogError($"[{Assembly.GetExecutingAssembly().FullName.Split(',')[0]}] Error when Creating Database: {e}");
                });    
            }
        }

        public void CreateTable(string CreateTableQuery)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();

                    MySqlCommand command = new MySqlCommand($"CREATE TABLE IF NOT EXISTS `{TableName}` ({CreateTableQuery});", connection);
                    command.ExecuteNonQuery();

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                TaskDispatcher.QueueOnMainThread(() =>
                {
                    Logger.LogError($"[{Assembly.GetExecutingAssembly().FullName.Split(',')[0]}] Error when Creating Database Table: {e}");
                });
            }
        }

        public List<Warn> Read()
        {
            List<Warn> warns = new List<Warn>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();

                    MySqlCommand command = new MySqlCommand($"SELECT * FROM `{TableName}`;", connection);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            warns.Add(new Warn()
                            {
                                owner = ulong.Parse(reader["SteamId"].ToString()),
                                moderatorSteamID64 = ulong.Parse(reader["ModeratorSteamId"].ToString()),
                                dateTime = DateTimeOffset.Parse(reader["DateTime"].ToString()),
                                reason = reader["Reason"].ToString()
                            });
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                TaskDispatcher.QueueOnMainThread(() =>
                {
                    Logger.LogError($"[{Assembly.GetExecutingAssembly().FullName.Split(',')[0]}] Error when Reading Database: {e}");
                });
            }
            return warns;
        }

        public async Task<List<Warn>> ReadAsync()
        {
            List<Warn> warns = new List<Warn>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    MySqlCommand command = new MySqlCommand($"SELECT * FROM `{TableName}`;", connection);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            warns.Add(new Warn()
                            {
                                owner = ulong.Parse(reader["SteamId"].ToString()),
                                moderatorSteamID64 = ulong.Parse(reader["ModeratorSteamId"].ToString()),
                                dateTime = DateTimeOffset.Parse(reader["DateTime"].ToString()),
                                reason = reader["Reason"].ToString()
                            });
                        }
                    }
                    await connection.CloseAsync();
                }
            }
            catch (Exception e)
            {
                TaskDispatcher.QueueOnMainThread(() =>
                {
                    Logger.LogError($"[{Assembly.GetExecutingAssembly().FullName.Split(',')[0]}] Error when Reading Database: {e}");
                });
            }
            return warns;
        }

        public async Task<List<Warn>> ReadWarnsAsync(ulong SteamID)
        {
            List<Warn> warns = new List<Warn>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    MySqlCommand command = new MySqlCommand($"SELECT * FROM `{TableName}` WHERE `SteamId`=@0;", connection);
                    command.Parameters.AddWithValue("@0", SteamID);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            warns.Add(new Warn()
                            {
                                owner = ulong.Parse(reader["SteamId"].ToString()),
                                moderatorSteamID64 = ulong.Parse(reader["ModeratorSteamId"].ToString()),
                                dateTime = DateTimeOffset.Parse(reader["DateTime"].ToString()),
                                reason = reader["Reason"].ToString()
                            });
                        }
                    }
                    await connection.CloseAsync();
                }
            }
            catch (Exception e)
            {
                TaskDispatcher.QueueOnMainThread(() =>
                {
                    Logger.LogError($"[{Assembly.GetExecutingAssembly().FullName.Split(',')[0]}] Error when Reading Database: {e}");
                });
            }
            return warns;
        }

        public async Task InsertAsync(Warn warning)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = new MySqlCommand($"INSERT INTO `{TableName}` (`SteamId`, `ModeratorSteamId`, `DateTime`, `Reason`) VALUES(@0, @1, @2, @3);", connection);
                    command.Parameters.AddWithValue("@0", warning.owner);
                    command.Parameters.AddWithValue("@1", warning.moderatorSteamID64);
                    command.Parameters.AddWithValue("@2", warning.dateTime.ToString());
                    command.Parameters.AddWithValue("@3", warning.reason);
                    await command.ExecuteNonQueryAsync();

                    await connection.CloseAsync();
                }
            }
            catch (Exception e)
            {
                TaskDispatcher.QueueOnMainThread(() =>
                {
                    Logger.LogError($"[{Assembly.GetExecutingAssembly().FullName.Split(',')[0]}] Error Inserting into Database: {e}");
                });
            }
        }

        public async Task DeleteAsync(Warn warning)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = new MySqlCommand($"DELETE FROM `{TableName}` WHERE `SteamId`=@0 AND `ModeratorSteamId`=@1 AND `DateTime`=@2 AND `Reason`=@3;", connection);
                    command.Parameters.AddWithValue("@0", warning.owner);
                    command.Parameters.AddWithValue("@1", warning.moderatorSteamID64);
                    command.Parameters.AddWithValue("@2", warning.dateTime.ToString());
                    command.Parameters.AddWithValue("@3", warning.reason);
                    await command.ExecuteNonQueryAsync();

                    await connection.CloseAsync();
                }
            }
            catch (Exception e)
            {
                TaskDispatcher.QueueOnMainThread(() =>
                {
                    Logger.LogError($"[{Assembly.GetExecutingAssembly().FullName.Split(',')[0]}] Error when Deleting from Database: {e}");
                });
            }
        }
    }
}
