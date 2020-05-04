using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using GSTValidator.Models;
using System.Linq;

namespace GSTValidator {
	internal class Client {
		private const int MAX_RETRY_COUNT = 3;
		private const string ALL_SEARCH_TYPE = "A";
		private const string REQUEST_BASE_URL = "https://services.gst.gov.in/";
		private const string GST_URL = REQUEST_BASE_URL + "services/api/search/gstp";

		private const int DELAY_BETWEEN_REQUESTS = 10; // in seconds
		private readonly Logger Logger = new Logger("CLIENT");
		private static readonly HttpClient HttpClient = new HttpClient();
		private static readonly SemaphoreSlim RequestSync = new SemaphoreSlim(1, 1);

		internal readonly List<State> States = new List<State>();

		private string GenerateStateUrl(StateCodes stateCode) => REQUEST_BASE_URL + "master/st/" + (int) stateCode + "/district";

		internal async Task InitAsync() {
			if(await CacheStatesAsync().ConfigureAwait(false)) {
				Logger.Info("Loaded all states!");
			}


		}

		private async Task<bool> CacheStatesAsync() {
			Logger.Info($"Caching {Enum.GetValues(typeof(StateCodes))} states...");

			foreach (StateCodes code in Enum.GetValues(typeof(StateCodes))) {
				string requestUrl = GenerateStateUrl(code);
				StateResponse response = await GetAsJsonObject<StateResponse>(requestUrl).ConfigureAwait(false);

				if(response == null || response.DistrictCollection.Length <= 0) {
					continue;
				}

				State state = new State(code, response.DistrictCollection.ToList());
				States.Add(state);
				Logger.Info($"Loaded '{code.ToString()}' state with '{state.Districts.Count}' districts.");
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
						if(headers != null && headers.Count > 0) {
							foreach(var pair in headers) {
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

							return JsonConvert.DeserializeObject<TResponseType>(responseContent);
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
			if(requestObject == null || string.IsNullOrEmpty(requestUrl)) {
				return default;
			}

			string requestJson = JsonConvert.SerializeObject(requestObject);

			if (string.IsNullOrEmpty(requestJson)) {
				return default;
			}

			for(int i = 0; i < maxTries; i++) {
				try {
					using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl)) {
						if (headers != null && headers.Count > 0) {
							foreach (var pair in headers) {
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

							return JsonConvert.DeserializeObject<T>(responseContent);
						}
					}
				}
				catch(Exception e) {
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

		internal enum StateCodes {
			JammuKashmir = 01,
			HimachalPradesh = 02,
			Punjab = 03,
			Chandigarh = 04,
			Uttarakhand = 05,
			Haryana = 06,
			Delhi = 07,
			Rajasthan = 08,
			UttarPradesh = 09,
			Bihar = 10,
			Sikkim = 11,
			ArunachalPradesh = 12,
			Nagaland = 13,
			Manipur = 14,
			Mizoram = 15,
			Tripura = 16,
			Meghalaya = 17,
			Assam = 18,
			WestBengal = 19,
			Jharkhand = 20,
			Orissa = 21,
			Chhattisgarh = 22,
			MadhyaPradesh = 23,
			Gujarat = 24,
			DamanDiu = 25,
			DadraNagarHaveli = 26,
			Maharashtra = 27,
			AndhraPradeshOld = 28,
			Karnataka = 29,
			Goa = 30,
			Lakshadweep = 31,
			Kerala = 32,
			TamilNadu = 33,
			Puducherry = 34,
			AndamanNicobarIslands = 35,
			Telengana = 36,
			AndhraPradeshNew = 37
		}
	}
}
