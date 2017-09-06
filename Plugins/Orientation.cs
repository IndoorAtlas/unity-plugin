using System;
using UnityEngine;

namespace IndoorAtlas {
	[Serializable]
	public class Orientation
	{
		public double x;            // Orientation quaternion (w, x, y, z).
		public double y;
		public double z;
		public double w;
		public long timestamp;      // UTC time of this orientation fix.

		public Quaternion getQuaternion() {
			return new Quaternion ((float)x, (float)y, (float)z, (float)w);
		}
	}
}
