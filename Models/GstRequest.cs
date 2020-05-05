using Newtonsoft.Json;
using static GSTValidator.Enums;

namespace GSTValidator.Models {
	public class GstRequest {
		[JsonProperty("searchType")]
		public string SearchType { get; set; }

		[JsonProperty("trpNam")]
		public string PractitionerName { get; set; }

		[JsonProperty("stCd")]
		public StateCodes StateCode { get; set; }

		[JsonProperty("dstCd")]
		public string DistrictCode { get; set; }

		[JsonProperty("pinCd")]
		public string PinCode { get; set; }

		public GstRequest(string _searchType, string _practitionerName, StateCodes _stateCode, string _districtCode, string _pinCode) {
			SearchType = _searchType;
			PractitionerName = _practitionerName;
			StateCode = _stateCode;
			DistrictCode = _districtCode;
			PinCode = _pinCode;
		}
	}
}
