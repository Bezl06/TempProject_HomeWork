using System;
using Microsoft.Data.SqlClient;
using System.Text;

namespace TempProj
{
    class Program
    {
        static void Main(string[] args)
        {
            string conString = "Server=.;Database=CreditDataBase;Trusted_Connection=true;";
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
                while (!app.Login())
                {
                    InputHelper.WriteComands("Повторить попытку", "Вернуться в главное меню");
                    command = InputHelper.GetCommand(2) == 2 ? 0 : 1;
                    if (command == 0)
                        break;
                }
                if (command == 0) continue;
                Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine("Вход выполнен успешно");
                while (true)
                {
                    app.ShowComands();
                    command = InputHelper.GetCommand(4);
                    if (command == 4) break;
                    bool isFinished = command switch
                    {
                        1 => app.FirstAction(),
                        2 => app.SecondAction(),
                        _ => app.ThirdAction()
                    };
                    if (isFinished)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        System.Console.WriteLine("Операция была завершена без ошибок");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        System.Console.WriteLine("Операция не была завершена корректно...");
                    }
                    Console.ResetColor();
                    System.Console.WriteLine("Нажмите Enter чтобы вернуться в главное меню...");
                    Console.ReadLine();
                }
            }
        }
    }
    static class InputHelper
    {
        public static bool GetInput(out int input)
        {
            System.Console.Write("Ввод : ");
            return int.TryParse(Console.ReadLine(), out input);
        }
        public static int GetCommand(int countComands)
        {
            int res;
            while (!GetInput(out res) || res > countComands || res < 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("Не правильно введенная команда, выберайте из существующих вариантов");
                Console.ResetColor();
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
        public static void FillParamsSql(SqlCommand command, params string[] sqlParams)
        {
            for (int i = 0; i < sqlParams.Length; i++)
                command.Parameters.AddWithValue($"@{i}", sqlParams[i]);
        }
        public static string[] ParamsToString(SqlDataReader reader, int indData = -1)
        {
            int countColls = reader.FieldCount;
            string[] res = new string[countColls];
            for (int i = 0; i < countColls; i++)
            {
                if (i == indData) res[i] = ((DateTime)reader[i]).ToString("d");
                else res[i] = reader[i]?.ToString() ?? "-";
            }
            return res;
        }
    }
    abstract class CreditApp
    {
        protected int userID;
        protected SqlConnection connection;
        public void CreditGraph(decimal summ, DateTime date, int months)
        {
            decimal creditBody = Math.Round(summ / months, 2);
            Table table = new Table("Дата", "Тело Кредита", "Проценты", "Платеж", "Остаток");
            table.AddRaw(date.ToString("d"), creditBody.ToString(), "0", "0", summ.ToString());
            summ -= creditBody;
            decimal percents = Math.Round(summ * (0.12m / months), 2);
            for (int i = 1; i <= months; i++, summ -= creditBody, percents = Math.Round(summ * (0.12m / months), 2))
                table.AddRaw(date.AddMonths(i).ToString("d"), creditBody.ToString(), percents.ToString(), (percents + creditBody).ToString(), summ.ToString());
            Console.ForegroundColor = ConsoleColor.Blue;
            System.Console.WriteLine($"Срок кредита : {months}\nПроцентная ставка годовых : 12%");
            table.ShowTable();
        }
        protected bool CheckID(int id, string table, string condition)
        {
            string query = $"select count(*) from {table} where ID={id} and {condition}";
            try
            {
                SqlCommand command = new SqlCommand(query, connection);
                int temp = command.ExecuteScalar() as int? ?? 0;
                if (temp < 1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine("Неправильный ID , выберите из перечисленных в таблице идентификатор");
                    Console.ResetColor();
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Exception : {ex.Message}");
                return false;
            }
        }
        public bool Login()
        {
            System.Console.Write("Логин : ");
            string login = Console.ReadLine();
            System.Console.Write("Пароль : ");
            string password = Console.ReadLine();
            string userType = this is AdminApp ? "Admin" : "Client";
            string query = "select min(ID) from Users where Type=@0 and Phone=@1 and Password=@2";
            try
            {
                SqlCommand command = new SqlCommand(query, connection);
                InputHelper.FillParamsSql(command, userType, login, password);
                userID = command.ExecuteScalar() as int? ?? 0;
                if (userID < 1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    System.Console.WriteLine("Неправильный логин или пароль (возможно вы не зарегистрированы, попробуйте зарегистрироваться у администратора)");
                    Console.ResetColor();
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Exception : {ex.Message}");
                return false;
            }
        }
        abstract public void ShowComands();
        abstract public bool FirstAction();
        abstract public bool SecondAction();
        abstract public bool ThirdAction();
    }
    class AdminApp : CreditApp
    {
        private int clientID = 0;
        public AdminApp(SqlConnection connection) => this.connection = connection;
        public void ShowClients(bool hasForm)
        {
            string query = $"select ID,Phone,Password,FirstName,SurName,MiddleName,BirthDate,Addres{(hasForm ? ",Creditibility" : "")} from Users where Type='Client' and Creditibility is {(hasForm ? "not" : "")} null";
            SqlCommand command = new SqlCommand(query, connection);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                Table table = hasForm ? new("ID", "Телефон", "Пароль", "Имя", "Фамилия", "Отчество", "День Рождения", "Аддрес", "Кред.Баллы") : new("ID", "Телефон", "Пароль", "Имя", "Фамилия", "Отчество", "День Рождения", "Аддрес");
                while (reader.Read())
                    table.AddRaw(InputHelper.ParamsToString(reader, 6));
                table.ShowTable();
            }
        }
        public override void ShowComands() => InputHelper.WriteComands("Зарегистрировать пользователя", "Заполнить анкету клиента", "Добавить заявку на кредит клиенту", "Вернуться к выбору пользователя");
        public override bool FirstAction()
        {
            System.Console.WriteLine("Укажите тип пользователя");
            InputHelper.WriteComands("Admin", "Client");
            string userType = InputHelper.GetCommand(2) == 1 ? "Admin" : "Client";
            System.Console.Write("Введите Имя пользователя : ");
            string name = Console.ReadLine();
            System.Console.Write("Введите Фамилию пользователя : ");
            string surName = Console.ReadLine();
            System.Console.Write("Введите Отчество пользователя : ");
            string midName = Console.ReadLine();
            System.Console.Write("Введите Дату рождения пользователя (YYYY-MM-DD) : ");
            string birhDate = Console.ReadLine();
            System.Console.Write("Введите аддрес пользователя : ");
            string addres = Console.ReadLine();
            System.Console.Write("Введите Логин для пользователя (телефон) : ");
            string login = Console.ReadLine();
            System.Console.Write("Введите Пароль для пользователя : ");
            string password = Console.ReadLine();
            SqlTransaction transaction = connection.BeginTransaction();
            string query = "insert Users(Type,Phone,Password,FirstName,SurName,MiddleName,BirthDate,Addres) values(@0,@1,@2,@3,@4,@5,@6,@7)";
            try
            {
                SqlCommand command = new SqlCommand(query, connection, transaction);
                InputHelper.FillParamsSql(command, userType, login, password, name, surName, midName, birhDate, addres);
                command.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                System.Console.WriteLine($"Exception : {ex.Message}");
                return false;
            }
        }
        public override bool SecondAction()
        {
            System.Console.WriteLine("Введите ID клиента, анкету которого хотите заполнить : ");
            ShowClients(false);
            InputHelper.GetInput(out clientID);
            if (!CheckID(clientID, "Users", "Creditibility is Null"))
                return false;
            System.Console.WriteLine("Укажите пол клиента");
            InputHelper.WriteComands("Man", "Woman");
            string gender = InputHelper.GetCommand(2) == 1 ? "Man" : "Woman";
            int creditibility = gender == "Man" ? 1 : 2;
            System.Console.WriteLine("Укажите семейное положение клиента");
            InputHelper.WriteComands("Холост", "Семьянин", "В разводе", "Вдовец/Вдова");
            string famStatus = InputHelper.GetCommand(4) switch
            {
                1 => "Single",
                2 => "FamilyMan",
                3 => "Divorced",
                _ => "Widow"
            };
            creditibility += famStatus == "Single" || famStatus == "Divorced" ? 1 : 2;
            System.Console.Write("Введите возраст клиента : ");
            if (!int.TryParse(Console.ReadLine(), out int age) || age < 18 || age > 110)
            {
                System.Console.WriteLine("Неправильный возраст");
                return false;
            }
            creditibility += age < 26 ? 0 : age > 35 && age < 63 ? 2 : 1;
            System.Console.WriteLine("Укажите гражданство клиента");
            InputHelper.WriteComands("Таджикистан", "Зарубеж");
            string citizenship = InputHelper.GetCommand(2) == 1 ? "Tajikistan" : "Abroad";
            creditibility += citizenship == "Tajikistan" ? 1 : 0;
            SqlTransaction transaction = connection.BeginTransaction();
            string query = "insert Forms values(@0,@1,@2,@3,@4)";
            try
            {
                SqlCommand command = new SqlCommand(query, connection, transaction);
                InputHelper.FillParamsSql(command, clientID.ToString(), gender, famStatus, age.ToString(), citizenship);
                command.ExecuteNonQuery();
                command.Parameters.Clear();
                command.CommandText = $"update Users set Creditibility={creditibility} where ID={clientID}";
                command.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                System.Console.WriteLine($"Exception : {ex.Message}");
                return false;
            }
        }
        public override bool ThirdAction()
        {
            System.Console.WriteLine("Введите ID клиента, заявку которого хотите оформить : ");
            ShowClients(true);
            InputHelper.GetInput(out clientID);
            if (!CheckID(clientID, "Users", "Creditibility is not Null"))
                return false;
            System.Console.WriteLine("Введите сумму кредита : ");
            decimal.TryParse(Console.ReadLine(), out decimal summ);
            System.Console.WriteLine("Введите общий ежемесячный доход клиента : ");
            decimal.TryParse(Console.ReadLine(), out decimal profit);
            System.Console.WriteLine("Введите срок кредита : ");
            int.TryParse(Console.ReadLine(), out int period);
            if (summ <= 0 || profit <= 0 || period <= 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("Неправильно введенные данные");
                Console.ResetColor();
                return false;
            }
            System.Console.WriteLine("Укажите цель кредита клиента");
            InputHelper.WriteComands("Бытовая техника", "Ремонт", "Телефон", "Прочее");
            string target = InputHelper.GetCommand(4) switch
            {
                1 => "Appliances",
                2 => "Repair",
                3 => "Phone",
                _ => "Other"
            };
            int creditibility = 0, countCredits = 0, delays = -1;
            try
            {
                SqlCommand command = new SqlCommand($"select top 1 Creditibility from Users where ID={clientID}", connection);
                creditibility = (int)command.ExecuteScalar();
                command.CommandText = $"select count(*) from Credits where UserID={clientID} and Status=1";
                countCredits = command.ExecuteScalar() as int? ?? 0;
                if (countCredits > 0)
                {
                    command.CommandText = $"select sum(Delays) from Credits where UserID={clientID} and Status=1";
                    delays = (int)command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Exception : {ex.Message}");
                return false;
            }
            decimal perCent = summ / (profit * period);
            creditibility += perCent < 0.8m ? 4 : perCent < 1.5m ? 3 : perCent < 2.5m ? 2 : 1;
            creditibility += countCredits < 1 ? 0 : countCredits < 3 ? 2 : 3;
            creditibility += delays < 4 ? 0 : delays < 5 ? -1 : delays < 8 ? -2 : -3;
            creditibility += target == "Other" ? -1 : target == "Phone" ? 0 : target == "Repair" ? 1 : 2;
            if (creditibility <= 11)
            {
                string answer = "К сожалению заявка клиента была отклонена по следующим причинам :\n-Низкая кредитоспособность";
                if (delays > 3) answer += "\n-Наличие множественных просрочек в кредитной истории";
                if (countCredits < 1) answer += "\n-Отсутствие кредитной истории";
                if (target == "Other") answer += "\n-Отсутствие цели для приобретения кредита";
                if (perCent > 1.5m) answer += "\n-Отсутствие соответствующего дохода для погашения кредита";
                Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine(answer);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine("Заявка клиента была успешно одобрена");
            }
            Console.ResetColor();
            SqlTransaction transaction = connection.BeginTransaction();
            string query = "insert CreditApps values(@0,@date,@1,@2,@3,@4)";
            try
            {
                SqlCommand command = new SqlCommand(query, connection, transaction);
                InputHelper.FillParamsSql(command, clientID.ToString(), summ.ToString(), target, period.ToString(), (creditibility > 11).ToString());
                command.Parameters.AddWithValue("@date", DateTime.Now);
                command.ExecuteNonQuery();
                if (creditibility > 11)
                {
                    command.Parameters.Clear();
                    command.CommandText = "select max(id) from CreditApps";
                    int creditID = (int)command.ExecuteScalar();
                    command.CommandText = "insert Credits values(@0,@1,@date,@2,@3,@4)";
                    command.Parameters.AddWithValue("@date", DateTime.Now);
                    InputHelper.FillParamsSql(command, clientID.ToString(), creditID.ToString(), summ.ToString(), "0", "True");
                    command.ExecuteNonQuery();
                    System.Console.WriteLine("Кредит с 12% годовыми был успешно оформлен\nГрафик погашения кредита :");
                    CreditGraph(summ, DateTime.Now, period);
                }
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                System.Console.WriteLine($"Exception : {ex.Message}");
                return false;
            }
        }
    }
    class ClientApp : CreditApp
    {
        private int creditID;
        public ClientApp(SqlConnection connection) => this.connection = connection;
        public override void ShowComands() => InputHelper.WriteComands("Просмотреть историю заявок", "Просмотреть остаток кредитов", "Показать детали кредита в виде графика погашения", "Вернуться к выбору пользователя");
        public override bool FirstAction()
        {
            string query = $"select ID,Date,Summ,Target,Period,Status from CreditApps where UserID={userID}";
            try
            {
                SqlCommand command = new SqlCommand(query, connection);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Table table = new Table("ID", "Дата", "Сумма", "Цель", "Срок", "Результат");
                    while (reader.Read())
                        table.AddRaw(InputHelper.ParamsToString(reader, 1));
                    table.ShowTable();
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Exception : {ex.Message}");
                return false;
            }
        }
        public override bool SecondAction()
        {
            string query = $"select ID,UpdatedDate,Balance,Delays from Credits where UserID={userID} and Balance<>0";
            try
            {
                SqlCommand command = new SqlCommand(query, connection);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Table table = new Table("ID", "Дата Обновления", "Остаток", "Задержки");
                    while (reader.Read())
                        table.AddRaw(InputHelper.ParamsToString(reader, 1));
                    table.ShowTable();
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Exception : {ex.Message}");
                return false;
            }
        }
        public override bool ThirdAction()
        {
            System.Console.WriteLine("Введите ID кредита, график погашения которого хотите получить : ");
            SecondAction();
            InputHelper.GetInput(out creditID);
            if (!CheckID(creditID, "Credits", $"UserID={userID} and Balance<>0"))
                return false;
            string query = $"select AppID from Credits where ID={creditID}";
            try
            {
                SqlCommand command = new SqlCommand(query, connection);
                int AppId = (int)command.ExecuteScalar();
                command.CommandText = $"select Summ,Date,Period from CreditApps where ID={AppId}";
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    decimal summ = (decimal)reader[0];
                    DateTime date = (DateTime)reader[1];
                    int period = (int)reader[2];
                    CreditGraph(summ, date, period);
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Exception : {ex.Message}");
                return false;
            }
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
                sum += ++maxLRaw[i];
            return sum;
        }
    }
}
