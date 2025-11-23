#if UNITY_IOS || UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace Monke.Editor
{
    /// <summary>
    /// Automatically configures iOS build settings for AI features
    /// - Adds Speech framework
    /// - Adds AVFoundation framework
    /// - Configures permissions in Info.plist
    /// </summary>
    public class iOSBuildPostProcessor
    {
#if UNITY_IOS
        [PostProcessBuild(1)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
        {
            if (buildTarget != BuildTarget.iOS)
                return;

            Debug.Log("[iOSBuildPostProcessor] Starting iOS post-processing...");

            // Configure Info.plist permissions
            ConfigureInfoPlist(path);

            // Configure Xcode project frameworks
            ConfigureXcodeProject(path);

            Debug.Log("[iOSBuildPostProcessor] iOS post-processing complete!");
        }

        /// <summary>
        /// Add required permissions to Info.plist
        /// </summary>
        private static void ConfigureInfoPlist(string buildPath)
        {
            string plistPath = Path.Combine(buildPath, "Info.plist");

            if (!File.Exists(plistPath))
            {
                Debug.LogError($"[iOSBuildPostProcessor] Info.plist not found at: {plistPath}");
                return;
            }

            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            PlistElementDict rootDict = plist.root;

            // Speech Recognition permission
            rootDict.SetString("NSSpeechRecognitionUsageDescription",
                "Monke needs to understand what you say to have conversations with you!");

            // Microphone permission (may already exist from Unity settings, but ensure it's set)
            if (!rootDict.values.ContainsKey("NSMicrophoneUsageDescription"))
            {
                rootDict.SetString("NSMicrophoneUsageDescription",
                    "Monke needs to hear your voice to talk with you!");
            }

            // Write back to Info.plist
            plist.WriteToFile(plistPath);

            Debug.Log("[iOSBuildPostProcessor] Info.plist permissions configured successfully");
        }

        /// <summary>
        /// Add required frameworks to Xcode project
        /// </summary>
        private static void ConfigureXcodeProject(string buildPath)
        {
            string projectPath = PBXProject.GetPBXProjectPath(buildPath);

            if (!File.Exists(projectPath))
            {
                Debug.LogError($"[iOSBuildPostProcessor] Xcode project not found at: {projectPath}");
                return;
            }

            PBXProject project = new PBXProject();
            project.ReadFromFile(projectPath);

            // Get target GUIDs
            string unityFrameworkTarget = project.GetUnityFrameworkTargetGuid();
            string mainTarget = project.GetUnityMainTargetGuid();

            bool frameworksAdded = false;

            // Add frameworks to UnityFramework target (Unity 2019.3+)
            if (!string.IsNullOrEmpty(unityFrameworkTarget))
            {
                project.AddFrameworkToProject(unityFrameworkTarget, "Speech.framework", false);
                project.AddFrameworkToProject(unityFrameworkTarget, "AVFoundation.framework", false);
                Debug.Log("[iOSBuildPostProcessor] Frameworks added to UnityFramework target");
                frameworksAdded = true;
            }

            // Add frameworks to main target
            if (!string.IsNullOrEmpty(mainTarget))
            {
                project.AddFrameworkToProject(mainTarget, "Speech.framework", false);
                project.AddFrameworkToProject(mainTarget, "AVFoundation.framework", false);
                Debug.Log("[iOSBuildPostProcessor] Frameworks added to Unity-iPhone target");
                frameworksAdded = true;
            }

            // Fallback: Try finding target by name
            if (!frameworksAdded)
            {
                Debug.LogWarning("[iOSBuildPostProcessor] Could not find targets by GUID, trying by name...");

                string iPhoneTarget = project.TargetGuidByName("Unity-iPhone");
                if (!string.IsNullOrEmpty(iPhoneTarget))
                {
                    project.AddFrameworkToProject(iPhoneTarget, "Speech.framework", false);
                    project.AddFrameworkToProject(iPhoneTarget, "AVFoundation.framework", false);
                    Debug.Log("[iOSBuildPostProcessor] Frameworks added to Unity-iPhone target (via name)");
                    frameworksAdded = true;
                }
            }

            if (!frameworksAdded)
            {
                Debug.LogError("[iOSBuildPostProcessor] Failed to add frameworks - no valid target found");
            }

            // Write back to project
            project.WriteToFile(projectPath);

            Debug.Log("[iOSBuildPostProcessor] Xcode project configured successfully");
        }
#endif // UNITY_IOS
    }
}
#endif // UNITY_IOS || UNITY_EDITOR
