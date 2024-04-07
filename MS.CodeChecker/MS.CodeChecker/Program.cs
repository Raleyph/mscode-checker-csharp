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
            CheckCodes();
            Console.WriteLine("Програма успешно завершила работу");
        }
        catch (Exception error) when (error is NullReferenceException or NoSuchWindowException or NoSuchFrameException or NoSuchElementException)
        {
            Console.WriteLine(error);
            Environment.Exit(-1);
        }
        
        Console.ReadLine();
    }

    private static void CheckCodes()
    {
        bool isReady = false;

        while (!isReady)
        {
            Driver driver = new Driver();
            List<string> codes = FileManager.GetSourceCodes().Except(FileManager.GetProcessedCodes()).ToList();

            foreach (string code in codes)
            {
                try
                {
                    bool isCodeValid = driver.IsCodeValid(code);
                    FileManager.WriteValidCode(code, isCodeValid ? null : CodeStatus.Used);
                    Thread.Sleep(5000);
                }
                catch (MicrosoftUnexpectedCodeError)
                {
                    driver.Close();
                    Thread.Sleep(90000);
                    break;
                }

                if (codes.IndexOf(code) != codes.Count - 1)
                    continue;
                
                driver.Close();
                isReady = true;
            }
        }
    }
}
