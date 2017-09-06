using System;
using UnityEngine;

namespace IndoorAtlas {
	[Serializable]
	public class Heading
	{
		public double heading;      // The heading is the direction of the y-axis projected to the horizontal plane. The values are the same as in IALocation.getBearing() with 0 indicating north, 90 east, and so on. The provided timestamp is consistent with IALocation.getTime()
		public long timestamp;      // UTC time of this orientation fix.
	}
}
