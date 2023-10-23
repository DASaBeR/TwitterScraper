using System.IO;

namespace TwitterScraper
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			var users = new List<string> { "nagouzil" };
			var followers = await User.GetUsersFollowers(users, "credentials.json", true, "F:\\Learning\\ASP.Net_Core\\TwitterScraper\\TwitterScraper\\output\\followers.json");
			foreach (var item in followers)
			{
				Console.WriteLine(item.Key + "    ::   " + item.Value);
			}
		}
	}
}