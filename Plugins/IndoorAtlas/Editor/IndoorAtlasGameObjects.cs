using UnityEditor;
using UnityEngine;
using IndoorAtlas;

public class IndoorAtlasGameObjects {
    [MenuItem("GameObject/IndoorAtlas/Session", false, 10)]
    public static void CreateSession(MenuCommand menuCommand) {
        GameObject go = new GameObject("IndoorAtlas Session");
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        go.AddComponent(typeof(IndoorAtlasSession));
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }

    [MenuItem("GameObject/IndoorAtlas/VR Camera", false, 10)]
    public static void CreateVRCamera(MenuCommand menuCommand) {
        GameObject go = new GameObject("IndoorAtlas VR Camera");
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        go.AddComponent(typeof(IndoorAtlasVRCamera));
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }

    [MenuItem("GameObject/IndoorAtlas/AR Wayfinding", false, 10)]
    public static void CreateARSessionOrigin(MenuCommand menuCommand) {
        GameObject go = new GameObject("IndoorAtlas AR Wayfinding");
        go.AddComponent(typeof(IndoorAtlasARWayfinding));
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }

    [MenuItem("GameObject/IndoorAtlas/UI Information Provider", false, 10)]
    public static void CreateTraceIdProvider(MenuCommand menuCommand) {
        GameObject go = new GameObject("IndoorAtlas UI Information Provider");
        go.AddComponent(typeof(IndoorAtlasUIInformationProvider));
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
}
