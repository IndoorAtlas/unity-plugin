using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.Assertions;


public class IaBehavior : MonoBehaviour {
	[Header("IndoorAtlas API credentials")]
	public string apiKey;
	public string apiSecret;

	[Header("Orientation request configuration")]
	public double headingSensitivity = 0.01f;
	public double orientationSensitivity = 0.0001f;
	#if UNITY_ANDROID
	private AndroidJavaObject iaJavaObject;
	#elif UNITY_IOS
	#endif

	// Initialization
	void Start () {
	#if UNITY_ANDROID
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		AndroidJavaObject joApikey = new AndroidJavaObject("java.lang.String", apiKey);
		AndroidJavaObject joApisecret = new AndroidJavaObject("java.lang.String", apiSecret);
		AndroidJavaObject joGobjName = new AndroidJavaObject("java.lang.String", name);
		iaJavaObject = new AndroidJavaObject ("com.indooratlas.android.unity.IaUnityPlugin",
		jo, joApikey, joApisecret, joGobjName, headingSensitivity, orientationSensitivity);
	#elif UNITY_IOS
		IAInit(apiKey, apiSecret, name, headingSensitivity, orientationSensitivity);
	#endif
	}

	#if UNITY_IOS
	[DllImport ("__Internal")] private static extern bool IAclose ();
	#endif
	void OnDestroy () {
	#if UNITY_ANDROID
		iaJavaObject.Call("close");
	#elif UNITY_IOS
		if (!IAclose()) {
			throw new System.Exception("IndoorAtlas Unity plugin has to be initialized successfully before closing it");
		}
	#endif
	}

	#if UNITY_IOS
	[DllImport ("__Internal")] private static extern bool IAinit (string apikey, string apisecret, string name, double headingSensitivity, double orientationSensitivity);
	public void IAInit (string apikey, string apisecret, string name, double headingSensitivity, double orientationSensitivity)
	{
		if (!IAinit(apikey, apisecret, name, headingSensitivity, orientationSensitivity)) {
			throw new System.Exception("IndoorAtlas Unity plugin has already been initialized");
		}
	}
	#endif
}
