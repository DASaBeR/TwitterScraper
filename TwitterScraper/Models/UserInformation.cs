using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterScraper.Models
{
	public class UserInformation
	{
		public string Following { get; set; }
		public string Followers { get; set; }
		public string JoinDate { get; set; }
		public string BirthDate { get; set; }
		public string Location { get; set; }
		public string Website { get; set; }
		public string Description { get; set; }
	}
}
