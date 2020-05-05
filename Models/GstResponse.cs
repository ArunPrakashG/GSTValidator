using Newtonsoft.Json;

namespace GSTValidator.Models {
	public class GstResponse {
		public class Practitioner {
			[JsonProperty("enrlNo")]
			public string EnrolmentNumber { get; set; }

			[JsonProperty("trpNam")]
			public string PractitionerName { get; set; }

			[JsonProperty("stCd")]
			public string StateCode { get; set; }

			[JsonProperty("dstCd")]
			public string DistrictCode { get; set; }

			[JsonProperty("pinCd")]
			public string PinCode { get; set; }

			[JsonProperty("cntctNo")]
			public string ContactNumber { get; set; }

			[JsonProperty("emailId")]
			public string EmailID { get; set; }

			[JsonProperty("ctgry")]
			public string Catagory { get; set; }

			[JsonProperty("adrs")]
			public Address Address { get; set; }

			[JsonProperty("searchType")]
			public string SearchType { get; set; }

			[JsonProperty("authTokn")]
			public string AuthToken { get; set; }
		}

		public class Address {
			[JsonProperty("bldNo")]
			public string BuildingNumber { get; set; }

			[JsonProperty("fltNo")]
			public string FlatNumber { get; set; }

			[JsonProperty("bldName")]
			public string BuildName { get; set; }

			[JsonProperty("road")]
			public string Road { get; set; }

			[JsonProperty("locality")]
			public string Locality { get; set; }

			[JsonProperty("district")]
			public string District { get; set; }

			[JsonProperty("pinCd")]
			public string PinCode { get; set; }

			[JsonProperty("addr")]
			public string FullAddress { get; set; }
		}
	}
}
