using System;
using Microsoft.Data.SqlClient;

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
            }
        }
    }
    abstract class CreditApp
    {
        protected SqlConnection connection;
        public void CreditGraph(decimal summ, DateTime date, int month)
        {
            decimal creditBody = summ / month;
            System.Console.WriteLine("Дата\tПлатеж\tПроценты\tТело Кредита\tОстаток");
        }
        public string GetInput()
        {
            System.Console.Write("Ввод : ");
            return Console.ReadLine();
        }
        public int GetCommand(int max)
        {
            int res = int.Parse(GetInput());
            while (res > max || res < 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("Не правильно введенная команда, выберайте из существующих вариантов");
                Console.ResetColor();
                res = int.Parse(GetInput());
            }
            return res;
        }
        public bool Login()
        {
            System.Console.Write("Введите логин(номер телефона) : ");
            string login = Console.ReadLine();
            System.Console.Write("Введите пароль : ");
            string password = Console.ReadLine();
            string userType = this is AdminApp ? "admin" : "client";
            string query = $"select count(*) from Users where Type={userType},Phone={login},Password{password}";
            SqlCommand command = new SqlCommand(query, connection);
            try { return ((int)command.ExecuteScalar()) > 0; }
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
        public AdminApp(SqlConnection connection) => this.connection = connection;
        public override bool FirstAction()
        {
            /* System.Console.Write("Выберите тип пользователя\n(1)-Admin\n(2)Client");
            string userType = GetCommand(2) switch
            {

            }; */
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
    class ClientApp : CreditApp
    {
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
}
