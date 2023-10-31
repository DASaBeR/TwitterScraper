using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium;
using OpenQA.Selenium.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Support.UI;
using Newtonsoft.Json;
using TwitterScraper.Models;
using SeleniumExtras.WaitHelpers;

namespace TwitterScraper
{
	public class Utils
	{

    public async static Task LogIn(IWebDriver driver, string env, int timeout = 20, int wait = 8)
    {
      Credentials credentials = LoadCredentials("F:\\Learning\\ASP.Net_Core\\TwitterScraper\\TwitterScraper\\credentials.json");

      string email = credentials.Email;
      string password = credentials.Password;
      string username = credentials.Username;

      driver.Navigate().GoToUrl("https://twitter.com/i/flow/login");
      Task.Delay(TimeSpan.FromSeconds(wait)).Wait();

      WebDriverWait waitDriver = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));

      IWebElement emailElement = waitDriver.Until(ExpectedConditions.ElementExists(By.XPath("//input[@autocomplete='username']")));
      emailElement.SendKeys(email);
      emailElement.SendKeys(Keys.Return);
      Task.Delay(TimeSpan.FromSeconds(wait)).Wait();

      if (CheckElementExists(By.XPath("//input[@data-testid='ocfEnterTextTextInput']"), driver))
      {
        IWebElement usernameElement = waitDriver.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath("//input[@data-testid='ocfEnterTextTextInput']")));
        usernameElement.SendKeys(username);
        usernameElement.SendKeys(Keys.Return);
        Task.Delay(TimeSpan.FromSeconds(wait)).Wait();

      }

      IWebElement passwordElement = waitDriver.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath("//input[@autocomplete='current-password']")));
      //IWebElement passwordElement = waitDriver.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath("//input[@autocomplete='on']")));
      passwordElement.SendKeys(password);
      passwordElement.SendKeys(Keys.Return);
      Task.Delay(TimeSpan.FromSeconds(wait)).Wait();

      if (CheckElementExists(By.XPath("//input[@autocomplete='on']"), driver))
      {
        IWebElement twoFactorElement = waitDriver.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath("//input[@autocomplete='on']")));
        string twoFactorCode = Console.ReadLine();
        twoFactorElement.SendKeys(twoFactorCode);
        twoFactorElement.SendKeys(Keys.Return);
        Task.Delay(TimeSpan.FromSeconds(wait)).Wait();

      }

    }

    public static async Task<Dictionary<string, List<string>>> GetUsersFollow(List<string> users, bool headless, string env, string followType)
    {
      Dictionary<string, List<string>> followsUsers = new Dictionary<string, List<string>>();
      ChromeOptions options = new ChromeOptions();
      if (headless)
      {
        options.AddArgument("--headless");
      }

      IWebDriver driver = new ChromeDriver(options);
      driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);

      foreach (var user in users)
      {
        try
        {
          await LogIn(driver, env);
        }
        catch
        {
          Task.Delay(TimeSpan.FromSeconds(10)).Wait();
          await LogIn(driver, env);

        }


        driver.Navigate().GoToUrl($"https://twitter.com/{user}/{followType}");
        Thread.Sleep(13000); // Wait for page to load


        bool scrolling = true;
        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
        List<string> follows = new List<string>();

        while (scrolling)
        {
          var primaryColumn = driver.FindElement(By.XPath("//div[contains(@data-testid,\"primaryColumn\")]"));

          var page_cards = primaryColumn.FindElements(By.XPath("//div[contains(@data-testid,\"UserCell\")]"));


          var elements1 = driver.FindElements(By.XPath("//div[contains(@data-testid,'UserCell')]"));
          foreach (var card in page_cards)
          {
            IWebElement element = card.FindElement(By.XPath(".//div[1]/div[1]/div[1]//a[1]"));

            string link = element.GetAttribute("href");

            string[] parts = link.Split('/');
            if (parts.Length > 1)
            {
              string username = "@" + parts[parts.Length - 1];
              follows.Add(username);
            }
          }

          followsUsers[user] = follows;

          var previousHeight = js.ExecuteScript("return document.body.scrollHeight");
          js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
          Thread.Sleep(8000); // Wait for new content to load
          var newHeight = js.ExecuteScript("return document.body.scrollHeight");

          if (newHeight.Equals(previousHeight))
          {
            scrolling = false;
          }
        }

      }

      driver.Quit();
      return followsUsers;
    }

    public static async Task<Dictionary<string, List<string>>> GetPostsLikes(List<string> postUrls, bool headless, string env, string engagementType)
    {
      Dictionary<string, List<string>> likedUsers = new Dictionary<string, List<string>>();
      ChromeOptions options = new ChromeOptions();
      if (headless)
      {
        options.AddArgument("--headless");
      }

      IWebDriver driver = new ChromeDriver(options);
      driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
      try
      {
        await LogIn(driver, env);
      }
      catch
      {
        Task.Delay(TimeSpan.FromSeconds(15)).Wait();
        await LogIn(driver, env);

      }
      foreach (var url in postUrls)
      {

        var splitedUrl = url.Split('/');

        driver.Navigate().GoToUrl($"https://twitter.com/{splitedUrl[3]}/status/{splitedUrl[5]}/{engagementType}");
        Thread.Sleep(13000); // Wait for page to load


        bool scrolling = true;
        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
        List<string> follows = new List<string>();

        while (scrolling)
        {
          var primaryColumn = driver.FindElement(By.XPath("//div[contains(@data-testid,\"primaryColumn\")]"));

          var page_cards = primaryColumn.FindElements(By.XPath("//div[contains(@data-testid,\"UserCell\")]"));


          var elements1 = driver.FindElements(By.XPath("//div[contains(@data-testid,'UserCell')]"));
          foreach (var card in page_cards)
          {
            IWebElement element = card.FindElement(By.XPath(".//div[1]/div[1]/div[1]//a[1]"));

            string link = element.GetAttribute("href");
            // username from the link
            string[] parts = link.Split('/');
            if (parts.Length > 1)
            {
              string username = "@" + parts[parts.Length - 1];
              follows.Add(username);
            }
          }

          likedUsers[url] = follows;

          var previousHeight = js.ExecuteScript("return document.body.scrollHeight");
          js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
          Thread.Sleep(8000); // Wait for new content to load
          var newHeight = js.ExecuteScript("return document.body.scrollHeight");

          if (newHeight.Equals(previousHeight))
          {
            scrolling = false;
          }
        }

      }

      driver.Quit();
      return likedUsers;
    }

    public static async Task<Dictionary<string, List<string>>> GetPosts(List<string> users, bool headless, string env)
    {
      Dictionary<string, List<string>> userTweets = new Dictionary<string, List<string>>();
      ChromeOptions options = new ChromeOptions();
      if (headless)
      {
        options.AddArgument("--headless");
      }

      IWebDriver driver = new ChromeDriver(options);
      driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
      try
      {
        await LogIn(driver, env);
      }
      catch
      {
        Task.Delay(TimeSpan.FromSeconds(15)).Wait();
        await LogIn(driver, env);

      }
      foreach (var user in users)
      {

        // Navigate to the user's followers or following page
        driver.Navigate().GoToUrl($"https://twitter.com/{user}");
        Thread.Sleep(13000); // Wait for page to load

        // Scroll to load all followers/following
        bool scrolling = true;
        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
        List<string> tweets = new List<string>();

        while (scrolling)
        {
          var primaryColumn = driver.FindElement(By.XPath("//div[contains(@data-testid,\"primaryColumn\")]"));

          //var page_cards = driver.FindElements(By.XPath("//article[contains(@data-testid,\"tweet\")]"));
          //var page_cards = driver.FindElements(By.XPath("//div[contains(@data-testid,\"cellInnerDiv\")]"));
          //var page_cards = driver.FindElements(By.XPath("//div[contains(@data-testid,\"cellInnerDiv\")]//div[1]/div[1]/article[1]//div[contains(@data-testid,\"tweetText\")]"));
          var page_cards = driver.FindElements(By.XPath("//div[contains(@data-testid,\"cellInnerDiv\")]//div[1]/div[1]/article[1][not(.//span[@data-testid=\"socialContext\"])][not(.//span[normalize-space(text())=\"Ad\"])]//div[contains(@data-testid,\"tweetText\")]"));
          //var page_cards = driver.FindElements(By.TagName("article"));
          foreach (var element in page_cards)
          {
            //var test = card.GetAttribute("innerHTML");
            //IWebElement element = card.FindElement(By.XPath(".//div[1]/div[1]/div[1]//article[1]"));
            //IWebElement element = card.FindElement(By.XPath("//div[contains(@data-testid,\"tweetText\")]"));

            string tweetHtml = element.GetAttribute("innerHTML");

            tweets.Add(tweetHtml);
          }

          userTweets[user] = tweets;

          var previousHeight = js.ExecuteScript("return document.body.scrollHeight");
          js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
          Thread.Sleep(8000); // Wait for new content to load
          var newHeight = js.ExecuteScript("return document.body.scrollHeight");

          if (newHeight.Equals(previousHeight))
          {
            scrolling = false;
          }
        }

      }

      driver.Quit();
      return userTweets;
    }

    public static async Task<Dictionary<string, List<string>>> GetReposts(List<string> users, bool headless, string env)
    {
      Dictionary<string, List<string>> userTweets = new Dictionary<string, List<string>>();
      ChromeOptions options = new ChromeOptions();
      if (headless)
      {
        options.AddArgument("--headless");
      }

      IWebDriver driver = new ChromeDriver(options);
      driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
      try
      {
        await LogIn(driver, env);
      }
      catch
      {
        Task.Delay(TimeSpan.FromSeconds(15)).Wait();
        await LogIn(driver, env);

      }
      foreach (var user in users)
      {

        driver.Navigate().GoToUrl($"https://twitter.com/{user}");
        Thread.Sleep(13000); // Wait for page to load

        bool scrolling = true;
        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
        List<string> tweets = new List<string>();

        while (scrolling)
        {
          //var primaryColumn = driver.FindElement(By.XPath("//div[contains(@data-testid,\"primaryColumn\")]"));

          //var page_cards = driver.FindElements(By.XPath("//article[contains(@data-testid,\"tweet\")]"));
          //var page_cards = driver.FindElements(By.XPath("//div[contains(@data-testid,\"cellInnerDiv\")]"));
          //var page_cards = driver.FindElements(By.XPath("//div[contains(@data-testid,\"cellInnerDiv\")]//div[1]/div[1]/article[1]//div[contains(@data-testid,\"tweetText\")]"));
          var page_cards = driver.FindElements(By.XPath("//div[contains(@data-testid,\"cellInnerDiv\")]//div[1]/div[1]/article[1][.//span[@data-testid=\"socialContext\"]][not(.//span[normalize-space(text())=\"Ad\"])]//div[contains(@data-testid,\"tweetText\")]"));
          //var page_cards = driver.FindElements(By.TagName("article"));
          foreach (var element in page_cards)
          {
            //var test = card.GetAttribute("innerHTML");
            //IWebElement element = card.FindElement(By.XPath(".//div[1]/div[1]/div[1]//article[1]"));
            //IWebElement element = card.FindElement(By.XPath("//div[contains(@data-testid,\"tweetText\")]"));

            string tweetHtml = element.GetAttribute("innerHTML");

            tweets.Add(tweetHtml);
          }

          userTweets[user] = tweets;

          var previousHeight = js.ExecuteScript("return document.body.scrollHeight");
          js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
          Thread.Sleep(8000); // Wait for new content to load
          var newHeight = js.ExecuteScript("return document.body.scrollHeight");

          if (newHeight.Equals(previousHeight))
          {
            scrolling = false;
          }
        }

      }

      driver.Quit();
      return userTweets;
    }

    public static async Task<Dictionary<string, List<string>>> GetReplys(List<string> urls, bool headless, string env)
    {
      Dictionary<string, List<string>> userTweets = new Dictionary<string, List<string>>();
      ChromeOptions options = new ChromeOptions();
      if (headless)
      {
        options.AddArgument("--headless");
      }

      IWebDriver driver = new ChromeDriver(options);
      driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
      try
      {
        await LogIn(driver, env);
      }
      catch
      {
        Task.Delay(TimeSpan.FromSeconds(15)).Wait();
        await LogIn(driver, env);

      }
      foreach (var url in urls)
      {

        driver.Navigate().GoToUrl($"{url}");
        Thread.Sleep(13000); // Wait for page to load

        bool scrolling = true;
        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
        List<string> tweets = new List<string>();

        while (scrolling)
        {
          //var primaryColumn = driver.FindElement(By.XPath("//div[contains(@data-testid,\"primaryColumn\")]"));

          var page_cards = driver.FindElements(By.XPath("//div[contains(@data-testid,\"cellInnerDiv\")][position() > 1]//div[1]/div[1]/article[1][not(.//span[normalize-space(text())=\"Ad\"])]//div[contains(@data-testid,\"tweetText\")]"));
          foreach (var element in page_cards)
          {

            string tweetHtml = element.GetAttribute("innerHTML");

            tweets.Add(tweetHtml);
          }

          userTweets[url] = tweets;

          var previousHeight = js.ExecuteScript("return document.body.scrollHeight");
          js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
          Thread.Sleep(8000); // Wait for new content to load
          var newHeight = js.ExecuteScript("return document.body.scrollHeight");

          if (newHeight.Equals(previousHeight))
          {
            scrolling = false;
          }
        }

      }

      driver.Quit();
      return userTweets;
    }

    public static bool CheckElementExists(By by, IWebDriver driver)
    {
      try
      {
        driver.FindElement(by);
        return true;
      }
      catch (NoSuchElementException)
      {
        return false;
      }
    }
    public static Credentials LoadCredentials(string filePath)
    {
      try
      {
        string json = File.ReadAllText(filePath);
        Credentials credentials = JsonConvert.DeserializeObject<Credentials>(json);
        return credentials;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error: {ex.Message}");
        return null;
      }
    }
  }
}
