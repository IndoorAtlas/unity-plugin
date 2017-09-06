using System;

namespace IndoorAtlas {
	[Serializable]
	public class Region
	{
		public enum Type : int {
			Unknown = 0,
			FloorPlan = 1,
			Venue = 2
		};

		public string id;
		public string name;
		public long timestamp;
		public Type type;
	}
}
