using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSTValidator.Models {
	public class GenericErrorResponse {
		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonProperty("errorCode")]
		public string ErrorCode { get; set; }
	}
}
