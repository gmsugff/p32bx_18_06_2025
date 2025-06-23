using System.Data.SQLite;

namespace p32bx_Datebasse
{
    internal class Program
    {
        private const string DbFile = "tea_shop.db";
        private static string ConnectionString => $"Data Source={DbFile};Version=3;";

        static void Main()
        {
            CreateDatabaseAndTable();
            InsertSampleDataIfEmpty();

            while (true)
            {
                Console.WriteLine("\n--- Магазин Чая ---");
                Console.WriteLine("1 - Добавить чай");
                Console.WriteLine("2 - Редактировать чай");
                Console.WriteLine("3 - Удалить чай");
                Console.WriteLine("4 - Показать все чаи");
                Console.WriteLine("5 - Показать чаи с упоминанием 'вишня' в описании");
                Console.WriteLine("6 - Показать чаи по себестоимости в диапазоне");
                Console.WriteLine("7 - Показать чаи по количеству грамм в диапазоне");
                Console.WriteLine("8 - Показать чаи из указанных стран");
                Console.WriteLine("9 - Отобразить страны и количество чаёв");
                Console.WriteLine("10 - Отобразить среднее количество грамм по каждой стране");
                Console.WriteLine("11 - Топ-3 самых дешёвых чая по стране");
                Console.WriteLine("12 - Топ-3 самых дорогих чая по стране");
                Console.WriteLine("13 - Топ-3 самых дешёвых чая по всем странам");
                Console.WriteLine("14 - Топ-3 самых дорогих чая по всем странам");
                Console.WriteLine("15 - Топ-3 стран по количеству чаёв");
                Console.WriteLine("16 - Топ-3 стран по количеству грамм");
                Console.WriteLine("17 - Топ-3 зелёных чаёв по количеству грамм");
                Console.WriteLine("18 - Топ-3 чёрных чаёв по количеству грамм");
                Console.WriteLine("19 - Топ-3 чая по каждому виду по количеству грамм");
                Console.WriteLine("0 - Выход");
                Console.Write("Введите номер: ");

                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1": AddTea(); break;
                    case "2": EditTea(); break;
                    case "3": DeleteTea(); break;
                    case "4": ShowAllTea(); break;
                    case "5": ShowTeaByDescriptionKeyword("вишня"); break;
                    case "6": ShowTeaByCostRange(); break;
                    case "7": ShowTeaByQuantityRange(); break;
                    case "8": ShowTeaByCountries(); break;
                    case "9": ShowCountryTeaCounts(); break;
                    case "10": ShowCountryAvgQuantity(); break;
                    case "11": ShowTop3CheapTeaByCountry(); break;
                    case "12": ShowTop3ExpensiveTeaByCountry(); break;
                    case "13": ShowTop3CheapTeaAllCountries(); break;
                    case "14": ShowTop3ExpensiveTeaAllCountries(); break;
                    case "15": ShowTop3CountriesByTeaCount(); break;
                    case "16": ShowTop3CountriesByTeaQuantity(); break;
                    case "17": ShowTop3TeaByTypeQuantity("зелёный"); break;
                    case "18": ShowTop3TeaByTypeQuantity("чёрный"); break;
                    case "19": ShowTop3TeaByEachType(); break;
                    case "0": return;
                    default: Console.WriteLine("Неверный ввод."); break;
                }
            }
        }

        static void CreateDatabaseAndTable()
        {
            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();

            string sql = @"
            CREATE TABLE IF NOT EXISTS Tea (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Country TEXT NOT NULL,
                Type TEXT NOT NULL CHECK(Type IN ('зелёный', 'чёрный', 'улун', 'белый', 'травяной')),
                Description TEXT,
                QuantityGrams INTEGER NOT NULL CHECK(QuantityGrams >= 0),
                Cost REAL NOT NULL CHECK(Cost >= 0)
            );";

            using var cmd = new SQLiteCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        static void InsertSampleDataIfEmpty()
        {
            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();

            string checkSql = "SELECT COUNT(*) FROM Tea;";
            using var checkCmd = new SQLiteCommand(checkSql, conn);
            long count = (long)checkCmd.ExecuteScalar();

            if (count == 0)
            {
                string insertSql = @"
                INSERT INTO Tea (Name, Country, Type, Description, QuantityGrams, Cost) VALUES
                ('Дянь Хун', 'Китай', 'чёрный', 'Классический красный чай с нотками вишни', 500, 800),
                ('Сенча', 'Япония', 'зелёный', 'Свежий зелёный чай с легким цветочным ароматом', 300, 1200),
                ('Улун Те Гуань Инь', 'Китай', 'улун', 'Полуферментированный чай с фруктовым оттенком', 250, 1500),
                ('Белый Пион', 'Китай', 'белый', 'Нежный чай с лёгким вкусом и ароматом вишни', 200, 1100),
                ('Травяной сбор ,'Вишнёвый сад', 'Россия', 'травяной', 'Сбор трав с ароматом вишни и ягод', 150, 600),
                    ('Ассам', 'Индия', 'чёрный', 'Крепкий чай с насыщенным вкусом', 400, 700),
                ('Габа Улун', 'Тайвань', 'улун', 'Чай с мягким вкусом и сладковатым послевкусием', 350, 1400),
                ('Зелёный Жасмин', 'Китай', 'зелёный', 'Зелёный чай с ароматом жасмина', 300, 1300);
                ";
                using var insertCmd = new SQLiteCommand(insertSql, conn);
                insertCmd.ExecuteNonQuery();
                Console.WriteLine("Добавлены примерные данные.");
            }
        }

        static void AddTea()
        {
            Console.WriteLine("Добавление нового чая.");
            string name = ReadString("Название: ");
            string country = ReadString("Страна: ");
            string type = ReadTeaType();
            string description = ReadString("Описание: ");
            int quantity = ReadInt("Количество грамм: ", 0, int.MaxValue);
            double cost = ReadDouble("Себестоимость: ", 0, double.MaxValue);

            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();

            string sql = @"INSERT INTO Tea (Name, Country, Type, Description, QuantityGrams, Cost)
                       VALUES (@name, @country, @type, @desc, @qty, @cost);";

            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@country", country);
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@desc", description);
            cmd.Parameters.AddWithValue("@qty", quantity);
            cmd.Parameters.AddWithValue("@cost", cost);

            int rows = cmd.ExecuteNonQuery();
            Console.WriteLine(rows > 0 ? "Чай добавлен." : "Ошибка при добавлении.");
        }

        static void EditTea()
        {
            int id = ReadInt("Введите Id чая для редактирования: ", 1, int.MaxValue);

            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();

            string checkSql = "SELECT COUNT(*) FROM Tea WHERE Id = @id;";
            using var checkCmd = new SQLiteCommand(checkSql, conn);
            checkCmd.Parameters.AddWithValue("@id", id);
            long count = (long)checkCmd.ExecuteScalar();
            if (count == 0)
            {
                Console.WriteLine("Чай с таким Id не найден.");
                return;
            }

            string name = ReadString("Новое название: ");
            string country = ReadString("Новая страна: ");
            string type = ReadTeaType();
            string description = ReadString("Новое описание: ");
            int quantity = ReadInt("Новое количество грамм: ", 0, int.MaxValue);
            double cost = ReadDouble("Новая себестоимость: ", 0, double.MaxValue);

            string updateSql = @"
            UPDATE Tea SET
                Name = @name,
                Country = @country,
                Type = @type,
                Description = @desc,
                QuantityGrams = @qty,
                Cost = @cost
            WHERE Id = @id;
        ";

            using var updateCmd = new SQLiteCommand(updateSql, conn);
            updateCmd.Parameters.AddWithValue("@name", name);
            updateCmd.Parameters.AddWithValue("@country", country);
            updateCmd.Parameters.AddWithValue("@type", type);
            updateCmd.Parameters.AddWithValue("@desc", description);
            updateCmd.Parameters.AddWithValue("@qty", quantity);
            updateCmd.Parameters.AddWithValue("@cost", cost);
            updateCmd.Parameters.AddWithValue("@id", id);

            int rows = updateCmd.ExecuteNonQuery();
            Console.WriteLine(rows > 0 ? "Чай обновлён." : "Ошибка при обновлении.");
        }

        static void DeleteTea()
        {
            int id = ReadInt("Введите Id чая для удаления: ", 1, int.MaxValue);

            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();

            string deleteSql = "DELETE FROM Tea WHERE Id = @id;";
            using var deleteCmd = new SQLiteCommand(deleteSql, conn);
            deleteCmd.Parameters.AddWithValue("@id", id);

            int rows = deleteCmd.ExecuteNonQuery();
            Console.WriteLine(rows > 0 ? "Чай удалён." : "Чай с таким Id не найден.");
        }

        static void ShowAllTea()
        {
            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();

            string sql = "SELECT * FROM Tea ORDER BY Id;";
            using var cmd = new SQLiteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            Console.WriteLine("Все чаи:");
            while (reader.Read())
            {
                PrintTea(reader);
            }
        }

        static void ShowTeaByDescriptionKeyword(string keyword)
        {
            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();

            string sql = "SELECT * FROM Tea WHERE Description LIKE @kw;";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@kw", "%" + keyword + "%");

            using var reader = cmd.ExecuteReader();

            Console.WriteLine($"Чаи с упоминанием '{keyword}' в описании:");
            while (reader.Read())
            {
                PrintTea(reader);
            }
        }

        static void ShowTeaByCostRange()
        {
            double minCost = ReadDouble("Минимальная себестоимость: ", 0, double.MaxValue);
            double maxCost = ReadDouble("Максимальная себестоимость: ", minCost, double.MaxValue);

            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();

            string sql = "SELECT * FROM Tea WHERE Cost BETWEEN @min AND @max ORDER BY Cost;";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@min", minCost);
            cmd.Parameters.AddWithValue("@max", maxCost);

            using var reader = cmd.ExecuteReader();

            Console.WriteLine($"Чаи с себестоимостью от {minCost} до {maxCost}:");
            while (reader.Read())
            {
                PrintTea(reader);
            }
        }

        static void ShowTeaByQuantityRange()
        {
            int minQty = ReadInt("Минимальное количество грамм: ", 0, int.MaxValue);
            int maxQty = ReadInt("Максимальное количество грамм: ", minQty, int.MaxValue);

            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();

            string sql = "SELECT * FROM Tea WHERE QuantityGrams BETWEEN @min AND @max ORDER BY QuantityGrams;";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@min", minQty);
            cmd.Parameters.AddWithValue("@max", maxQty);

            using var reader = cmd.ExecuteReader();

            Console.WriteLine($"Чаи с количеством грамм от {minQty} до {maxQty}:");
            while (reader.Read())
            {
                PrintTea(reader);
            }
        }

        static void ShowTeaByCountries()
        {
            Console.WriteLine("Введите страны через запятую (например: Китай,Япония,Россия):");
            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Пустой ввод.");
                return;
            }
            string[] countries = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();

          
            List<string> paramNames = new();
            var cmd = new SQLiteCommand(conn);
            for (int i = 0; i < countries.Length; i++)
            {
                string param = $"@country{i}";
                paramNames.Add(param);
                cmd.Parameters.AddWithValue(param, countries[i]);
            }

            string sql = $"SELECT * FROM Tea WHERE Country IN ({string.Join(',', paramNames)}) ORDER BY Country, Name;";
            cmd.CommandText = sql;

            using var reader = cmd.ExecuteReader();

            Console.WriteLine($"Чаи из стран: {string.Join(", ", countries)}");
            while (reader.Read())
            {
                PrintTea(reader);
            }
        }

        static void ShowCountryTeaCounts()
        {
            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();

            string sql = "SELECT Country, COUNT(*) AS TeaCount FROM Tea GROUP BY Country ORDER BY TeaCount DESC;";
            using var cmd = new SQLiteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            Console.WriteLine("Количество чаёв по странам:");
            while (reader.Read())
            {
                Console.WriteLine($"{reader["Country"]}: {reader["TeaCount"]}");
            }
        }

        static void ShowCountryAvgQuantity()
        {
            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();

            string sql = "SELECT Country, AVG(QuantityGrams) AS AvgQuantity FROM Tea GROUP BY Country ORDER BY Country;";
            using var cmd = new SQLiteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            Console.WriteLine("Среднее количество грамм по странам:");
            while (reader.Read())
            {
                Console.WriteLine($"{reader["Country"]}: {Convert.ToDouble(reader["AvgQuantity"]):F2} г");
            }
        }

        static void ShowTop3CheapTeaByCountry()
        {
            string country = ReadString("Введите страну: ");
            ShowTop3TeaByCountryAndOrder(country, ascending: true);
        }

        static void ShowTop3ExpensiveTeaByCountry()
        {
            string country = ReadString("Введите страну: ");
            ShowTop3TeaByCountryAndOrder(country, ascending: false);
        }

        static void ShowTop3TeaByCountryAndOrder(string country, bool ascending)
        {
            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();

            string order = ascending ? "ASC" : "DESC";
            string sql = $@"
            SELECT * FROM Tea
            WHERE Country = @country
            ORDER BY Cost {order}
            LIMIT 3;
        ";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@country", country);

            using var reader = cmd.ExecuteReader();

            Console.WriteLine($"Топ-3 {(ascending ? "дешёвых" : "дорогих")} чаёв из {country}:");
            while (reader.Read())
            {
                PrintTea(reader);
            }
        }

        static void ShowTop3CheapTeaAllCountries()
        {
            ShowTop3TeaAllCountries(ascending: true);
        }

        static void ShowTop3ExpensiveTeaAllCountries()
        {
            ShowTop3TeaAllCountries(ascending: false);
        }

        static void ShowTop3TeaAllCountries(bool ascending)
        {
            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();

            string order = ascending ? "ASC" : "DESC";
            string sql = $@"
            SELECT * FROM Tea
            ORDER BY Cost {order}
            LIMIT 3;
        ";
            using var cmd = new SQLiteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            Console.WriteLine($"Топ-3 {(ascending ? "дешёвых" : "дорогих")} чаёв по всем странам:");
            while (reader.Read())
            {
                PrintTea(reader);
            }
        }

        static void ShowTop3CountriesByTeaCount()
        {
            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();

            string sql = @"
            SELECT Country, COUNT(*) as CountTea
            FROM Tea
            GROUP BY Country
            ORDER BY CountTea DESC
            LIMIT 3;
        ";
            using var cmd = new SQLiteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            Console.WriteLine("Топ-3 стран по количеству чаёв:");
            while (reader.Read())
            {
                Console.WriteLine($"{reader["Country"]}: {reader["CountTea"]}");
            }
        }

        static void ShowTop3CountriesByTeaQuantity()
        {
            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();

            string sql = @"
            SELECT Country, SUM(QuantityGrams) as SumQuantity
            FROM Tea
            GROUP BY Country
            ORDER BY SumQuantity DESC
            LIMIT 3;
        ";
            using var cmd = new SQLiteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            Console.WriteLine("Топ-3 стран по количеству грамм чая:");
            while (reader.Read())
            {
                Console.WriteLine($"{reader["Country"]}: {reader["SumQuantity"]} г");
            }
        }

        static void ShowTop3TeaByTypeQuantity(string type)
        {
            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();

            string sql = @"
            SELECT * FROM Tea
            WHERE Type = @type
            ORDER BY QuantityGrams DESC
            LIMIT 3;
        ";
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@type", type);

            using var reader = cmd.ExecuteReader();

            Console.WriteLine($"Топ-3 {type} чаёв по количеству грамм:");
            while (reader.Read())
            {
                PrintTea(reader);
            }
        }

        static void ShowTop3TeaByEachType()
        {
            string[] types = new[] { "зелёный", "чёрный", "улун", "белый", "травяной" };
            foreach (var type in types)
            {
                ShowTop3TeaByTypeQuantity(type);
                Console.WriteLine();
            }
        }

        
        static void PrintTea(SQLiteDataReader reader)
        {
            Console.WriteLine($"Id: {reader["Id"]}, Название: {reader["Name"]}, Страна: {reader["Country"]}, Вид: {reader["Type"]}, Описание: {reader["Description"]}, Кол-во (г): {reader["QuantityGrams"]}, Себестоимость: {reader["Cost"]}");
        }

        static string ReadString(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine()?.Trim() ?? "";
        }

        static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int val) && val >= min && val <= max)
                    return val;
                Console.WriteLine($"Введите число от {min} до {max}.");
            }
        }

        static double ReadDouble(string prompt, double min, double max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (double.TryParse(Console.ReadLine(), out double val) && val >= min && val <= max)
                    return val;
                Console.WriteLine($"Введите число от {min} до {max}.");
            }
        }

        static string ReadTeaType()
        {
            string[] validTypes = { "зелёный", "чёрный", "улун", "белый", "травяной" };
            while (true)
            {
                Console.Write($"Введите вид чая ({string.Join("/", validTypes)}): ");
                string input = Console.ReadLine()?.Trim().ToLower() ?? "";
                foreach (var t in validTypes)
                {
                    if (input == t)
                        return input;
                }
                Console.WriteLine("Неверный тип чая.");
            }
        }

    }
}
