// #define IA_VERBOSE
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;

// Optional utility class which provides automation mechanisms to import IndoorAtlas SDK
// to the Unity generated Xcode project in the post build phase. The tasks are:
// - Disabling bitcode
// - Adding IndoorAtlas SDK dependency
// - Add NSLocationAlwaysUsageDescription and NSLocationWhenInUseUsageDescription
// Obviously each step could be done manually in the generated Xcode project file so
// this script should be considered as a helper utility.
public class XcodeFixes {
	[PostProcessBuildAttribute(1)]
	public int callbackOrder { get { return 999; } }

	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget target, string path)
	{
		if (target == BuildTarget.iOS) {
			string projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
			PBXProject proj = new PBXProject();
			proj.ReadFromString(File.ReadAllText(projectPath));

			// Disable bitcode
			UnityEngine.Debug.Log("Bitcode will be disabled. IndoorAtlas.framework uses processor optimized assembly functions, so it is not possible to enable Bitcode.");
			string targetGUID = proj.TargetGuidByName("Unity-iPhone");
			proj.AddBuildProperty(targetGUID, "ENABLE_BITCODE", "false"); 
			File.WriteAllText(projectPath, proj.WriteToString());

			// Add NSLocationAlwaysUsageDescription and NSLocationWhenInUseUsageDescription
			string plistPath = path + "/Info.plist";
			string locationUsageDescription = "IndoorAtlas demo project needs access to device location";
			PlistDocument plist = new PlistDocument();
			plist.ReadFromString(File.ReadAllText(plistPath));
			plist.root.CreateArray ("NSLocationAlwaysUsageDescription").AddString (locationUsageDescription);
			plist.root.CreateArray ("NSLocationWhenInUseUsageDescription").AddString (locationUsageDescription);
			File.WriteAllText(plistPath, plist.WriteToString());

			// Heuristic cocoapods search
			List<string> paths = new List<string>(System.Environment.GetEnvironmentVariable ("PATH").Split(':'));
			paths.Add ("/usr/local/bin/");
			string pod = paths.Select(x => Path.Combine(x, "pod"))
				.Where(x => File.Exists(x))
				.FirstOrDefault();
			if (pod == null) {
				UnityEngine.Debug.LogError ("Cocoapods not found. Make sure cocoapods is installed and/or consider adding IndoorAtlas SDK dependency manually to the generated XCode project.");
				return;
			}
			#if IA_VERBOSE
			else {
				UnityEngine.Debug.Log ("Cocoapods found at " + pod);
			}
			#endif

			// Run cocoapods to add IndoorAtlas SDK dependencies
			// Overwrites if needed Podfile in the project path (otherwise Build&Run woudln't work).
			File.Copy ("Assets/Plugins/iOS/Podfile", path + "/Podfile", true);
			using (Process cocoapod = new Process ()) {
				cocoapod.StartInfo.FileName = pod;
				cocoapod.StartInfo.Arguments = "install";
				cocoapod.StartInfo.CreateNoWindow = true;
				cocoapod.StartInfo.UseShellExecute = false;
				cocoapod.StartInfo.WorkingDirectory = path;
				cocoapod.StartInfo.RedirectStandardOutput = true;
				cocoapod.StartInfo.RedirectStandardError = true;
				cocoapod.Start ();
				cocoapod.WaitForExit ();
				if (cocoapod.ExitCode != 0) {
					UnityEngine.Debug.Log("Cocoapods out: " + cocoapod.StandardOutput.ReadToEnd());
					UnityEngine.Debug.Log("Cocoapods err: " + cocoapod.StandardError.ReadToEnd());
					UnityEngine.Debug.LogError ("Failed running cocoapods. Make sure cocoapods is installed and you target to iOS 8.0 or newer.");
				}
			}
		}
	}
}

#endif
