using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSTValidator.Models {
	public class GstResponse {
		[JsonProperty]
		public Practitioner[] Root { get; set; }

		public class Practitioner {
			[JsonProperty("enrlNo")]
			public string enrlNo { get; set; }
			[JsonProperty("trpNam")]
			public string trpNam { get; set; }
			[JsonProperty("stCd")]
			public string stCd { get; set; }
			[JsonProperty("dstCd")]
			public object dstCd { get; set; }
			[JsonProperty("pinCd")]
			public object pinCd { get; set; }
			[JsonProperty("cntctNo")]
			public string cntctNo { get; set; }
			[JsonProperty("emailId")]
			public string emailId { get; set; }
			[JsonProperty("ctgry")]
			public string ctgry { get; set; }
			[JsonProperty("adrs")]
			public Adrs adrs { get; set; }
			[JsonProperty("searchType")]
			public object searchType { get; set; }
			[JsonProperty("authTokn")]
			public object authTokn { get; set; }
		}

		public class Adrs {
			[JsonProperty("bldNo")]
			public string bldNo { get; set; }
			[JsonProperty("fltNo")]
			public string fltNo { get; set; }
			[JsonProperty("bldName")]
			public string bldName { get; set; }
			[JsonProperty("road")]
			public string road { get; set; }
			[JsonProperty("locality")]
			public string locality { get; set; }
			[JsonProperty("district")]
			public string district { get; set; }
			[JsonProperty("pinCd")]
			public string pinCd { get; set; }
			[JsonProperty("addr")]
			public string addr { get; set; }
		}

	}
}
