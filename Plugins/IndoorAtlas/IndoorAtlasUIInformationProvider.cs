using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

namespace IndoorAtlas {

[AddComponentMenu("IndoorAtlas/IndoorAtlas UI Information Provider")]
public class IndoorAtlasUIInformationProvider : MonoBehaviour {
    LocationManager manager = null;
    Region currentVenue = null;
    UnityAction<int> poiAction = null;

    [Header("IndoorAtlas information provider configuration")]

    [SerializeField]
    [Tooltip("The Text widget for traceId.")]
    public Text m_traceId;

    /// <summary>
    /// The <c>Text</c> widget for traceId.
    /// </summary>
    public Text traceId {
        get { return m_traceId; }
        set { m_traceId = value; }
    }

    [SerializeField]
    [Tooltip("The Text widget for current region.")]
    public Text m_region;

    /// <summary>
    /// The <c>Text</c> widget for current region.
    /// </summary>
    public Text region {
        get { return m_region; }
        set { m_region = value; }
    }

    [SerializeField]
    [Tooltip("The AR Wayfinding component for changing wayfinding target.")]
    public IndoorAtlasARWayfinding m_wayfinder;

    /// <summary>
    /// The AR Wayfinding <c>Component</c> for changing wayfinding target.
    /// </summary>
    public IndoorAtlasARWayfinding wayfinder {
        get { return m_wayfinder; }
        set { m_wayfinder = value; }
    }

    [SerializeField]
    [Tooltip("The Dropdown widget for pois.")]
    public Dropdown m_poi;

    void onPoiChanged() {
        if (m_poi && m_wayfinder) {
            if (m_poi.value == 0) {
                m_wayfinder.wayfinding = false;
                return;
            }
            if (currentVenue == null) return;
            m_wayfinder.target = currentVenue.venue.pois[m_poi.value - 1].position;
            m_wayfinder.wayfinding = true;
        }
    }

    /// <summary>
    /// The <c>Dropdown</c> widget for pois.
    /// </summary>
    public Dropdown poi {
        get { return m_poi; }
        set {
            if (poiAction != null) {
                if (m_poi) m_poi.onValueChanged.RemoveListener(poiAction);
            }
            m_poi = value;
            if (m_poi) {
                poiAction = delegate{onPoiChanged();};
                m_poi.onValueChanged.AddListener(poiAction);
            } else if (poiAction != null) {
                poiAction = null;
            }
        }
    }

    void IndoorAtlasOnEnterRegion(Region region) {
        if (m_region) m_region.text = region.name;
        if (region.type == Region.Type.Venue) {
            currentVenue = region;
            if (m_poi) {
                m_poi.ClearOptions();
                List<string> options = new List<string>{"None"};
                foreach (POI poi in currentVenue.venue.pois) options.Add(poi.name);
                m_poi.AddOptions(options);
            }
        }
    }

    void IndoorAtlasOnExitRegion(Region region) {
        if (region.type == Region.Type.FloorPlan && currentVenue != null) {
            if (m_region) m_region.text = currentVenue.name;
        } else if (region.type == Region.Type.Venue) {
            currentVenue = null;
            if (m_poi) {
                m_poi.ClearOptions();
                List<string> options = new List<string>{"None"};
                m_poi.AddOptions(options);
            }
        }
    }

    void UpdateText() {
        if (m_traceId) m_traceId.text = manager.GetTraceId();
    }

    void Awake() {
        if (m_poi) {
            m_poi.ClearOptions();
            List<string> options = new List<string>{"None"};
            m_poi.AddOptions(options);
            if (poiAction != null) m_poi.onValueChanged.RemoveListener(poiAction);
            poiAction = delegate{onPoiChanged();};
            m_poi.onValueChanged.AddListener(poiAction);
        }
    }

    void OnEnable() {
        manager = new LocationManager();
        if (m_traceId) InvokeRepeating("UpdateText", 0.0f, 1.0f);
    }

    void OnDisable() {
        CancelInvoke();
        manager = null;
    }
}

}
