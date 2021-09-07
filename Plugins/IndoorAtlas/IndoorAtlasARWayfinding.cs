using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace IndoorAtlas {

[DisallowMultipleComponent]
[AddComponentMenu("IndoorAtlas/IndoorAtlas AR Wayfinding")]
public class IndoorAtlasARWayfinding : MonoBehaviour {
    LocationManager manager = null;

    [Header("IndoorAtlas AR wayfinding configuration")]

    [SerializeField]
    [Tooltip("The LatLngFloor that marks the target for navigation.")]
    LatLngFloor m_target;

    /// <summary>
    /// The <c>LatLngFloor</c> that marks the target for navigation.
    /// </summary>
    public LatLngFloor target
    {
        get { return m_target; }
        set {
            m_target = value;
            if (manager != null && m_wayfinding) {
                InstantiateTurns();
                manager.StopMonitoringForWayfinding();
                manager.StartMonitoringForWayfinding(m_target);
            }
        }
    }

    [SerializeField]
    [Tooltip("Enable wayfinding?")]
    bool m_wayfinding = true;

    public bool wayfinding
    {
        get { return m_wayfinding; }
        set {
            if (m_wayfinding == value) return;
            if (manager != null) InstantiateTurns();
            if ((m_wayfinding = value)) {
                if (manager != null) {
                    manager.StartMonitoringForWayfinding(m_target);
                }
            } else {
                if (manager != null) {
                    manager.StopMonitoringForWayfinding();
                }
            }
        }
    }

    [SerializeField]
    [Tooltip("The ARPlaneManager for capturing horizontal planes")]
    ARPlaneManager m_planeManager;

    /// <summary>
    /// The <c>LatLngFloor</c> that marks the target for navigation.
    /// </summary>
    public ARPlaneManager planeManager
    {
        get { return m_planeManager; }
        set { m_planeManager = value; }
    }

    [SerializeField]
    [Tooltip("The Camera to associate with the AR device.")]
    Camera m_camera;
    ARCameraManager m_cameraManager;

    bool IsTracking() {
        switch(ARSession.notTrackingReason)
        {
            case NotTrackingReason.None:
                return true;
            case NotTrackingReason.Initializing:
                break;
            case NotTrackingReason.Relocalizing:
                break;
            case NotTrackingReason.InsufficientLight:
                break;
            case NotTrackingReason.InsufficientFeatures:
                break;
            case NotTrackingReason.ExcessiveMotion:
                break;
            case NotTrackingReason.Unsupported:
                break;
        }
        return false;
    }

    void OnArFrame(ARCameraFrameEventArgs eventArgs) {
        if (manager == null || !IsTracking()) return;
        foreach (ARPlane plane in m_planeManager.trackables) {
            if (plane.alignment != PlaneAlignment.HorizontalUp) continue;
            manager.AddArPlane(plane.center.x, plane.center.y, plane.center.z, plane.extents.x, plane.extents.y);
        }
        manager.SetArPoseMatrix(m_camera.transform.localToWorldMatrix);
    }

    void RegisterFrameEvent() {
        if (manager == null) return;
        m_cameraManager.frameReceived += OnArFrame;
    }

    /// <summary>
    /// The <c>Camera</c> to associate with the AR device.
    /// </summary>
#if UNITY_EDITOR
    public new Camera camera
#else
    public Camera camera
#endif
    {
        get { return m_camera; }
        set {
           m_camera = value;
           if (m_cameraManager != null) m_cameraManager.frameReceived -= OnArFrame;
           m_cameraManager = m_camera.GetComponent<ARCameraManager>();
           RegisterFrameEvent();
        }
    }

    [SerializeField]
    [Tooltip("The GameObject that points to the navigation direction.")]
    GameObject m_compass;

    /// <summary>
    /// The <c>GameObject</c> that points to the navigation direction.
    /// </summary>
    public GameObject compass
    {
        get { return m_compass; }
        set { m_compass = value; }
    }

    [SerializeField]
    [Tooltip("The GameObject that represents the navigation goal.")]
    GameObject m_goal;

    /// <summary>
    /// The <c>GameObject</c> that represents the navigation goal.
    /// </summary>
    public GameObject goal
    {
        get { return m_goal; }
        set { m_goal = value; }
    }

    [SerializeField]
    [Tooltip("The GameObject that represents a navigation turn instruction.")]
    GameObject m_turn;
    GameObject[] turns = null;

    void SetObjectsActive(bool active) {
        if (m_compass) m_compass.SetActive(active);
        if (m_goal) m_goal.SetActive(active);
        if (turns != null) foreach (GameObject turn in turns) turn.SetActive(active);
    }

    void DestroyTurns() {
        if (turns != null) {
            for (int i = 1; i < turns.Length; ++i) Destroy(turns[i]);
            turns = null;
        }
    }

    void InstantiateTurns() {
        DestroyTurns();
        if (m_turn) {
            m_turn.SetActive(false);
            turns = new GameObject[16]; // pool of 16 maximum turns (pretty optimistic)
            turns[0] = m_turn;
            for (int i = 1; i < turns.Length; ++i) turns[i] = Instantiate(m_turn);
        }
    }

    /// <summary>
    /// The <c>GameObject</c> that represents a navigation turn instruction.
    /// </summary>
    public GameObject turn
    {
        get { return m_turn; }
        set { m_turn = value; InstantiateTurns(); }
    }

    void Awake() {
        m_cameraManager = m_camera.GetComponent<ARCameraManager>();
    }

    void OnEnable() {
        if (manager != null) return;
        manager = new LocationManager();
        manager.GetArIsConverged(); // ensures that the ar session is created
        if (m_wayfinding) {
           manager.StopMonitoringForWayfinding();
           manager.StartMonitoringForWayfinding(m_target);
        }
        InstantiateTurns();
        Application.onBeforeRender += OnBeforeRender;
        RegisterFrameEvent();
    }

    void OnDisable() {
        if (manager == null) return;
        m_cameraManager.frameReceived -= OnArFrame;
        Application.onBeforeRender -= OnBeforeRender;
        manager.StopMonitoringForWayfinding();
        manager.ReleaseArSession();
        manager = null;
        DestroyTurns();
        SetObjectsActive(false);
    }

    void OnBeforeRender() {
        if (manager == null) return;

        manager.SetArCameraToWorldMatrix(m_camera.cameraToWorldMatrix);
        if (!m_wayfinding || !manager.GetArIsConverged() || !IsTracking()) {
            SetObjectsActive(false);
            return;
        }

        Matrix4x4 matrix;
        if (m_compass) {
            if ((matrix = manager.GetArCompassMatrix()) != Matrix4x4.identity) {
                m_compass.transform.rotation = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
                m_compass.transform.position = matrix.GetColumn(3);
                m_compass.SetActive(true);
            } else {
                m_compass.SetActive(false);
            }
        }

        if (m_goal) {
            if ((matrix = manager.GetArGoalMatrix()) != Matrix4x4.identity) {
                m_goal.transform.rotation = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
                m_goal.transform.position = matrix.GetColumn(3);
                m_goal.SetActive(true);
            } else {
                m_goal.SetActive(false);
            }
        }

        if (turns != null) {
           int t = 0;
           int count = manager.GetArTurnCount();
           for (int i = 0; i < count && t < turns.Length; ++i) {
               if ((matrix = manager.GetArTurnMatrix(i)) != Matrix4x4.identity) {
                   turns[t].transform.rotation = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
                   turns[t].transform.position = matrix.GetColumn(3);
                   turns[t].SetActive(true);
                   ++t;
               }
           }
           for (; t < turns.Length; ++t) turns[t].SetActive(false);
        }
    }
}

}
