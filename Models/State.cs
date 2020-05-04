using System.Collections.Generic;
using static GSTValidator.Models.StateResponse;

namespace GSTValidator.Models {
	internal struct State {
		internal readonly Client.StateCodes StateCode;
		internal readonly List<District> Districts;

		internal State(Client.StateCodes _code, List<District> _districts) {
			StateCode = _code;
			Districts = _districts;
		}
	}
}
