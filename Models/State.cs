using System.Collections.Generic;
using static GSTValidator.Enums;
using static GSTValidator.Models.StateResponse;

namespace GSTValidator.Models {
	internal class State {
		internal readonly StateCodes StateCode;
		internal readonly List<Districts> Districts;
		internal int DistrictsCount => Districts != null ? Districts.Count : 0;

		internal State(StateCodes _code, List<Districts> _districts) {
			StateCode = _code;
			Districts = _districts;
		}
	}
}
