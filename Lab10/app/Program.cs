using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab10.utils;
using Lab10;
using System.Data.Common;
using Npgsql;



namespace Lab10.app
{
    internal class Program
    {
        private static readonly string path = "C:\\Users\\amana\\Downloads\\ticker (1).txt";
        private static readonly object lockObject = new object(); //лбъект для многопоточного доступа
        public static async Task Main(string[] args)
        {
            DateTime today = DateTime.Today;

            NpgsqlConnection connection = new NpgsqlConnection(DatabaseConfig.GetDsn()); //подключаемся к бд PostgreSQL
            await connection.OpenAsync();

            string createTickersTable = "CREATE TABLE IF NOT EXISTS Tickers " +
                                        "(id SERIAL PRIMARY KEY, " +
                                        "ticker VARCHAR(255));";

            string createPricesTable = "CREATE TABLE IF NOT EXISTS Prices " +
                                       "(id SERIAL PRIMARY KEY, " +
                                       "tickerid INT, " +
                                       "price DOUBLE PRECISION, " +
                                       "date VARCHAR(255));";

            string createTodaysConditionTable = "CREATE TABLE IF NOT EXISTS TodaysCondition " +
                                                "(id SERIAL PRIMARY KEY, " +
                                                "tickerid INT, " +
                                                "state VARCHAR(255));";

            NpgsqlCommand commandCreateTables = new NpgsqlCommand(createTickersTable
                                                                  + createPricesTable
                                                                  + createTodaysConditionTable, connection);
            commandCreateTables.ExecuteNonQuery(); //создание таблиц

            try
            {
                using (StreamReader reader = new StreamReader(path)) //открываем файл для чтения
                {
                    string ticker;
                    while ((ticker = reader.ReadLine()) != null)
                    {
                        double price;
                        try
                        {
                            price = await TickerPrice.GetTodayPrice(ticker); //запрос цены
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error fetching price for {ticker}: {ex.Message}");
                            continue;
                        }

                        NpgsqlConnection updateConnection = new NpgsqlConnection(DatabaseConfig.GetDsn());
                        await updateConnection.OpenAsync(); //обновление бд

                        NpgsqlCommand insertCommand = updateConnection.CreateCommand();
                        insertCommand.CommandText = "INSERT INTO Tickers (ticker) VALUES (@ticker) RETURNING id";
                        insertCommand.Parameters.AddWithValue("@ticker", ticker);
                        int tickerId = (int)insertCommand.ExecuteScalar();


                        insertCommand.CommandText = $"INSERT INTO Prices (tickerid, price, date) VALUES (@tickerId, @price, @today)";
                        insertCommand.Parameters.AddWithValue("@tickerId", tickerId);
                        insertCommand.Parameters.AddWithValue("@price", price);
                        insertCommand.Parameters.AddWithValue("@today", today.ToString("yyyy-MM-dd"));
                        insertCommand.ExecuteNonQuery();

                        updateConnection.Close();
                        await AnalyzeAndUpdateCondition(connection, ticker, price, today); //анализ цены

                    }

                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }

            connection.Close(); //закрытие соединенеия

            NpgsqlConnection newConnection = new NpgsqlConnection(DatabaseConfig.GetDsn()); //создание нового соединения

            try
            {

                while (true)
                {
                    Console.Write("Enter ticker to retrieve its condition (or 'exit' to exit): "); //работаем до exit
                    string userInput = Console.ReadLine();

                    if (userInput.ToLower() == "exit")
                        break;

                    string ticker = userInput.ToUpper();

                    await newConnection.OpenAsync();

                    lock (lockObject)
                    {
                        try
                        {
                            int tickerId = GetTickerId(newConnection, ticker);

                            if (tickerId == 0)
                            {
                                Console.WriteLine($"Ticker '{ticker}' not found in the database.");
                                continue;
                            }

                            string selectConditionQuery = "SELECT state FROM TodaysCondition WHERE tickerid = @tickerId"; //SQL-запрос для condition
                            NpgsqlCommand selectConditionCommand = new NpgsqlCommand(selectConditionQuery, newConnection);//команда
                            selectConditionCommand.Parameters.AddWithValue("@tickerId", tickerId);

                            var condition = selectConditionCommand.ExecuteScalar(); //выполняет SQL запрос к базе данных и возвращает результат выполнения запроса в переменную condition.

                            Console.WriteLine($"Condition for {ticker}: {condition}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error retrieving condition for {ticker}: {ex.Message}");
                        }
                    }
                    newConnection.Close();
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }
            connection.Close();
        }

        private static async Task AnalyzeAndUpdateCondition(NpgsqlConnection connection, string ticker, double todayPrice, DateTime today)
        {
            try
            {
                int tickerId = GetTickerId(connection, ticker);

                double yesterdayPrice = await TickerPrice.GetYesterdayPrice(ticker);

                double condition = todayPrice - yesterdayPrice;

                NpgsqlCommand insertConditionCommand = connection.CreateCommand();
                insertConditionCommand.CommandText =
                    "INSERT INTO TodaysCondition (tickerid, state) VALUES (@tickerId, @state)";
                insertConditionCommand.Parameters.AddWithValue("@tickerId", tickerId);
                insertConditionCommand.Parameters.AddWithValue("@state", condition);
                await insertConditionCommand.ExecuteNonQueryAsync();
            }
            catch (Exception err)
            {
                Console.WriteLine("error");
            }
        }

        private static int GetTickerId(NpgsqlConnection connection, string ticker)
        {
            NpgsqlCommand command = connection.CreateCommand();
            command.CommandText = $"SELECT id FROM Tickers WHERE ticker = '{ticker}'";
            command.Parameters.AddWithValue("@ticker", ticker);
            return (int)command.ExecuteScalar(); //запрос по идентификатору
        }

    }
}
