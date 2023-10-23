using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitterScraper.Models;
using System.Text.Json;

namespace TwitterScraper
{
	public class User
	{
		public static Dictionary<string, List<string>> GetUsersInformation(List<string> users, bool headless = true)
		{
			Dictionary<string, List<string>> usersInfo = new Dictionary<string, List<string>>();
			IWebDriver driver = new ChromeDriver(); // Assuming ChromeDriver is set up properly

			foreach (var user in users)
			{
				LogUserPage(user, driver);

				try
				{
					string following = driver.FindElement(By.XPath("//a[contains(@href,'/following')]/span[1]/span[1]")).Text;
					string followers = driver.FindElement(By.XPath("//a[contains(@href,'/followers')]/span[1]/span[1]")).Text;

					string joinDate = driver.FindElement(By.XPath("//div[contains(@data-testid,'UserProfileHeader_Items')]/span[3]")).Text;
					string birthday = driver.FindElement(By.XPath("//div[contains(@data-testid,'UserProfileHeader_Items')]/span[2]")).Text;
					string location = driver.FindElement(By.XPath("//div[contains(@data-testid,'UserProfileHeader_Items')]/span[1]")).Text;

					string website = "";
					try
					{
						IWebElement element = driver.FindElement(By.XPath("//div[contains(@data-testid,'UserProfileHeader_Items')]//a[1]"));
						website = element.GetAttribute("href");
					}
					catch (NoSuchElementException)
					{
						// Handle exception if website element is not found
					}

					string description = "";
					try
					{
						description = driver.FindElement(By.XPath("//div[contains(@data-testid,'UserDescription')]")).Text;
					}
					catch (NoSuchElementException)
					{
						// Handle exception if description element is not found
					}

					List<string> userInfo = new List<string>
								{
										following, followers, joinDate, birthday, location, website, description
								};

					usersInfo[user] = userInfo;
				}
				catch (NoSuchElementException)
				{
					// Handle exception if elements are not found
				}
			}

			driver.Quit();
			return usersInfo;
		}

		public static void LogUserPage(string user, IWebDriver driver)
		{
			// Add sleep or wait logic here if needed
			driver.Navigate().GoToUrl($"https://twitter.com/{user}");
			// Add more logic if needed after loading the user page
		}

		public static async Task<Dictionary<string, List<string>>> GetUsersFollowers(List<string> users, string env, /*bool verbose = true, */bool headless = true,/* int wait = 2, int limit = int.MaxValue,*/ string filePath = null)
		{
			Dictionary<string, List<string>> followers = await TwitterScraper.Utils.GetUsersFollow(users, headless, env, "followers"/*, verbose, wait, limit*/);
			
			followers = followers.ToDictionary(x => x.Key, x => x.Value.Distinct().ToList());

			if (string.IsNullOrEmpty(filePath))
			{
				filePath = $"outputs/{users[0]}_{users[^1]}_followers.json";
			}
			else
			{
				filePath += $"{users[0]}_{users[^1]}_followers.json";
			}

			SaveToJsonFile(followers, filePath);
			Console.WriteLine($"File saved in {filePath}");
			return followers;
		}

		public static async Task<Dictionary<string, List<string>>> GetUsersFollowing(List<string> users, string env, bool verbose = true, bool headless = true, int wait = 2, int limit = int.MaxValue, string filePath = null)
		{
			Dictionary<string, List<string>> following = await Utils.GetUsersFollow(users, headless, env, "following"/*, verbose, wait, limit*/);

			if (string.IsNullOrEmpty(filePath))
			{
				filePath = $"outputs/{users[0]}_{users[^1]}_following.json";
			}
			else
			{
				filePath += $"{users[0]}_{users[^1]}_following.json";
			}

			SaveToJsonFile(following, filePath);
			Console.WriteLine($"File saved in {filePath}");
			return following;
		}

		public static void SaveToJsonFile<T>(T data, string filePath)
		{
			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
			};

			string jsonString = JsonSerializer.Serialize(data, options);
			File.WriteAllText(filePath, jsonString);
		}

	}
}
