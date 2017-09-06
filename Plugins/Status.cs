using System;
using UnityEngine;

namespace IndoorAtlas {
	[Serializable]
	public class Status
	{
		public enum ServiceStatus : int {
			Limited = 10,
			OutOfService = 0,
			TemporarilyUnavailable = 1,
			Available = 2
		};

		public ServiceStatus status;      // Service status
	}
}
