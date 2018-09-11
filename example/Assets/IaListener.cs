using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IaListener : MonoBehaviour {
	void Start () {
	}
	
	void Update () {
	}

	void Awake() {
	}

	void onLocationChanged(string data) {
		IndoorAtlas.Location location = JsonUtility.FromJson<IndoorAtlas.Location>(data);
		Debug.Log ("onLocationChanged " + location.latitude + ", " + location.longitude);
	}

	void onStatusChanged(string data) {
		IndoorAtlas.Status serviceStatus = JsonUtility.FromJson<IndoorAtlas.Status> (data);
		Debug.Log ("onStatusChanged " + serviceStatus.status);
		Debug.Log ("Trace ID: " + GetComponent<IaBehavior>().GetTraceID());
	}

	void onHeadingChanged(string data) {
		IndoorAtlas.Heading heading = JsonUtility.FromJson<IndoorAtlas.Heading>(data);
		Debug.Log ("onHeadingChanged " + heading.heading);
	}

	void onOrientationChange(string data) {
		Quaternion orientation = JsonUtility.FromJson<IndoorAtlas.Orientation>(data).getQuaternion();
		Quaternion rot = Quaternion.Inverse(new Quaternion(orientation.x, orientation.y, -orientation.z, orientation.w));
		Camera.main.transform.rotation = Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)) * rot;
	}

	void onEnterRegion (string data) {
		IndoorAtlas.Region region = JsonUtility.FromJson<IndoorAtlas.Region>(data);
		Debug.Log ("onEnterRegion " + region.name + ", " + region.type + ", " + region.id + " at " + region.timestamp);
	}

	void onExitRegion (string data) {
		IndoorAtlas.Region region = JsonUtility.FromJson<IndoorAtlas.Region>(data);
		Debug.Log ("onExitRegion " + region.name + ", " + region.type + ", " + region.id + " at " + region.timestamp);
	}
}
