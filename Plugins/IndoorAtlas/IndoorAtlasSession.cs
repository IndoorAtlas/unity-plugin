using UnityEngine;

namespace IndoorAtlas {

[DisallowMultipleComponent]
[DefaultExecutionOrder(-1)]
[AddComponentMenu("IndoorAtlas/IndoorAtlas Session")]
public class IndoorAtlasSession : MonoBehaviour {
    LocationManager manager = null;

    [Header("IndoorAtlas API credentials")]

    [SerializeField]
    [Tooltip("IndoorAtlas API key.")]
    string apiKey;

    [SerializeField]
    [Tooltip("IndoorAtlas API secret.")]
    string apiSecret;

    [SerializeField]
    [Tooltip("IndoorAtlas API endpoint (blank for default).")]
    string apiEndpoint;

    [Header("IndoorAtlas session configuration")]

    [SerializeField]
    [Tooltip("The minimum distance measured in meters that the device must move horizontally before an update event is generated.")]
    double m_distanceFilter = 0.7;

    public double distanceFilter
    {
        get { return m_distanceFilter; }
        set {
            m_distanceFilter = value;
            if (manager != null) manager.SetDistanceFilter(value);
        }
    }

    [SerializeField]
    [Tooltip("The minimum amount of time measured in seconds that must be elapsed before an update event is generated.")]
    double m_timeFilter = 2.0;

    public double timeFilter
    {
        get { return m_timeFilter; }
        set {
            m_timeFilter = value;
            if (manager != null) manager.SetTimeFilter(value);
        }
    }

    [SerializeField]
    [Tooltip("The minimum angular change in degrees required to generate a new heading event.")]
    double m_headingFilter = 1.0;

    public double headingFilter
    {
        get { return m_headingFilter; }
        set {
            m_headingFilter = value;
            if (manager != null) manager.SetHeadingFilter(value);
        }
    }

    [SerializeField]
    [Tooltip("The minimum angular change in degrees required to generate a new attitude event.")]
    double m_attitudeFilter = 1.0;

    public double attitudeFilter
    {
        get { return m_attitudeFilter; }
        set {
            m_attitudeFilter = value;
            if (manager != null) manager.SetAttitudeFilter(value);
        }
    }

    void WarnIfMultipleSessions() {
        var sessions = FindObjectsOfType<IndoorAtlasSession>();
        if (sessions.Length > 1) {
            // Compile a list of session names
            string sessionNames = "";
            foreach (var session in sessions) {
                sessionNames += string.Format("\t{0}\n", session.name);
            }
            Debug.LogWarningFormat(
                    "Multiple active IndoorAtlas Sessions found. " +
                    "These will conflict with each other, so " +
                    "you should only have one active IndoorAtlas Session at a time. " +
                    "Found these active sessions:\n{0}", sessionNames);
        }
    }

    void OnEnable() {
        if (manager != null) return;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        WarnIfMultipleSessions();
#endif
        manager = new LocationManager();
        manager.Init(apiKey, apiSecret, apiEndpoint, name);
        manager.SetDistanceFilter(m_distanceFilter);
        manager.SetTimeFilter(m_timeFilter);
        manager.SetHeadingFilter(m_headingFilter);
        manager.SetAttitudeFilter(m_attitudeFilter);
        manager.StartUpdatingLocation();
    }

    void OnDisable() {
        if (manager == null) return;
        manager.StopUpdatingLocation();
        manager.Close();
        manager = null;
    }

    void NativeIndoorAtlasOnLocationChanged(string data) {
#if DEVELOPMENT_BUILD
        Debug.Log("IndoorAtlas: IndoorAtlasOnLocationChanged()");
#endif
        IndoorAtlas.Location location = JsonUtility.FromJson<IndoorAtlas.Location>(data);
        BroadcastMessage("IndoorAtlasOnLocationChanged", location, SendMessageOptions.DontRequireReceiver);
    }

    void NativeIndoorAtlasOnStatusChanged(string data) {
#if DEVELOPMENT_BUILD
        Debug.Log("IndoorAtlas: IndoorAtlasOnStatusChanged()");
#endif
        IndoorAtlas.Status serviceStatus = JsonUtility.FromJson<IndoorAtlas.Status> (data);
        BroadcastMessage("IndoorAtlasOnStatusChanged", serviceStatus, SendMessageOptions.DontRequireReceiver);
    }

    void NativeIndoorAtlasOnHeadingChanged(string data) {
#if DEVELOPMENT_BUILD
        Debug.Log("IndoorAtlas: IndoorAtlasOnHeadingChanged()");
#endif
        IndoorAtlas.Heading heading = JsonUtility.FromJson<IndoorAtlas.Heading>(data);
        BroadcastMessage("IndoorAtlasOnHeadingChanged", heading, SendMessageOptions.DontRequireReceiver);
    }

    void NativeIndoorAtlasOnOrientationChanged(string data) {
#if DEVELOPMENT_BUILD
        Debug.Log("IndoorAtlas: IndoorAtlasOnOrientationChanged()");
#endif
        Quaternion orientation = JsonUtility.FromJson<IndoorAtlas.Orientation>(data).getQuaternion();
        Quaternion rot = Quaternion.Inverse(new Quaternion(orientation.x, orientation.y, -orientation.z, orientation.w));
        Quaternion unityRot = Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f)) * rot;
        BroadcastMessage("IndoorAtlasOnOrientationChanged", unityRot, SendMessageOptions.DontRequireReceiver);
    }

    void NativeIndoorAtlasOnEnterRegion(string data) {
#if DEVELOPMENT_BUILD
        Debug.Log("IndoorAtlas: IndoorAtlasOnEnterRegion()");
#endif
        IndoorAtlas.Region region = JsonUtility.FromJson<IndoorAtlas.Region>(data);
        BroadcastMessage("IndoorAtlasOnEnterRegion", region, SendMessageOptions.DontRequireReceiver);
    }

    void NativeIndoorAtlasOnExitRegion(string data) {
#if DEVELOPMENT_BUILD
        Debug.Log("IndoorAtlas: IndoorAtlasOnExitRegion()");
#endif
        IndoorAtlas.Region region = JsonUtility.FromJson<IndoorAtlas.Region>(data);
        BroadcastMessage("IndoorAtlasOnExitRegion", region, SendMessageOptions.DontRequireReceiver);
    }

    void NativeIndoorAtlasOnRoute(string data) {
#if DEVELOPMENT_BUILD
        Debug.Log("IndoorAtlas: IndoorAtlasOnRoute()");
#endif
        IndoorAtlas.Route route = JsonUtility.FromJson<IndoorAtlas.Route>(data);
        BroadcastMessage("IndoorAtlasOnRoute", route, SendMessageOptions.DontRequireReceiver);
    }
}

}
