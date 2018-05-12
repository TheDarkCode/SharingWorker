using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharingWorker.FileHost
{
	interface IFileHost
	{
		string Name { get; }
		string User { get; set; }
		string Password { get; set; }
		bool Enabled { get; set; }
		bool LoggedIn { get; set; }

		bool LoadConfig();
		Task LogIn();
		Task<List<string>> GetLinks(string filename, bool withFileName = false);
	}
}
