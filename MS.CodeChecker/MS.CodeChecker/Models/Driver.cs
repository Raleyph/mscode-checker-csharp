using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace MS.CodeChecker.Models;

public enum CodeStatus { Used }

public class Driver
{
    private const string TargetLink = "https://account.microsoft.com/billing/redeem?refd=account.microsoft.com";

    private readonly string[] _credentialsInputSelectors = { "#i0116", "#i0118" };
    private const string CredentialsAcceptSelector = "#idSIButton9";
    private const string StayLoggedSelector = "#acceptButton";
    
    private const string IframeId = "redeem-iframe";
    private const string CodeInputSelector = "#store-cart-root > div > div > div > div.content--3nYMiOWt > input";
    private const string CodeErrorClassname = "errorMessageText--0VFASJvm";

    private readonly ChromeDriver _driver;
    
    private readonly WebDriverWait _redirectDriverWait;
    private readonly WebDriverWait _elementDriverWait;

    private IWebElement? _codeInput;

    public Driver()
    {
        ChromeOptions options = new ChromeOptions();
        
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddArgument("--start-maximized");

        _driver = new ChromeDriver(options: options, chromeDriverDirectory: "driver/chromedriver.exe");
        
        _redirectDriverWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(120));
        _elementDriverWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

        Init();
    }

    private IWebElement FindExistElement(By selector)
        => _elementDriverWait.Until(ExpectedConditions.ElementExists(selector));
    
    private IWebElement FindInteractableElement(By selector)
        => _elementDriverWait.Until(ExpectedConditions.ElementToBeClickable(selector));
    
    private void Init()
    {
        Login();
        _codeInput = GetCodeInput();
    }

    private void Login()
    {
        _driver.Navigate().GoToUrl(TargetLink);

        string[] credentials = FileManager.GetCredentials();

        for (int i = 0; i < _credentialsInputSelectors.Length; i++)
        {
            FindInteractableElement(By.CssSelector(_credentialsInputSelectors[i])).SendKeys(credentials[i]);
            FindInteractableElement(By.CssSelector(CredentialsAcceptSelector)).Click();
        }
        
        FindInteractableElement(By.CssSelector(StayLoggedSelector)).Click();
    }

    private IWebElement GetCodeInput()
    {
        IWebElement frame = _redirectDriverWait.Until(ExpectedConditions.ElementExists(By.Id(IframeId)));

        if (frame == null)
            throw new NoSuchFrameException("ОШИБКА! Iframe не был найден.");

        _driver.SwitchTo().Frame(frame);

        try
        {
            return FindExistElement(By.CssSelector(CodeInputSelector));
        }
        catch
        {
            throw new NoSuchElementException("ОШИБКА! Поле для ввода ключей не было найдено.");
        }
    }

    public bool IsCodeValid(string code)
    {
        _codeInput?.Clear();
        _codeInput?.SendKeys(code);

        IWebElement errorElement;

        try
        {
            errorElement = FindExistElement(By.ClassName(CodeErrorClassname));
        }
        catch
        {
            return true;
        }
        
        ReadOnlyCollection<IWebElement> errorSpan = errorElement.FindElements(By.XPath("*"));

        string errorText = errorSpan[0].Text;

        if (errorText.Contains("unexpected") || errorText.Contains("помилка"))
            throw new MicrosoftUnexpectedCodeError();
        
        return false;
    }
    
    public void Close() => _driver.Close();
}

public class MicrosoftUnexpectedCodeError : Exception { }
