using Newtonsoft.Json;

namespace GSTValidator.Models {
	public class StateResponse {
		[JsonProperty("data")]
		public District[] DistrictCollection { get; set; }

		public class District {
			[JsonProperty("c")]
			public string DistrictCode { get; set; }

			[JsonProperty("n")]
			public string DistrictName { get; set; }
		}
	}
}
