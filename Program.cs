using System;
using Microsoft.Data.SqlClient;
using System.Text;

namespace TempProj
{
    class Program
    {
        static void Main(string[] args)
        {
            string conString = "Server=.;Database=DataBase;Trusted_Connection=true;";
            using SqlConnection connection = new SqlConnection(conString);
            connection.Open();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                System.Console.WriteLine("Не удалось подключиться к базе данных");
                return;
            }
            while (true)
            {
                InputHelper.WriteComands("Войти как админ", "Войти как клиент", "Выйти из приложения");
                int command = InputHelper.GetCommand(3);
                if (command == 3) return;
                CreditApp app = command == 1 ? new AdminApp(connection) : new ClientApp(connection);
                if (!InputHelper.Repeater(app.Login)) continue;
            }
        }
    }
    static class InputHelper
    {
        public static int GetInput()
        {
            System.Console.Write("Ввод : ");
            return int.Parse(Console.ReadLine());
        }
        public static int GetCommand(int countComands)
        {
            int res = GetInput();
            while (res > countComands || res < 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("Не правильно введенная команда, выберайте из существующих вариантов");
                Console.ResetColor();
                res = GetInput();
            }
            return res;
        }
        public static void WriteComands(params string[] comands)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            for (int i = 0; i < comands.Length; i++)
            {
                System.Console.WriteLine($"({i + 1})-{comands[i]}");
            }
            Console.ResetColor();
        }
        public static bool Repeater(Func<bool> func)
        {
            while (!func())
            {
                WriteComands("Повторить попытку", "Вернуться в главное меню");
                if (GetCommand(2) == 2)
                    return false;
                Console.Clear();
            }
            return true;
        }
        public static void FillParamsSql(SqlCommand command, params string[] sqlParams)
        {
            for (int i = 0; i < sqlParams.Length; i++)
                command.Parameters.AddWithValue($"@{i}", sqlParams[i]);
        }
    }
    abstract class CreditApp
    {
        protected int userID;
        protected SqlConnection connection;
        public void CreditGraph(decimal summ, DateTime date, int month)
        {
            decimal creditBody = summ / month;
            System.Console.WriteLine("Дата\tПлатеж\tПроценты\tТело Кредита\tОстаток");
        }
        public bool Login()
        {
            System.Console.Write("Логин : ");
            string login = Console.ReadLine();
            System.Console.Write("Пароль : ");
            string password = Console.ReadLine();
            string userType = this is AdminApp ? "admin" : "client";
            string query = $"select min(ID) from Users where Type={userType} and Phone={login} and Password={password}";
            try
            {
                SqlCommand command = new SqlCommand(query, connection);
                userID = (int)command.ExecuteScalar();
                if (userID < 1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine("Неправильный логин или пароль (возможно вы не зарегистрированы, попробуйте зарегистрироваться у администратора)");
                    Console.ResetColor();
                    return false;
                }
                return true;
            }
            catch
            {
                System.Console.WriteLine("Операция была завершена с исключением...");
                return false;
            }
        }
        abstract public bool FirstAction();
        abstract public bool SecondAction();
        abstract public bool ThirdAction();
    }
    class AdminApp : CreditApp
    {
        private int clientID = 0;
        public AdminApp(SqlConnection connection) => this.connection = connection;
        public void ShowClients()
        {
            string query = "select * from Users where Type='Client'";
            SqlCommand command = new SqlCommand(query, connection);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                System.Console.WriteLine("|ID\t|Телефон\t|Пароль\t|Имя\t|Фамилия\t|Отчество\t|День Рождения\t|Аддрес\t|Кредитоспособность");
                if (reader.HasRows)
                    while (reader.Read())
                    {

                    }
            }
        }
        public override bool FirstAction()
        {
            System.Console.Write("Выберите тип пользователя");
            InputHelper.WriteComands("Admin", "Client");
            string userType = InputHelper.GetCommand(2) == 1 ? "Admin" : "Client";
            System.Console.Write("Введите Имя пользователя : ");
            string name = Console.ReadLine();
            System.Console.Write("Введите Фамилию пользователя : ");
            string surName = Console.ReadLine();
            System.Console.Write("Введите Отчество пользователя : ");
            string midName = Console.ReadLine();
            System.Console.Write("Введите Дату рождения пользователя (DD/MM/YYYY) : ");
            DateTime birhDate = DateTime.Parse(Console.ReadLine());
            System.Console.Write("Введите аддрес пользователя : ");
            string addres = Console.ReadLine();
            System.Console.Write("Введите Логин для пользователя (телефон) : ");
            string login = Console.ReadLine();
            System.Console.Write("Введите Пароль для пользователя : ");
            string password = Console.ReadLine();
            SqlTransaction transaction = connection.BeginTransaction();
            string query = "insert Users values(@0,@1,@2,@3,@4,@5,@6,@birthDate,@7)";
            try
            {
                SqlCommand command = new SqlCommand(query, connection, transaction);
                command.Parameters.AddWithValue("@birthDate", birhDate);
                InputHelper.FillParamsSql(command, userType, login, password, name, surName, midName, addres);
                command.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }
        public override bool SecondAction()
        {
            throw new NotImplementedException();
        }

        public override bool ThirdAction()
        {
            throw new NotImplementedException();
        }
    }
    class ClientApp : CreditApp
    {
        public ClientApp(SqlConnection connection) => this.connection = connection;
        public override bool FirstAction()
        {
            throw new NotImplementedException();
        }

        public override bool SecondAction()
        {
            throw new NotImplementedException();
        }

        public override bool ThirdAction()
        {
            throw new NotImplementedException();
        }
    }
    class Table
    {
        private string[][] table;
        private int[] maxLRaw;
        private int raws = 0;
        private readonly int cols;
        public Table(params string[] firstRaw)
        {
            cols = firstRaw.Length;
            maxLRaw = new int[cols];
            for (int i = 0; i < cols; i++)
                maxLRaw[i] = 0;
            AddRaw(firstRaw);
        }
        public void AddRaw(params string[] raw)
        {
            Array.Resize<string[]>(ref table, raws + 1);
            table[^1] = new string[cols];
            for (int i = 0; i < cols; i++)
            {
                table[raws][i] = raw[i];
                maxLRaw[i] = raw[i].Length > maxLRaw[i] ? raw[i].Length : maxLRaw[i];
            }
            raws++;
        }
        public void ShowTable()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            StringBuilder sb = new StringBuilder(Sum() + cols + 1);
            sb.Append('|').Append(' ', maxLRaw[0] - table[0][0].Length).Append(table[0][0]).Append('|');
            for (int i = 1; i < cols; i++)
                sb.Append(' ', maxLRaw[i] - table[0][i].Length).Append(table[0][i]).Append('|');
            System.Console.WriteLine(sb);
            sb.Clear();
            sb.Append('|').Append('-', maxLRaw[0]).Append('|');
            for (int i = 1; i < cols; i++)
                sb.Append('-', maxLRaw[i]).Append('|');
            System.Console.WriteLine(sb);
            sb.Clear();
            for (int i = 1; i < raws; i++)
            {
                sb.Append('|').Append(' ', maxLRaw[0] - table[i][0].Length).Append(table[i][0]).Append('|');
                for (int j = 1; j < cols; j++)
                    sb.Append(' ', maxLRaw[j] - table[i][j].Length).Append(table[i][j]).Append('|');
                System.Console.WriteLine(sb);
                sb.Clear();
            }
            Console.ResetColor();
        }
        private int Sum()
        {
            int sum = 0;
            for (int i = 0; i < cols; i++)
                sum += maxLRaw[i];
            return sum;
        }
    }
}
