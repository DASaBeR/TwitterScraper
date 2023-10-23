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
		public static IWebDriver InitDriver(bool headless, string env = "chrome")
		{
			IWebDriver driver;
			ChromeOptions chromeOptions = new ChromeOptions();
			FirefoxOptions firefoxOptions = new FirefoxOptions();
			// Set options based on headless and other requirements

			if (headless)
			{
				chromeOptions.AddArgument("--headless");
				firefoxOptions.AddArgument("--headless");
			}

			// Set other options as needed

			if (env.Equals("chrome", StringComparison.OrdinalIgnoreCase))
			{
				driver = new ChromeDriver(chromeOptions);
			}
			else
			{
				driver = new FirefoxDriver(firefoxOptions);
			}

			driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(100);
			return driver;
		}

		public async static Task LogIn(IWebDriver driver, string env, int timeout = 20, int wait = 5)
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
			passwordElement.SendKeys(password);
			passwordElement.SendKeys(Keys.Return);
			Task.Delay(TimeSpan.FromSeconds(wait)).Wait();

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
			driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);

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

				// Navigate to the user's followers or following page
				driver.Navigate().GoToUrl($"https://twitter.com/{user}/{followType}");
				Thread.Sleep(10000); // Wait for page to load

				// Scroll to load all followers/following
				bool scrolling = true;
				IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
				List<string> follows = new List<string>();

				while (scrolling)
				{
					var primaryColumn = driver.FindElement(By.XPath("//div[contains(@data-testid,\"primaryColumn\")]"));
					//extract only the Usercell
					var page_cards = primaryColumn.FindElements(By.XPath("//div[contains(@data-testid,\"UserCell\")]"));

					// Extract followers/following
					var elements1 = driver.FindElements(By.XPath("//div[contains(@data-testid,'UserCell')]"));
					foreach (var card in page_cards)
					{
						IWebElement element = card.FindElement(By.XPath(".//div[1]/div[1]/div[1]//a[1]"));

						string link = element.GetAttribute("href");
						// Extract username from the link
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
