using System;
using System.Threading.Tasks;

namespace GSTValidator {
	internal class Program {
		private static readonly Client Client = new Client();
		private static readonly Logger Logger = new Logger("CORE");

		private static async Task Main(string[] args) {
			Console.ForegroundColor = ConsoleColor.White;
			Console.Title = "GST Validator by SynergY";			
			await Client.InitAsync().ConfigureAwait(false);
			Logger.Info("Press any key to exit...");
			Console.ReadKey(true);
		}
	}
}
