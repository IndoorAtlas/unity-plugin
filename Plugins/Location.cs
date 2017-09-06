using System;

namespace IndoorAtlas {
	[Serializable]
	public class Location
	{
		public float accuracy;      // Location accuracy in meters.
		public double altitude;     // Altitude in meters, or 0.0 if not available.
		public float bearing;       // Bearing in degrees, in range of (0.0, 360.0].
		public int floorLevel;      // The logical floor level of building.
		public bool hasFloorlevel;  // true if this location fix had floor level information.
		public double latitude;     // Longitude in degrees.
		public double longitude;    // Latitude in degrees.
		public long timestamp;      // UTC time of this location fix.
	}
}
