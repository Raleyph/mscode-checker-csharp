using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using RandomUserAgent;

namespace MS.CodeChecker.Models;

public enum CodeStatus {
    Used
}

public class Driver
{
    private const string TargetLink = "https://account.microsoft.com/billing/redeem?refd=account.microsoft.com";
    private const string IframeId = "redeem-iframe";
    private const string InputSelector = "#store-cart-root > div > div > div > div.content--3nYMiOWt > input";
    private const string CodeErrorClassname = "errorMessageText--0VFASJvm";

    private readonly ChromeDriver _driver;
    private readonly WebDriverWait _driverWait;

    public Driver()
    {
        ChromeOptions options = new ChromeOptions();
        
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddArgument("--start-maximized");
        options.AddArgument($"--user-agent={RandomUa.RandomUserAgent}");

        _driver = new ChromeDriver(options: options, chromeDriverDirectory: "driver/chromedriver.exe");
        
        _driverWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(120));
        _driver.Navigate().GoToUrl(TargetLink);
    }

    private void Login()
    {
        while (!_driver.Url.Contains(TargetLink)) { }

        IWebElement frame = _driverWait.Until(ExpectedConditions.ElementExists(By.Id(IframeId)));

        if (frame == null) throw new NoSuchFrameException("ОШИБКА! Iframe не был найден.");

        _driver.SwitchTo().Frame(frame);

        try
        {
            _driverWait.Until(ExpectedConditions.ElementExists(By.CssSelector(InputSelector)));
        }
        catch
        {
            throw new NoSuchElementException("ОШИБКА! Поле для ввода ключей не было найдено.");
        }
        
        Thread.Sleep(2000);
    }

    private bool IsCodeValid(IWebElement codeInput, string code)
    {
        codeInput.Clear();
        codeInput.SendKeys(code);

        try
        {
            new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
                .Until(ExpectedConditions.ElementExists(By.ClassName(CodeErrorClassname)));
        }
        catch
        {
            return true;
        }

        return false;
    }

    public void Start()
    {
        Login();

        IWebElement codeInput = _driver.FindElement(By.CssSelector(InputSelector));
        
        List<string> sourceCodes = FileManager.GetSourceCodes();
        IEnumerable<string> codes = sourceCodes.Except(FileManager.GetProcessedCodes());
        
        foreach (string code in codes)
        {
            FileManager.WriteValidCode(
                code,
                IsCodeValid(codeInput, code) ? null : CodeStatus.Used
                );
            
            Thread.Sleep(8000);
        }
        
        _driver.Close();
    }
}
