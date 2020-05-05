using Newtonsoft.Json;

namespace GSTValidator.Models {
	public class StateResponse {
		[JsonProperty("data")]
		public Data[] StateCollection { get; set; }

		public class Data {
			[JsonProperty("c")]
			public string DistrictCode { get; set; }

			[JsonProperty("n")]
			public Districts[] DistrictCollection { get; set; }
		}

		public class Districts {
			[JsonProperty("c")]
			public string DistrictNumericalCode { get; set; }

			[JsonProperty("n")]
			public string DistrictName { get; set; }
		}
	}
}
