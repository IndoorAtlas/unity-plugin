using System;
using UnityEngine;
using System.Runtime.InteropServices;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace IndoorAtlas {
[Serializable]
public class WGS84 {
    // Longitude in degrees.
    public double latitude;
    // Latitude in degrees.
    public double longitude;
}

[Serializable]
public class LatLngFloor {
    // WGS84 coordinate.
    public WGS84 coordinate;
    // Floor level.
    public int floor;
}

[Serializable]
public class Heading {
    // The heading is the direction of the y-axis projected to the horizontal plane. The values are the same as in IALocation.getBearing() with 0 indicating north, 90 east, and so on. The provided timestamp is consistent with IALocation.getTime()
    public double heading;
    // UTC time of this orientation fix.
    public long timestamp;
}

[Serializable]
public class Location {
    // Location accuracy in meters.
    public float accuracy;
    // Altitude in meters, or 0.0 if not available.
    public double altitude;
    // Bearing in degrees, in range of (0.0, 360.0].
    public float bearing;
    // Position information.
    LatLngFloor position;
    // UTC time of this location fix.
    public long timestamp;
}

[Serializable]
public class Orientation {
    // Orientation quaternion (w, x, y, z).
    public double x, y, z, w;
    // UTC time of this orientation fix.
    public long timestamp;

    public Quaternion getQuaternion() {
        return new Quaternion((float)x, (float)y, (float)z, (float)w);
    }
}

[Serializable]
public class Geofence {
    // Floorplan id
    public string id;
    // Name
    public string name;
    // JSON payload
    public string payload;
    // Position
    public LatLngFloor position;
    // Unique points of the geofence, if any
    public WGS84[] points;
}

[Serializable]
public class POI {
    // Floorplan id
    public string id;
    // Name
    public string name;
    // JSON payload
    public string payload;
    // Position
    public LatLngFloor position;
}

[Serializable]
public class Floorplan {
    // Floorplan id
    public string id;
    // Name
    public string name;
    // URL for the image used for the mapping
    public string imageUrl;
    // Width of the image in pixels
    public uint width;
    // Height of the image in pixels
    public uint height;
    // Pixel to meter conversion
    public float pixelToMeterConversion;
    // Meter to pixel conversion
    public float meterToPixelConversion;
    // Width in meters
    public float widthMeters;
    // Height in meters
    public float heightMeters;
    // Floor number
    public int floor;
}

[Serializable]
public class Venue {
    // Floorplan id
    public string id;
    // Name
    public string name;
    // Floor plans in this venue
    public Floorplan[] floorplans;
    // Geofences in this venue
    public Geofence[] geofences;
    // Pois in this venue
    public POI[] pois;
}

[Serializable]
public class Region {
    public enum Type : int {
        Unknown = 0,
        // Floorplan region.
        FloorPlan = 1,
        // Venue region.
        Venue = 2,
        // Geofence region.
        Geofence = 3,
    };

    // Region id.
    public string id;
    // Region name.
    public string name;
    // UTC time of this region event.
    public long timestamp;
    // Region type.
    public Type type;
    // If this is a venue region then this will point to the venue object
    public Venue venue;
    // If this is a floorplan region then this will point to the floorplan object
    public Floorplan floorplan;
    // If this is a geofence region then this will point to the geofence object
    public Geofence geofence;
}

[Serializable]
public class Status {
    public enum ServiceStatus : int {
        // Service is available, but might have a reduced performance.
        Limited = 10,
        // Service is unavailable.
        OutOfService = 0,
        // Service is temporarily unavailable.
        TemporarilyUnavailable = 1,
        // Service is available and working normally.
        Available = 2
    };

    // Service status.
    public ServiceStatus status;
}

[Serializable]
public class RoutePoint {
    // Position of the point
    LatLngFloor position;
    // Zero-based index of the node in the original JSON graph this point corresponds to.
    // If this is a virtual wayfinding node, e.g., a strating point of the route outside
    // the original graph, nodeIndex will be -1.
    int nodeIndex;
}

[Serializable]
public class RouteLeg {
    // Starting point of the leg.
    RoutePoint begin;
    // Ending point of the leg.
    RoutePoint end;
    // Lenght of the leg in meters.
    double length;
    // Direction of the leg in ENU coordinates in degrees.
    // 0 is North and 90 is East.
    double direction;
    // Zero-based index of the edge corresponding to this leg in the original JSON graph.
    // If this is a virtual leg, for example, a segment connecting an off-graph starting
    // point to the graph, edgeIndex will be -1.
    int edgeIndex;
}

[Serializable]
public class Route {
    // Array of RouteLeg objects representing the route.
    RouteLeg[] legs;
    // Whether route is available
    bool isSuccessful;
    // Error status for routing
    string error;
}

public class LocationManager {
#if UNITY_ANDROID
    private static AndroidJavaObject jPlugin = null;
#endif

    private static readonly Matrix4x4 m_unityWorldToIndoorAtlasWorld = Matrix4x4.Scale(new Vector3(1, 1, -1));

#if UNITY_IOS
    [DllImport("__Internal")] private static extern bool indooratlas_init(string apikey, string apisecret, string apiEndpoint, string session);
#endif
    // Initializes the IndoorAtlas SDK
    // Do not call this manually, this is handled by IndoorAtlasSession game object!
    public void Init(string apiKey, string apiSecret, string apiEndpoint, string session) {
#if DEVELOPMENT_BUILD
        Debug.Log("IndoorAtlas: Init()");
#endif
#if UNITY_IOS
        if (!indooratlas_init(apiKey, apiSecret, apiEndpoint, session))
            throw new System.Exception("IndoorAtlas: failed to initialize IndoorAtlasSession, do you have multiple sessions?");
#elif UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation)) Permission.RequestUserPermission(Permission.FineLocation);
        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation)) Permission.RequestUserPermission(Permission.CoarseLocation);
        Input.location.Start();
        AndroidJavaClass jUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jActivity = jUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject jApiKey = new AndroidJavaObject("java.lang.String", apiKey);
        AndroidJavaObject jApiSecret = new AndroidJavaObject("java.lang.String", apiSecret);
        AndroidJavaObject jApiEndpoint = new AndroidJavaObject("java.lang.String", apiEndpoint);
        AndroidJavaObject jSession = new AndroidJavaObject("java.lang.String", session);
        jPlugin = new AndroidJavaObject("com.indooratlas.android.unity.Plugin", jActivity, jApiKey, jApiSecret, jApiEndpoint, jSession);
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern bool indooratlas_close();
#endif
    // Deinitalizes the IndoorAtlas SDK
    // Do not call this manually, this is handled by IndoorAtlasSession game object!
    public void Close() {
#if DEVELOPMENT_BUILD
        Debug.Log("IndoorAtlas: Close()");
#endif
#if UNITY_IOS
        indooratlas_close();
#elif UNITY_ANDROID
        jPlugin.Call("close");
        jPlugin = null;
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern string indooratlas_versionString();
#endif
    public string VersionString() {
#if UNITY_IOS
        return indooratlas_versionString();
#elif UNITY_ANDROID
        return jPlugin.Call<string>("versionString");
#else
        return "0.0.0+indooratlas.unavailable.on.this.platform";
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern void indooratlas_setDistanceFilter(double filter);
#endif
    public void SetDistanceFilter(double filter) {
#if UNITY_IOS
        indooratlas_setDistanceFilter(filter);
#elif UNITY_ANDROID
        jPlugin.Call("setDistanceFilter", filter);
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern double indooratlas_getDistanceFilter();
#endif
    public double GetDistanceFilter() {
#if UNITY_IOS
        return indooratlas_getDistanceFilter();
#elif UNITY_ANDROID
        return jPlugin.Call<double>("getDistanceFilter");
#else
        return 0;
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern void indooratlas_setTimeFilter(double filter);
#endif
    public void SetTimeFilter(double filter) {
#if UNITY_IOS
        indooratlas_setTimeFilter(filter);
#elif UNITY_ANDROID
        jPlugin.Call("setTimeFilter", filter);
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern double indooratlas_getTimeFilter();
#endif
    public double GetTimeFilter() {
#if UNITY_IOS
        return indooratlas_getTimeFilter();
#elif UNITY_ANDROID
        return jPlugin.Call<double>("getTimeFilter");
#else
        return 0;
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern void indooratlas_setHeadingFilter(double filter);
#endif
    public void SetHeadingFilter(double filter) {
#if UNITY_IOS
        indooratlas_setHeadingFilter(filter);
#elif UNITY_ANDROID
        jPlugin.Call("setHeadingFilter", filter);
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern double indooratlas_getHeadingFilter();
#endif
    public double GetHeadingFilter() {
#if UNITY_IOS
        return indooratlas_getHeadingFilter();
#elif UNITY_ANDROID
        return jPlugin.Call<double>("getHeadingFilter");
#else
        return 0;
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern void indooratlas_setAttitudeFilter(double filter);
#endif
    public void SetAttitudeFilter(double filter) {
#if UNITY_IOS
        indooratlas_setAttitudeFilter(filter);
#elif UNITY_ANDROID
        jPlugin.Call("setAttitudeFilter", filter);
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern double indooratlas_getAttitudeFilter();
#endif
    public double GetAttitudeFilter() {
#if UNITY_IOS
        return indooratlas_getAttitudeFilter();
#elif UNITY_ANDROID
        return jPlugin.Call<double>("getAttitudeFilter");
#else
        return 0;
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern void indooratlas_lockFloor(int floor);
#endif
    public void LockFloor(int floor) {
#if UNITY_IOS
        indooratlas_lockFloor(floor);
#elif UNITY_ANDROID
        jPlugin.Call("lockFloor", floor);
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern void indooratlas_unlockFloor();
#endif
    public void UnlockFloor() {
#if UNITY_IOS
        indooratlas_unlockFloor();
#elif UNITY_ANDROID
        jPlugin.Call("unlockFloor");
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern void indooratlas_lockIndoors(bool flag);
#endif
    public void LockIndoors(bool flag) {
#if UNITY_IOS
        indooratlas_lockIndoors(flag);
#elif UNITY_ANDROID
        jPlugin.Call("lockIndoors", flag);
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern void indooratlas_startUpdatingLocation();
#endif
    public void StartUpdatingLocation() {
#if DEVELOPMENT_BUILD
        Debug.Log("IndoorAtlas: StartUpdatingLocation()");
#endif
#if UNITY_IOS
        indooratlas_startUpdatingLocation();
#elif UNITY_ANDROID
        jPlugin.Call("startUpdatingLocation");
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern void indooratlas_stopUpdatingLocation();
#endif
    public void StopUpdatingLocation() {
#if DEVELOPMENT_BUILD
        Debug.Log("IndoorAtlas: StopUpdatingLocation()");
#endif
#if UNITY_IOS
        indooratlas_stopUpdatingLocation();
#elif UNITY_ANDROID
        jPlugin.Call("stopUpdatingLocation");
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern void indooratlas_startMonitoringForWayfinding(string to);
#endif
    public void StartMonitoringForWayfinding(LatLngFloor to) {
#if DEVELOPMENT_BUILD
        Debug.Log("IndoorAtlas: StartMonitoringForWayfinding()");
#endif
#if UNITY_IOS
        indooratlas_startMonitoringForWayfinding(JsonUtility.ToJson(to));
#elif UNITY_ANDROID
        jPlugin.Call("startMonitoringForWayfinding", JsonUtility.ToJson(to));
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern void indooratlas_stopMonitoringForWayfinding();
#endif
    public void StopMonitoringForWayfinding() {
#if UNITY_IOS
        indooratlas_stopMonitoringForWayfinding();
#elif UNITY_ANDROID
        jPlugin.Call("stopMonitoringForWayfinding");
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern string indooratlas_traceID();
#endif
    public string GetTraceId() {
#if UNITY_IOS
        return indooratlas_traceID();
#elif UNITY_ANDROID
        return jPlugin.Call<string>("getTraceId");
#else
        return "";
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern void indooratlas_releaseArSession();
#endif
    public void ReleaseArSession() {
#if UNITY_IOS
        indooratlas_releaseArSession();
#elif UNITY_ANDROID
        jPlugin.Call("releaseArSession");
#endif
    }

    static public Matrix4x4 UnityMatrixToIndoorAtlasMatrix(Matrix4x4 matrix) {
        // Convert Unity left-handed matrices to IndoorAtlas right-handed matrices.
        return m_unityWorldToIndoorAtlasWorld * matrix * m_unityWorldToIndoorAtlasWorld;
    }

    static public Matrix4x4 IndoorAtlasMatrixToUnityMatrix(Matrix4x4 matrix) {
        // Convert IndoorAtlas right-handed matrices to Unity left-handed matrices.
        return m_unityWorldToIndoorAtlasWorld * matrix;
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern void indooratlas_setArPoseMatrix(float[] matrix);
#endif
    public void SetArPoseMatrix(Matrix4x4 matrix) {
        matrix = UnityMatrixToIndoorAtlasMatrix(matrix);
#if UNITY_IOS
        // The iOS SDK internally transforms ARKit pose matrix to IndoorAtlas compatible matrix.
        // However, ARFoundation's pose matrix is already transformed from ARKit to Unity's left-handed world coordinates.
        // Apply inverse of ARKit transformation here to make the transformation in SDK no-op.
        Matrix4x4 arKitInverse = Matrix4x4.zero;
        arKitInverse[1,0] = -1;
        arKitInverse[0,1] =  1;
        arKitInverse[2,2] =  1;
        arKitInverse[3,3] =  1;
        matrix *= arKitInverse;
#endif
        float[] native = new float[16];
        for (int i = 0; i < 16; ++i) native[i] = matrix[i];
#if UNITY_IOS
        indooratlas_setArPoseMatrix(native);
#elif UNITY_ANDROID
        jPlugin.Call("setArPoseMatrix", native);
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern void indooratlas_setArCameraToWorldMatrix(float[] matrix);
#endif
    public void SetArCameraToWorldMatrix(Matrix4x4 matrix) {
        matrix = IndoorAtlasMatrixToUnityMatrix(matrix);
        float[] native = new float[16];
        for (int i = 0; i < 16; ++i) native[i] = matrix[i];
#if UNITY_IOS
        indooratlas_setArCameraToWorldMatrix(native);
#elif UNITY_ANDROID
        jPlugin.Call("setArCameraToWorldMatrix", native);
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern bool indooratlas_getArIsConverged();
#endif
    public bool GetArIsConverged() {
#if UNITY_IOS
        return indooratlas_getArIsConverged();
#elif UNITY_ANDROID
        return jPlugin.Call<bool>("getArIsConverged");
#else
        return false;
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern bool indooratlas_getArCompassMatrix(float[] matrix);
#endif
    public Matrix4x4 GetArCompassMatrix() {
        Matrix4x4 matrix = Matrix4x4.identity;
#if UNITY_IOS
        float[] native = new float[16];
        if (indooratlas_getArCompassMatrix(native)) for (int i = 0; i < 16; ++i) matrix[i] = native[i];
#elif UNITY_ANDROID
        float[] native = jPlugin.Call<float[]>("getArCompassMatrix");
        if (native.Length == 16) for (int i = 0; i < 16; ++i) matrix[i] = native[i];
#endif
        return (matrix != Matrix4x4.identity ? IndoorAtlasMatrixToUnityMatrix(matrix) : matrix);
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern bool indooratlas_getArGoalMatrix(float[] matrix);
#endif
    public Matrix4x4 GetArGoalMatrix() {
        Matrix4x4 matrix = Matrix4x4.identity;
#if UNITY_IOS
        float[] native = new float[16];
        if (indooratlas_getArGoalMatrix(native)) for (int i = 0; i < 16; ++i) matrix[i] = native[i];
#elif UNITY_ANDROID
        float[] native = jPlugin.Call<float[]>("getArGoalMatrix");
        if (native.Length == 16) for (int i = 0; i < 16; ++i) matrix[i] = native[i];
#endif
        return (matrix != Matrix4x4.identity ? IndoorAtlasMatrixToUnityMatrix(matrix) : matrix);
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern int indooratlas_getArTurnCount();
#endif
    public int GetArTurnCount() {
#if UNITY_IOS
        return indooratlas_getArTurnCount();
#elif UNITY_ANDROID
        return jPlugin.Call<int>("getArTurnCount");
#else
        return 0;
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern bool indooratlas_getArTurnMatrix(int index, float[] matrix);
#endif
    public Matrix4x4 GetArTurnMatrix(int index) {
        Matrix4x4 matrix = Matrix4x4.identity;
#if UNITY_IOS
        float[] native = new float[16];
        if (indooratlas_getArTurnMatrix(index, native)) for (int i = 0; i < 16; ++i) matrix[i] = native[i];
#elif UNITY_ANDROID
        float[] native = jPlugin.Call<float[]>("getArTurnMatrix", index);
        if (native.Length == 16) for (int i = 0; i < 16; ++i) matrix[i] = native[i];
#endif
        return (matrix != Matrix4x4.identity ? IndoorAtlasMatrixToUnityMatrix(matrix) : matrix);
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern void indooratlas_addArPlane(float cx, float cy, float cz, float ex, float ez);
#endif
    public void AddArPlane(float cx, float cy, float cz, float ex, float ez) {
        Vector3 c = m_unityWorldToIndoorAtlasWorld.MultiplyPoint3x4(new Vector3(cx, cy, cz));
        Vector3 e = m_unityWorldToIndoorAtlasWorld.MultiplyPoint3x4(new Vector3(ex, 0, ez));
#if UNITY_IOS
        indooratlas_addArPlane(c.x, c.y, c.z, e.x, e.z);
#elif UNITY_ANDROID
        jPlugin.Call("addArPlane", c.x, c.y, c.z, e.x, e.z);
#endif
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern void indooratlas_geoToAr(double lat, double lon, int floor, float heading, float zOffset, float[] matrix);
#endif
    public Matrix4x4 GeoToAr(double lat, double lon, int floor, float heading, float zOffset) {
        Matrix4x4 matrix = Matrix4x4.identity;
#if UNITY_IOS
        float[] native = new float[16];
        indooratlas_geoToAr(lat, lon, floor, heading, zOffset, native);
        for (int i = 0; i < 16; ++i) matrix[i] = native[i];
#elif UNITY_ANDROID
        float[] native = jPlugin.Call<float[]>("geoToAr", lat, lon, floor, heading, zOffset);
        if (native.Length == 16) for (int i = 0; i < 16; ++i) matrix[i] = native[i];
#endif
        return (matrix != Matrix4x4.identity ? IndoorAtlasMatrixToUnityMatrix(matrix) : matrix);
    }

#if UNITY_IOS
    [DllImport("__Internal")] private static extern string indooratlas_arToGeo(double x, double y, double z);
#endif
    public Location ArToGeo(float x, float y, float z) {
        Vector3 c = m_unityWorldToIndoorAtlasWorld.MultiplyPoint3x4(new Vector3(x, y, z));
#if UNITY_IOS
        string data = indooratlas_arToGeo(c.x, c.y, c.z);
#elif UNITY_ANDROID
        string data = jPlugin.Call<string>("arToGeo", c.x, c.y, c.z);
#endif
        return (data != "" ? JsonUtility.FromJson<Location>(data) : null);
    }
}
}
