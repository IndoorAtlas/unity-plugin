using UnityEngine;

namespace IndoorAtlas {

[RequireComponent(typeof(Camera))]
[AddComponentMenu("IndoorAtlas/IndoorAtlas VR Camera")]
public class IndoorAtlasVRCamera : MonoBehaviour {
    Camera m_camera;

    void Awake() {
        m_camera = GetComponent<Camera>();
    }

    void IndoorAtlasOnOrientationChanged(Quaternion orientation) {
        m_camera.transform.rotation = orientation;
    }
}

}
