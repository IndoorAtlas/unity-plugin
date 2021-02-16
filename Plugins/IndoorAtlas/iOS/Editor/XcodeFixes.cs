#if UNITY_IOS
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;

// Utility class which provides automation mechanisms to import IndoorAtlas SDK
// to the Unity generated Xcode project in the post build phase. The tasks are:
// - Disabling bitcode
// - Adding IndoorAtlas SDK dependency
// - Add the neccessary plist keys
public class XcodeFixes {
    [PostProcessBuildAttribute(1)]
    public int callbackOrder { get { return 999; } }

    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string path)
    {
        if (target != BuildTarget.iOS) return;

        string projectPath = PBXProject.GetPBXProjectPath(path);
        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projectPath));

        // Disable bitcode
        UnityEngine.Debug.Log("Bitcode will be disabled. IndoorAtlas.framework uses processor optimized assembly functions, so it is not possible to enable Bitcode.");
#if UNITY_2019_3_OR_NEWER
        string mainGUID = proj.GetUnityMainTargetGuid();
        proj.AddBuildProperty(proj.GetUnityFrameworkTargetGuid(), "ENABLE_BITCODE", "false");
        proj.AddFrameworkToProject(proj.GetUnityFrameworkTargetGuid(), "CoreLocation.framework", false);
#else
        string mainGUID = proj.TargetGuidByName(PBXProject.GetUnityTargetName());
        proj.AddBuildProperty(proj.TargetGuidByName("UnityFramework"), "ENABLE_BITCODE", "false");
        proj.AddFrameworkToProject(proj.TargetGuidByName("UnityFramework"), "CoreLocation.framework", false);
#endif
        proj.AddBuildProperty(mainGUID, "ENABLE_BITCODE", "false");

        // Add IndoorAtlas.framework
        // proj.AddFrameworkToProject(mainGUID, "Plugins/IndoorAtlas/iOS/IndoorAtlas.framework", false);
        string frameworkGUID = proj.FindFileGuidByProjectPath("Frameworks/Plugins/IndoorAtlas/iOS/IndoorAtlas.framework");
        PBXProjectExtensions.AddFileToEmbedFrameworks(proj, mainGUID, frameworkGUID);
        proj.WriteToFile(projectPath);

        // Add NSLocationAlwaysUsageDescription and NSLocationWhenInUseUsageDescription
        string plistPath = path + "/Info.plist";
        string locationUsageDescription = "IndoorAtlas demo project needs access to device location";
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));
        plist.root.SetString("NSLocationAlwaysUsageDescription", locationUsageDescription);
        plist.root.SetString("NSLocationWhenInUseUsageDescription", locationUsageDescription);
        plist.root.SetString("NSLocationAlwaysAndWhenInUseUsageDescription", locationUsageDescription);
        plist.root.SetString("NSBluetoothAlwaysUsageDescription", "Needed for accurate positioning");
        plist.root.SetString("NSBluetoothPeripheralUsageDescription", "Needed for accurate positioning");
        plist.root.SetString("NSMotionUsageDescription", "Needed for accurate positioning");
        File.WriteAllText(plistPath, plist.WriteToString());
    }
}
#endif
