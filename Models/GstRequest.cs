using Newtonsoft.Json;

namespace GSTValidator.Models {
	public class GstRequest {
		[JsonProperty("searchType")]
		public string SearchType { get; set; }

		[JsonProperty("trpNam")]
		public string PractitionerName { get; set; }

		[JsonProperty("stCd")]
		public string StateCode { get; set; }

		[JsonProperty("dstCd")]
		public string DistrictCode { get; set; }

		[JsonProperty("pinCd")]
		public string PinCode { get; set; }
	}
}
