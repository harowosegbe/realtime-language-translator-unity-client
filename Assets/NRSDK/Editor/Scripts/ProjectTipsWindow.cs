/****************************************************************************
* Copyright 2019 Xreal Techonology Limited.All rights reserved.
*
* This file is part of NRSDK.
*
* https://www.xreal.com/        
*
*****************************************************************************/

namespace NRKernal
{
    using UnityEditor;
    using UnityEngine;
    using System.IO;
    using UnityEngine.Rendering;
    using System.Collections.Generic;
    using NRKernal.Release;
    using LitJson;
    using System.Linq;
    using System;
    using System.Text;

    /// <summary> Form for viewing the project tips. </summary>
    [InitializeOnLoad]
    public class ProjectTipsWindow : EditorWindow
    {
        /// <summary> A check. </summary>
        private abstract class Check
        {
            /// <summary> The key. </summary>
            protected string _key;
            protected MessageType _level;
            public MessageType level
            {
                get
                {
                    return _level;
                }
            }

            public Check(MessageType level)
            {
                _level = level;
            }

            /// <summary> Ignores this object. </summary>
            public void Ignore()
            {
                EditorPrefs.SetBool(ignorePrefix + _key, true);
            }

            /// <summary> Query if this object is ignored. </summary>
            /// <returns> True if ignored, false if not. </returns>
            public bool IsIgnored()
            {
                return EditorPrefs.HasKey(ignorePrefix + _key);
            }

            /// <summary> Deletes the ignore. </summary>
            public void DeleteIgnore()
            {
                EditorPrefs.DeleteKey(ignorePrefix + _key);
            }

            /// <summary> Query if this object is valid. </summary>
            /// <returns> True if valid, false if not. </returns>
            public abstract bool IsValid();

            /// <summary> Draw graphical user interface. </summary>
            public abstract void DrawGUI();

            /// <summary> Query if this object is fixable. </summary>
            /// <returns> True if fixable, false if not. </returns>
            public abstract bool IsFixable();

            /// <summary> Fixes this object. </summary>
            public abstract void Fix();

            protected void DrawContent(string title, string message)
            {
                EditorGUILayout.HelpBox(title, level);
                EditorGUILayout.LabelField(message, EditorStyles.textArea);
            }
        }

        /// <summary> A ckeck for buildTarget . </summary>
        private class CkeckBuildTargetAndroid : Check
        {
            /// <summary> Default constructor. </summary>
            public CkeckBuildTargetAndroid(MessageType level) : base(level)
            {
                _key = this.GetType().Name;
            }

            /// <summary> Query if this object is valid. </summary>
            /// <returns> True if valid, false if not. </returns>
            public override bool IsValid()
            {
                return EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
            }

            /// <summary> Draw graphical user interface. </summary>
            public override void DrawGUI()
            {
                string message = @"In order to develop on NRSDK, BuildTarget must be set to Android. 
in panel of Player Settings, choose 'Android' in platform list, and click 'Switch Platform' button.";
                DrawContent("BuildTarget is Android", message);
            }

            /// <summary> Query if this object is fixable. </summary>
            /// <returns> True if fixable, false if not. </returns>
            public override bool IsFixable()
            {
                return true;
            }

            /// <summary> Fixes this object. </summary>
            public override void Fix()
            {
                if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                }
            }
        }  /// <summary> A ckeck for Scripting Backend . </summary>
        private class CkeckBuildScriptingBackend : Check
        {
            /// <summary> Default constructor. </summary>
            public CkeckBuildScriptingBackend(MessageType level) : base(level)
            {
                _key = this.GetType().Name;
            }

            /// <summary> Query if this object is valid. </summary>
            /// <returns> True if valid, false if not. </returns>
            public override bool IsValid()
            {
                return PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP;
            }

            /// <summary> Draw graphical user interface. </summary>
            public override void DrawGUI()
            {
                string message = @"In order to develop on NRSDK, Scripting Backend must be set to IL2CPP. 
in panel of 'Project Settings', click 'Player'->'Other Settings'->'Scripting Backend', select 'IL2CPP'.";
                DrawContent("Scripting Backend is IL2CPP", message);
            }

            /// <summary> Query if this object is fixable. </summary>
            /// <returns> True if fixable, false if not. </returns>
            public override bool IsFixable()
            {
                return true;
            }

            /// <summary> Fixes this object. </summary>
            public override void Fix()
            {
                if(PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) != ScriptingImplementation.IL2CPP)
                {
                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                }
            }
        }  /// <summary> A ckeck for Target Architectures . </summary>
        private class CkeckBuildTargetArchitectures : Check
        {
            /// <summary> Default constructor. </summary>
            public CkeckBuildTargetArchitectures(MessageType level) : base(level)
            {
                _key = this.GetType().Name;
            }

            /// <summary> Query if this object is valid. </summary>
            /// <returns> True if valid, false if not. </returns>
            public override bool IsValid()
            {
                return PlayerSettings.Android.targetArchitectures == AndroidArchitecture.ARM64;
            }

            /// <summary> Draw graphical user interface. </summary>
            public override void DrawGUI()
            {
                string message = @"In order to develop on NRSDK, Target Architectures must be set to ARM64. 
in panel of 'Project Settings', click 'Player'->'Other Settings'->'Target Architectures', select 'ARM64'.";
                DrawContent("Target Architectures is ARM64", message);
            }

            /// <summary> Query if this object is fixable. </summary>
            /// <returns> True if fixable, false if not. </returns>
            public override bool IsFixable()
            {
                return PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP;
            }

            /// <summary> Fixes this object. </summary>
            public override void Fix()
            {
                if (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
                {
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                }
            }
        }

        /// <summary> A ckeck android vsyn. </summary>
        private class CkeckAndroidVsyn : Check
        {
            /// <summary> Default constructor. </summary>
            public CkeckAndroidVsyn(MessageType level) : base(level)
            {
                _key = this.GetType().Name;
            }

            /// <summary> Query if this object is valid. </summary>
            /// <returns> True if valid, false if not. </returns>
            public override bool IsValid()
            {
                return QualitySettings.vSyncCount == 0;
            }

            /// <summary> Draw graphical user interface. </summary>
            public override void DrawGUI()
            {
                string message = @"In order to render correct on mobile devices, the vSyn in quality settings must be disabled. 
in dropdown list of Quality Settings > V Sync Count, choose 'Dont't Sync' for all levels.";
                DrawContent("vSyn is opened on Mobile Devices", message);
            }

            /// <summary> Query if this object is fixable. </summary>
            /// <returns> True if fixable, false if not. </returns>
            public override bool IsFixable()
            {
                return true;
            }

            /// <summary> Fixes this object. </summary>
            public override void Fix()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ||
                    EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
                {

                    QualitySettings.vSyncCount = 0;
                }
            }
        }

        /// <summary> Ckeck android SD card permission descriptor. </summary>
        private class CkeckAndroidSDCardPermission : Check
        {
            /// <summary> Default constructor. </summary>
            public CkeckAndroidSDCardPermission(MessageType level) : base(level)
            {
                _key = this.GetType().Name;
            }

            /// <summary> Query if this object is valid. </summary>
            /// <returns> True if valid, false if not. </returns>
            public override bool IsValid()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    return PlayerSettings.Android.forceSDCardPermission;
                }
                else
                {
                    return false;
                }
            }

            /// <summary> Draw graphical user interface. </summary>
            public override void DrawGUI()
            {
                string message = @"In order to run correct on mobile devices, the sdcard write permission should be set. 
in dropdown list of Player Settings > Other Settings > Write Permission, choose 'External(SDCard)'.";
                DrawContent("Sdcard permission not available", message);
            }

            /// <summary> Query if this object is fixable. </summary>
            /// <returns> True if fixable, false if not. </returns>
            public override bool IsFixable()
            {
                return true;
            }

            /// <summary> Fixes this object. </summary>
            public override void Fix()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    PlayerSettings.Android.forceSDCardPermission = true;
                }
            }
        }

        /// <summary> Android minSdkVersion should be higher than  29. </summary>
        private class CkeckAndroidMinAPILevel : Check
        {
            public CkeckAndroidMinAPILevel(MessageType level) : base(level)
            {
                _key = this.GetType().Name;
            }

            public override bool IsValid()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    return PlayerSettings.Android.minSdkVersion >= AndroidSdkVersions.AndroidApiLevel29 ||
                        PlayerSettings.Android.minSdkVersion == AndroidSdkVersions.AndroidApiLevelAuto;
                }
                else
                {
                    return false;
                }
            }

            /// <summary> Draw graphical user interface. </summary>
            public override void DrawGUI()
            {
                string message = @"In order to run correct on mobile devices, Android minSdkVersion should be higher than 29.";
                DrawContent("Android minSdkVersion should be higher than 29", message);
            }

            /// <summary> Query if this object is fixable. </summary>
            /// <returns> True if fixable, false if not. </returns>
            public override bool IsFixable()
            {
                return true;
            }

            /// <summary> Fixes this object. </summary>
            public override void Fix()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
                }
            }
        }

        /// <summary> A ckeck android orientation. </summary>
        private class CkeckAndroidOrientation : Check
        {
            /// <summary> Default constructor. </summary>
            public CkeckAndroidOrientation(MessageType level) : base(level)
            {
                _key = this.GetType().Name;
            }

            /// <summary> Query if this object is valid. </summary>
            /// <returns> True if valid, false if not. </returns>
            public override bool IsValid()
            {
                return PlayerSettings.defaultInterfaceOrientation == UIOrientation.Portrait;
            }

            /// <summary> Draw graphical user interface. </summary>
            public override void DrawGUI()
            {
                string message = @"In order to display correct on mobile devices, the orientation should be set to portrait. 
in dropdown list of Player Settings > Resolution and Presentation > Default Orientation, choose 'Portrait'.";
                DrawContent("Orientation is not portrait", message);
            }

            /// <summary> Query if this object is fixable. </summary>
            /// <returns> True if fixable, false if not. </returns>
            public override bool IsFixable()
            {
                return true;
            }

            /// <summary> Fixes this object. </summary>
            public override void Fix()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
                }
            }
        }

        /// <summary> A ckeck android graphics a pi. </summary>
        private class CkeckAndroidGraphicsAPI : Check
        {
            /// <summary> Default constructor. </summary>
            public CkeckAndroidGraphicsAPI(MessageType level) : base(level)
            {
                _key = this.GetType().Name;
            }

            /// <summary> Query if this object is valid. </summary>
            /// <returns> True if valid, false if not. </returns>
            public override bool IsValid()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    bool autoGraphicsAPI = PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android);
                    var graphics = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
                    if (!autoGraphicsAPI && graphics != null && graphics.Length == 1 &&
                        graphics[0] == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3)
                    {
                        return true;
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }

            /// <summary> Draw graphical user interface. </summary>
            public override void DrawGUI()
            {
                string message = @"In order to render correct on mobile devices, the graphicsAPIs should be set to OpenGLES3. 
in dropdown list of Player Settings > Other Settings > Graphics APIs , choose 'OpenGLES3'.";
                DrawContent("GraphicsAPIs is not OpenGLES3", message);
            }

            /// <summary> Query if this object is fixable. </summary>
            /// <returns> True if fixable, false if not. </returns>
            public override bool IsFixable()
            {
                return true;
            }

            /// <summary> Fixes this object. </summary>
            public override void Fix()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
                    PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new GraphicsDeviceType[1] { GraphicsDeviceType.OpenGLES3 });
                }
            }
        }

        /// <summary> A ckeck color space. </summary>
        private class CkeckColorSpace : Check
        {
            /// <summary> Default constructor. </summary>
            public CkeckColorSpace(MessageType level) : base(level)
            {
                _key = this.GetType().Name;
            }

            /// <summary> Query if this object is valid. </summary>
            /// <returns> True if valid, false if not. </returns>
            public override bool IsValid()
            {
                return PlayerSettings.colorSpace == ColorSpace.Linear;
            }

            /// <summary> Draw graphical user interface. </summary>
            public override void DrawGUI()
            {
                string message = @"In order to display correct on mobile devices, the colorSpace should be set to linear. 
in dropdown list of Player Settings > Other Settings > Color Space, choose 'Linear'.";
                DrawContent("ColorSpace is not Linear", message);
            }

            /// <summary> Query if this object is fixable. </summary>
            /// <returns> True if fixable, false if not. </returns>
            public override bool IsFixable()
            {
                return true;
            }

            /// <summary> Fixes this object. </summary>
            public override void Fix()
            {
                if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                {
                    PlayerSettings.colorSpace = ColorSpace.Linear;
                }
            }
        }

        /// <summary> A ckeck that android build system is gradle. </summary>
        private class CkeckAndroidBuildGradle: Check
        {
            /// <summary> Default constructor. </summary>
            public CkeckAndroidBuildGradle(MessageType level) : base(level)
            {
                _key = this.GetType().Name;
            }

            /// <summary> Query if this object is valid. </summary>
            /// <returns> True if valid, false if not. </returns>
            public override bool IsValid()
            {
                return EditorUserBuildSettings.androidBuildSystem == AndroidBuildSystem.Gradle;
            }

            /// <summary> Draw graphical user interface. </summary>
            public override void DrawGUI()
            {
                string message = @"";
                DrawContent("Gradle plugin is valid", message);
            }

            /// <summary> Query if this object is fixable. </summary>
            /// <returns> True if fixable, false if not. </returns>
            public override bool IsFixable()
            {
                return true;
            }

            /// <summary> Fixes this object. </summary>
            public override void Fix()
            {
                EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            }
        }

#if UNITY_EDITOR_OSX
        private class CheckImageTrackingCliExecutePermission : Check
        {
            private string m_CliBinaryPath = null;
            string CliBinaryPath
            {
                get
                {
                    if (m_CliBinaryPath == null)
                    {
                        NRTrackingImageDatabase.FindCliBinaryPath(out m_CliBinaryPath);
                    }
                    return m_CliBinaryPath;
                }
            }
            public CheckImageTrackingCliExecutePermission(MessageType level): base(level)
            {
                _key = GetType().Name;
            }

            public override void DrawGUI()
            {
                DrawContent("ImageTrackingCli", "Has no execute permission");
            }

            public override void Fix()
            {
                ShellHelper.RunCommand("chmod", $"755 {CliBinaryPath}");
            }

            public override bool IsFixable()
            {
                return true;
            }

            public override bool IsValid()
            {
                ShellHelper.RunCommand("ls", $"-l {CliBinaryPath}", out string output, out string error);
                return output.Contains("-rwxr-xr-x");
            }
        }
#endif

        /// <summary> The checks. </summary>
        private static Check[] checks = new Check[]
        {
            new CkeckBuildTargetAndroid(MessageType.Error),
            new CkeckAndroidVsyn(MessageType.Error),
            new CkeckAndroidMinAPILevel(MessageType.Error),
            new CkeckBuildScriptingBackend(MessageType.Error),
            new CkeckBuildTargetArchitectures(MessageType.Error),                
            //new CkeckAndroidSDCardPermission(),
            new CkeckAndroidOrientation(MessageType.Warning),
            new CkeckAndroidGraphicsAPI(MessageType.Error),
            new CkeckAndroidBuildGradle(MessageType.Error),
            //new CkeckColorSpace(Level.Error),
#if UNITY_EDITOR_OSX
            new CheckImageTrackingCliExecutePermission(MessageType.Error),
#endif
        };

        /// <summary> The window. </summary>
        private static ProjectTipsWindow m_Window;
        /// <summary> The scroll position. </summary>
        private Vector2 m_ScrollPosition;
        /// <summary> The ignore prefix. </summary>
        private const string ignorePrefix = "NRKernal.ignore";

        static ProjectTipsWindow()
        {
            EditorApplication.update += Update;
        }

        /// <summary> Shows the window. </summary>
        [MenuItem("NRSDK/Project Tips", false, 50)]
        public static void ShowWindow()
        {
            m_Window = GetWindow<ProjectTipsWindow>(true);
            m_Window.minSize = new Vector2(320, 300);
            m_Window.maxSize = new Vector2(320, 800);
            m_Window.titleContent = new GUIContent("NRSDK | Project Tips");
        }

        /// <summary> Updates this object. </summary>
        private static void Update()
        {
            bool show = false;

            foreach (Check check in checks)
            {
                if (!check.IsIgnored() && !check.IsValid() && check.level > MessageType.Warning)
                {
                    show = true;
                }
            }

            if (show)
            {
                ShowWindow();
            }

            EditorApplication.update -= Update;
        }

        /// <summary> Executes the 'graphical user interface' action. </summary>
        public void OnGUI()
        {
            var resourcePath = GetResourcePath();
            var logo = AssetDatabase.LoadAssetAtPath<Texture2D>(resourcePath + "icon.png");
            var rect = GUILayoutUtility.GetRect(position.width, 80, GUI.skin.box);
            GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);

            string aboutText = "This window provides tips to help fix common issues with the NRSDK and your project.";
            EditorGUILayout.LabelField(aboutText, EditorStyles.textArea);

            int ignoredCount = 0;
            int fixableCount = 0;
            int invalidNotIgnored = 0;

            for (int i = 0; i < checks.Length; i++)
            {
                Check check = checks[i];

                bool ignored = check.IsIgnored();
                bool valid = check.IsValid();
                bool fixable = check.IsFixable();

                if (!valid && !ignored && fixable)
                {
                    fixableCount++;
                }

                if (!valid && !ignored)
                {
                    invalidNotIgnored++;
                }

                if (ignored)
                {
                    ignoredCount++;
                }
            }

            Rect issuesRect = EditorGUILayout.GetControlRect();
            GUI.Box(new Rect(issuesRect.x - 4, issuesRect.y, issuesRect.width + 8, issuesRect.height), "Tips", EditorStyles.toolbarButton);

            if (invalidNotIgnored > 0)
            {
                m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
                {
                    for (int i = 0; i < checks.Length; i++)
                    {
                        Check check = checks[i];

                        if (!check.IsIgnored() && !check.IsValid())
                        {
                            invalidNotIgnored++;

                            GUILayout.BeginVertical("box");
                            {
                                check.DrawGUI();

                                EditorGUILayout.BeginHorizontal();
                                {
                                    // Aligns buttons to the right
                                    GUILayout.FlexibleSpace();

                                    if (check.IsFixable())
                                    {
                                        if (GUILayout.Button("Fix"))
                                            check.Fix();
                                    }

                                    //if (GUILayout.Button("Ignore"))
                                    //    check.Ignore();
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            GUILayout.EndVertical();
                        }
                    }
                }
                GUILayout.EndScrollView();
            }

            GUILayout.FlexibleSpace();

            if (invalidNotIgnored == 0)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();

                    GUILayout.BeginVertical();
                    {
                        GUILayout.Label("No issues found");

                        if (GUILayout.Button("Close Window"))
                            Close();
                    }
                    GUILayout.EndVertical();

                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.BeginHorizontal("box");
            {
                if (fixableCount > 0)
                {
                    if (GUILayout.Button("Accept All"))
                    {
                        if (EditorUtility.DisplayDialog("Accept All", "Are you sure?", "Yes, Accept All", "Cancel"))
                        {
                            for (int i = 0; i < checks.Length; i++)
                            {
                                Check check = checks[i];

                                if (!check.IsIgnored() &&
                                    !check.IsValid())
                                {
                                    if (check.IsFixable())
                                        check.Fix();
                                }
                            }
                        }
                    }
                }

                //if (invalidNotIgnored > 0)
                //{
                //    if (GUILayout.Button("Ignore All"))
                //    {
                //        if (EditorUtility.DisplayDialog("Ignore All", "Are you sure?", "Yes, Ignore All", "Cancel"))
                //        {
                //            for (int i = 0; i < checks.Length; i++)
                //            {
                //                Check check = checks[i];

                //                if (!check.IsIgnored())
                //                    check.Ignore();
                //            }
                //        }
                //    }
                //}

                //if (ignoredCount > 0)
                //{
                //    if (GUILayout.Button("Show Ignored"))
                //    {
                //        foreach (Check check in checks)
                //            check.DeleteIgnore();
                //    }
                //}
            }
            GUILayout.EndHorizontal();
        }

        /// <summary> Gets resource path. </summary>
        /// <returns> The resource path. </returns>
        private string GetResourcePath()
        {
            var ms = MonoScript.FromScriptableObject(this);
            var path = AssetDatabase.GetAssetPath(ms);
            path = Path.GetDirectoryName(path);
            return path.Substring(0, path.Length - "Editor".Length - 1) + "Textures/";
        }
    }
}
