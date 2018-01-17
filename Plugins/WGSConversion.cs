using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IndoorAtlas {
/// <summary>
/// A utility class which converts IndoorAtlas SDK's location coordinates to metric
/// (east, north) coordinates. This is achieved with a linear approximation around
/// a fixed "linearization point" (origin). This point can be any fixed point
/// in the 3D world whose (latitude, longitude) and (x, y, z) relation can be
/// determined accurately.
/// The origin has to be updated if the movement is "great" with respect to Earth's
/// curvature (e.g. moving to another side of the world in a simulated environment).
/// </summary>
public class WGSConversion {
	private double lat0;
	private double lon0;
	private double deltaX;
	private double deltaY;
	private bool hasLinearizationPoint;

	public WGSConversion() {
		hasLinearizationPoint = false;
	}

	/// <summary>
	/// Sets the origin in metric coordinates to (latitude, longitude) location.
	/// </summary>
	/// <param name="latitude">The latitude of origin in degrees</param>
	/// <param name="longitude">The longitude of origin in degrees</param>
	public void setOrigin(double latitude, double longitude) {
		lat0 = latitude;
		lon0 = longitude;

		const double a = 6378137.0;            // Earth's semimajor axis in meters in WGS84
		const double f = 1.0 / 298.257223563;  // Earth's flattening in WGS84
		const double b = a * (1.0 - f);
		const double a2 = a * a;
		const double b2 = b * b;
		double sinlat = System.Math.Sin(System.Math.PI / 180.0 * latitude);
		double coslat = System.Math.Cos(System.Math.PI / 180.0 * latitude);
		double tmp = System.Math.Sqrt(a2 * coslat * coslat + b2 * sinlat * sinlat);

		deltaX = (System.Math.PI / 180.0) * (a2 / tmp) * coslat;
		deltaY = (System.Math.PI / 180.0) * (a2 * b2 / (tmp * tmp * tmp));
		hasLinearizationPoint = true;
	}

	/// <summary>
	/// Converts a (latitude, longitude) pair to (east, north) metric coordinates
	/// with respect to the (previously set) origin. That is, what is the translation
	/// to east and north respectively from the origin to reach (latitude, longitude).
	/// </summary>
	/// <param name="latitude">Latitude in degrees</param>
	/// <param name="longitude">Longitude in degrees</param>
	/// <returns>Metric coordinates in a local (east, north) coordinate system.</returns>
	/// <exception cref="System.InvalidOperationException">
	/// Thrown if setOrigin hasn't been called before this function call.
	/// </exception>
	public Vector2 WGStoEN(double latitude, double longitude) {
		if (!hasLinearizationPoint) {
			throw new System.InvalidOperationException("Origin hasn't been set before the conversion.");
		}
		return new Vector2((float)(deltaX * (longitude - lon0)),
		                   (float)(deltaY * (latitude - lat0)));
	}

	/// <summary>
	/// Checks if origin has been set successfully.
	/// </summary>
	/// <returns>True if converter has origin, false otherwise.</returns>
	public bool isReady() {
		return hasLinearizationPoint;
	}
}
}
