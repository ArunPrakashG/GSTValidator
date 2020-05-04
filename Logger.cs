using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GSTValidator {
	internal class Logger {
		private readonly string Identifier;
		private bool _debugEnabled;
		private const string LOG_FILE = "log.txt";
		private static readonly SemaphoreSlim FileSemaphore = new SemaphoreSlim(1, 1);

		internal bool Debug {
			get => _debugEnabled;
			set {
				_debugEnabled = value;

				if (_debugEnabled) {
					Console.ForegroundColor = ConsoleColor.DarkBlue;
					Console.WriteLine("DEBUG mode is enabled!");					
				}
				else {
					Console.ForegroundColor = ConsoleColor.DarkBlue;
					Console.WriteLine("DEBUG mode is disabled.");
				}

				Console.ResetColor();
			}
		}

		internal Logger(string? _identifier = null) => Identifier = _identifier ?? "NA";

		internal void Info(string? msg, [CallerMemberName] string? methodName = null, [CallerLineNumber] int lineNumber = 0, bool skipToFile = false) {
			if (string.IsNullOrEmpty(msg)) {
				return;
			}

			Console.WriteLine($"{DateTime.Now.ToString()} [ {Identifier} ] {msg}");

			if (!skipToFile) {
				ToFile($"{methodName}() | ln {lineNumber} | INFO | {DateTime.Now.ToString()} [ {Identifier} ] {msg}");
			}			
		}

		internal void Trace(string? msg, [CallerMemberName] string? methodName = null, [CallerLineNumber] int lineNumber = 0) {
			if (string.IsNullOrEmpty(msg)) {
				return;
			}

			if (Debug) {
				Info($"DEBUG | {msg}", methodName, lineNumber, true);
			}

			ToFile($"{methodName}() | ln {lineNumber} | TRACE | {DateTime.Now.ToString()} [ {Identifier} ] {msg}");
		}

		internal void Error(string? msg, [CallerMemberName] string? methodName = null, [CallerLineNumber] int lineNumber = 0) {
			if (string.IsNullOrEmpty(msg)) {
				return;
			}

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"{DateTime.Now.ToString()} [ {Identifier} ] {msg}");
			Console.ResetColor();
			ToFile($"{methodName}() | ln {lineNumber} | ERROR | {DateTime.Now.ToString()} [ {Identifier} ] {msg}");
		}

		internal void Exception(Exception? e, [CallerMemberName] string? methodName = null, [CallerLineNumber] int lineNumber = 0) {
			if (e == null) {
				return;
			}

			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine($"{DateTime.Now.ToString()} [ {Identifier} ] {e.Message}");
			Console.ResetColor();
			ToFile($"{methodName}() | ln {lineNumber} | ERROR | {DateTime.Now.ToString()} [ {Identifier} ] {e.Message} \n {e.StackTrace}");
		}

		private void ToFile(string? msg) {
			if (string.IsNullOrEmpty(msg)) {
				return;
			}

			FileSemaphore.Wait();

			try {
				using (StreamWriter writer = new StreamWriter(LOG_FILE, true)) {
					writer.WriteLine(msg);
					writer.Flush();
				}
			}
			catch (Exception) { }
			finally {
				FileSemaphore.Release();
			}
		}
	}
}
