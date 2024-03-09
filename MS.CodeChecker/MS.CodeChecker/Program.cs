using MS.CodeChecker.Models;
using OpenQA.Selenium;

namespace MS.CodeChecker;

public static class Program
{
    public static void Main()
    {
        try
        {
            FileManager.CheckEnvironment();
        }
        catch (Exception e) when (e is ArgumentException or FileNotFoundException)
        {
            Console.WriteLine(e.Message);
            Console.ReadLine();
        }

        try
        {
            bool result = new Driver().Start();

            Console.WriteLine(
                result
                    ? "Програма успешно завершила работу"
                    : "Программа звершила работу с ошибкой авторизации! Скорее всего, " +
                      "превышено время ожидания. Попробуйте запустить проверку заново."
            );
        }
        catch (Exception error) when (error is NullReferenceException or NoSuchWindowException)
        {
            Environment.Exit(-1);
        }
        
        Console.ReadLine();
    }
}
