using System;
using System.Threading;
using System.Threading.Tasks;

namespace GSTValidator {
	public static class Helpers {
		public static void InBackground(Action action, bool longRunning = false) {
			if (action == null) {
				return;
			}

			TaskCreationOptions options = TaskCreationOptions.DenyChildAttach;

			if (longRunning) {
				options |= TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness;
			}

			Task.Factory.StartNew(action, CancellationToken.None, options, TaskScheduler.Default);
		}
	}
}
