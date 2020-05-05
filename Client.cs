using GSTValidator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static GSTValidator.Enums;

namespace GSTValidator {
	internal class Client {
		private const int DELAY_BETWEEN_REQUESTS = 4; // in seconds
		private const string CSV_PATH = "CSVOutput.csv";
		private const int MAX_RETRY_COUNT = 3;
		private const string ALL_SEARCH_TYPE = "A";
		private const string REQUEST_BASE_URL = "https://services.gst.gov.in/";
		private const string GST_URL = REQUEST_BASE_URL + "services/api/search/gstp";

		private static readonly HttpClient HttpClient = new HttpClient();
		private static readonly SemaphoreSlim RequestSync = new SemaphoreSlim(1, 1);
		private readonly Logger Logger = new Logger("CLIENT");

		internal readonly List<State> States = new List<State>();

		private string GenerateStateUrl(StateCodes stateCode) => $"{REQUEST_BASE_URL}master/st/{(int) stateCode:D2}/district";

		internal async Task InitAsync() {
			States.Clear();
			StateCodes stateCode = StateCodes.AndamanNicobarIslands;
			string districtCode = null;
			string pinCode = null;
			string parctitionerName = null;
			State state = null;

			Out("------------------- Search Configuration -------------------");

			for (int i = 0; i < 3; i++) {
				Out("Enter the state code: ");
				Out("(Enter the number by the left side of each state)", ConsoleColor.White);
				Out("Example: ' 02 ' (without quotes) which stands for 'HimachalPradesh'", ConsoleColor.White);

				foreach (StateCodes code in Enum.GetValues(typeof(StateCodes))) {
					Out($"| {(int) code:D2}  |  {code.ToString()}", ConsoleColor.White);
				}

				string stateCodeString = In();

				if (string.IsNullOrEmpty(stateCodeString) || stateCodeString.Where(x => !char.IsDigit(x)).Count() > 0) {
					Out("Entered value is invalid.");
				}

				stateCode = (StateCodes) Enum.Parse(typeof(StateCodes), stateCodeString);
				Out("Please wait... getting districts of the specified state...");
				state = await CacheStateAsync(stateCode).ConfigureAwait(false);

				if (state == null) {
					Out("Failed to get the districts. Please reenter the state code.");
					continue;
				}

				break;
			}

			Out("Enter district numerical code: ");
			Out("(the code on left side of the district name)", ConsoleColor.White);
			Out("(leave empty to display all practitioners in the state)", ConsoleColor.White);

			foreach (StateResponse.Districts district in state.Districts) {
				Out($"| {district.DistrictNumericalCode}  |  {district.DistrictName}", ConsoleColor.White);
			}

			districtCode = In();

			Out("Enter pin code of the location of which you want to display practitioners of: ");
			Out("(leave empty if you don't want to filter with pin code)", ConsoleColor.White);
			pinCode = In();

			Out("Enter the practitioner name: ");
			parctitionerName = InWithMessage("(Leave Empty to display all the practitioners in the location)", ConsoleColor.White);

			Out("------------------- Configuration Success -------------------");

			Logger.Info("Please wait while we fetch the required data from 'https://services.gst.gov.in/' ...");

			List<GstResponse.Practitioner> result = await RequestGstPractitionersAsync(parctitionerName, stateCode, districtCode, pinCode).ConfigureAwait(false);

			if (result != null) {
				Logger.Info("Successfully received the data!");

				IEnumerable<GstResponse.Practitioner> parti = result.Where(x => (StateCodes) Enum.Parse(typeof(StateCodes), x.StateCode) == stateCode);

				Logger.Info($"Found a total of '{parti.Count()}' practitioners from '{stateCode.ToString()}' state.");

				foreach (GstResponse.Practitioner p in parti) {
					Out($"{p.EnrolmentNumber} | {p.PractitionerName}", ConsoleColor.White);
				}

				Logger.Info("Shall we save the received data in CSV file ? (y/n)");

				if (InBoolean(ConsoleKey.Y)) {
					StringBuilder lines = new StringBuilder();

					foreach (GstResponse.Practitioner p in parti) {
						lines.AppendLine($"{p.EnrolmentNumber},{p.PractitionerName},{p.EmailID},{p.ContactNumber},{p.Address.BuildingNumber} {p.Address.BuildName} {p.Address.FlatNumber} {p.Address.Road} {p.Address.PinCode},{p.StateCode},{p.DistrictCode}");
					}

					ToFile(lines.ToString());
				}

				Logger.Info("All operations completed!");
				return;
			}

			Logger.Error("Failed to complete operations.");
			Logger.Error("Please restart the application.");
		}

		private void ToFile(string data) {
			using (FileStream stream = new FileStream(CSV_PATH, FileMode.OpenOrCreate)) {
				using (StreamWriter writer = new StreamWriter(stream)) {
					writer.Write(data);
					writer.Flush();
					Logger.Info($"All data successfully written to '{CSV_PATH}' file!");
				}
			}
		}

		private bool InBoolean(ConsoleKey key) => Console.ReadKey(true).Key == key;

		private void Out(string message, ConsoleColor color = ConsoleColor.Green) {
			if (string.IsNullOrEmpty(message)) {
				return;
			}

			Console.ForegroundColor = color;
			Console.WriteLine("> " + message);
			Logger.Trace("> " + message);
			Console.ResetColor();
		}

		private string InWithMessage(string message, ConsoleColor color = ConsoleColor.Green) {
			if (string.IsNullOrEmpty(message)) {
				return default;
			}

			Console.ForegroundColor = color;
			Console.WriteLine("> " + message);
			Console.ResetColor();
			return In();
		}

		private string In() => Console.ReadLine();

		private async Task<List<GstResponse.Practitioner>> RequestGstPractitionersAsync(string parctitionerName, StateCodes stateCode, string districtCode, string pinCode = null, string searchType = ALL_SEARCH_TYPE) {
			if (States.Count <= 0) {
				Logger.Error("State codes isn't loaded correctly... Please restart the application!");
				return null;
			}

			State state = States.FirstOrDefault(x => x.StateCode == stateCode);

			if (state == null) {
				Logger.Error("No such state exist in the cached list. We will attempt to reload the list...");
				await CacheStateAsync(stateCode).ConfigureAwait(false);
				state = States.FirstOrDefault(x => x.StateCode == stateCode);
			}

			if (state.DistrictsCount <= 0) {
				Logger.Error($"The specified state '{state.StateCode.ToString()}' doesn't have any districts.");
				return null;
			}

			GstRequest gstRequest = new GstRequest(searchType, parctitionerName, stateCode, districtCode, pinCode);
			return await PostAsJsonObject<List<GstResponse.Practitioner>, GstRequest>(gstRequest, null, GST_URL, 3).ConfigureAwait(false);
		}

		private async Task<State> CacheStateAsync(StateCodes _state) {
			Logger.Info($"Caching data for '{_state.ToString()}' state...");
			string requestUrl = GenerateStateUrl(_state);

			Dictionary<string, string> headers = new Dictionary<string, string>() {
					{"Referer", "https://services.gst.gov.in/services/locategstp" }
			};

			StateResponse response = await GetAsJsonObject<StateResponse>(requestUrl, headers).ConfigureAwait(false);

			if (response == null || response.StateCollection == null || response.StateCollection.Length <= 0 || response.StateCollection.FirstOrDefault().DistrictCollection.Length <= 0) {
				return null;
			}

			State state = new State(_state, response.StateCollection.FirstOrDefault().DistrictCollection.ToList());
			States.Add(state);
			Logger.Info($"Loaded '{(int) _state:D2}:{_state.ToString()}' state with '{state.DistrictsCount}' districts.");
			return state;
		}

		private async Task<bool> CacheStatesAsync() {
			Logger.Info($"Caching '{Enum.GetValues(typeof(StateCodes)).Length}' states...");

			foreach (StateCodes code in Enum.GetValues(typeof(StateCodes))) {
				await CacheStateAsync(code).ConfigureAwait(false);
			}

			return States.Count == Enum.GetValues(typeof(StateCodes)).Length;
		}

		private async Task<TResponseType> GetAsJsonObject<TResponseType>(string requestUrl, Dictionary<string, string> headers, int maxTries = MAX_RETRY_COUNT) {
			if (string.IsNullOrEmpty(requestUrl)) {
				return default;
			}

			for (int i = 0; i < maxTries; i++) {
				try {
					using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl)) {
						if (headers != null && headers.Count > 0) {
							foreach (KeyValuePair<string, string> pair in headers) {
								request.Headers.Add(pair.Key, pair.Value);
							}
						}

						using (HttpResponseMessage response = await ExecuteRequest(async () => await HttpClient.SendAsync(request).ConfigureAwait(false))) {
							if (!response.IsSuccessStatusCode) {
								Logger.Error($"Request failed. Retrying... ({i})");
								continue;
							}

							string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

							if (string.IsNullOrEmpty(responseContent)) {
								return default;
							}

							TResponseType result = default;
							try {
								result = JsonConvert.DeserializeObject<TResponseType>(responseContent);
							}
							catch {
								GenericErrorResponse errorResponse = JsonConvert.DeserializeObject<GenericErrorResponse>(responseContent);
								Logger.Error($"Failed! | {errorResponse.Message}");
								return default;
							}

							return result;
						}
					}
				}
				catch (Exception e) {
					Logger.Exception(e);
					return default;
				}
			}

			Logger.Error($"Request to {requestUrl} failed after {maxTries} retries...");
			Logger.Error("Check your internet connection.");
			return default;
		}

		private async Task<T> PostAsJsonObject<T, U>(U requestObject, Dictionary<string, string> headers, string requestUrl, int maxTries = MAX_RETRY_COUNT) {
			if (requestObject == null || string.IsNullOrEmpty(requestUrl)) {
				return default;
			}

			string requestJson = JsonConvert.SerializeObject(requestObject);

			if (string.IsNullOrEmpty(requestJson)) {
				return default;
			}

			for (int i = 0; i < maxTries; i++) {
				try {
					using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl)) {
						if (headers != null && headers.Count > 0) {
							foreach (KeyValuePair<string, string> pair in headers) {
								request.Headers.Add(pair.Key, pair.Value);
							}
						}

						request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

						using (HttpResponseMessage response = await ExecuteRequest(async () => await HttpClient.SendAsync(request).ConfigureAwait(false))) {
							if (!response.IsSuccessStatusCode) {
								Logger.Error($"Request failed. Retrying... ({i})");
								continue;
							}

							string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

							if (string.IsNullOrEmpty(responseContent)) {
								return default;
							}

							T result = default;

							try {
								result = JsonConvert.DeserializeObject<T>(responseContent);
							}
							catch {
								GenericErrorResponse errorResponse = JsonConvert.DeserializeObject<GenericErrorResponse>(responseContent);
								Logger.Error($"Failed! | {errorResponse.Message}");
								return default;
							}

							return result;
						}
					}
				}
				catch (Exception e) {
					Logger.Exception(e);
					return default;
				}
			}

			Logger.Error($"Request to {requestUrl} failed after {maxTries} retries...");
			Logger.Error("Check your internet connection.");
			return default;
		}

		private async Task<T> ExecuteRequest<T>(Func<Task<T>> function) {
			if (function == null) {
				return default;
			}

			await RequestSync.WaitAsync().ConfigureAwait(false);

			try {
				return await function().ConfigureAwait(false);
			}
			finally {
				await Task.Delay(TimeSpan.FromSeconds(DELAY_BETWEEN_REQUESTS)).ConfigureAwait(false);
				RequestSync.Release();
			}
		}
	}
}
