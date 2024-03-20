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
        
        new Driver().Start();
        Console.WriteLine("Програма успешно завершила работу");
        
        try
        {
            new Driver().Start();
            Console.WriteLine("Програма успешно завершила работу");
        }
        catch (Exception error) when (
            error is NullReferenceException
                or NoSuchWindowException
                or NoSuchFrameException
                or NoSuchElementException
            )
        {
            Console.WriteLine(error);
            Environment.Exit(-1);
        }
        
        Console.ReadLine();
    }
}
