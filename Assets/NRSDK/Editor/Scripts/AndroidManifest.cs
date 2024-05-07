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
    using System.Text;
    using System.Xml;


    /// <summary> An android XML document. </summary>
    internal class AndroidXmlDocument : XmlDocument
    {
        /// <summary> Full pathname of the file. </summary>
        protected string m_Path;
        /// <summary> Manager for name space. </summary>
        protected XmlNamespaceManager nameSpaceManager;
        /// <summary> The android XML namespace. </summary>
        public readonly string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";
        /// <summary> The android tools XML namespace. </summary>
        public readonly string AndroidToolsXmlNamespace = "http://schemas.android.com/tools";

        /// <summary> Constructor. </summary>
        /// <param name="path"> Full pathname of the file.</param>
        public AndroidXmlDocument(string path)
        {
            m_Path = path;
            using (var reader = new XmlTextReader(m_Path))
            {
                reader.Read();
                Load(reader);
            }
            nameSpaceManager = new XmlNamespaceManager(NameTable);
            nameSpaceManager.AddNamespace("android", AndroidXmlNamespace);
        }

        /// <summary> Gets the save. </summary>
        /// <returns> A string. </returns>
        public string Save()
        {
            return SaveAs(m_Path);
        }

        /// <summary> Saves as. </summary>
        /// <param name="path"> Full pathname of the file.</param>
        /// <returns> A string. </returns>
        public string SaveAs(string path)
        {
            using (var writer = new XmlTextWriter(path, new UTF8Encoding(false)))
            {
                writer.Formatting = Formatting.Indented;
                Save(writer);
            }
            return path;
        }
    }

    /// <summary> A list of the android. </summary>
    internal class AndroidManifest : AndroidXmlDocument
    {
        /// <summary> Element describing the application. </summary>
        private readonly XmlElement ApplicationElement;

        /// <summary> Constructor. </summary>
        /// <param name="path"> Full pathname of the file.</param>
        public AndroidManifest(string path) : base(path)
        {
            ApplicationElement = SelectSingleNode("/manifest/application") as XmlElement;
        }

        /// <summary> Creates android attribute. </summary>
        /// <param name="key">   The key.</param>
        /// <param name="value"> The value.</param>
        /// <param name="name">  (Optional) The name.</param>
        /// <returns> The new android attribute. </returns>
        private XmlAttribute CreateAndroidAttribute(string key, string value, string name = "android")
        {
            XmlAttribute attr;
            if (name.Equals("tools"))
            {
                attr = CreateAttribute(name, key, AndroidToolsXmlNamespace);
                attr.Value = value;
            }
            else
            {
                attr = CreateAttribute(name, key, AndroidXmlNamespace);
                attr.Value = value;
            }
            return attr;
        }

        /// <summary> Gets activity with launch intent. </summary>
        /// <returns> The activity with launch intent. </returns>
        internal XmlNode GetActivityWithLaunchIntent()
        {
            return SelectSingleNode("/manifest/application/activity[intent-filter/action/@android:name='android.intent.action.MAIN' and " +
                    "intent-filter/category/@android:name='android.intent.category.LAUNCHER']", nameSpaceManager);
        }
        
        /// <summary> Sets external storage. </summary>
        /// <param name="flag"> True to flag.</param>
        internal void SetExternalStorage(bool flag)
        {
            var activity = SelectSingleNode("/manifest/application");
            var rightapplicationData = SelectSingleNode("/manifest/application[@android:requestLegacyExternalStorage='true']", nameSpaceManager);

            if (flag)
            {
                if (rightapplicationData == null)
                {
                    XmlAttribute newAttribute = CreateAndroidAttribute("requestLegacyExternalStorage", "true");
                    activity.Attributes.Append(newAttribute);
                }
            }
            else
            {
                if (rightapplicationData != null)
                {
                    activity.Attributes.RemoveNamedItem("android:requestLegacyExternalStorage");
                }
            }
        }

        /// <summary> Sets camera permission. </summary>
        internal void SetCameraPermission()
        {
            var manifest = SelectSingleNode("/manifest");
            if (!manifest.InnerXml.Contains("android.permission.CAMERA"))
            {
                XmlElement child = CreateElement("uses-permission");
                manifest.AppendChild(child);
                XmlAttribute newAttribute = CreateAndroidAttribute("name", "android.permission.CAMERA");
                child.Attributes.Append(newAttribute);
            }
            //else
            //{
            //    NRDebugger.Info("Already has the camera permission.");
            //}
        }

        internal void SetPackageReadPermission()
        {
            var manifest = SelectSingleNode("/manifest");
            if (!manifest.InnerXml.Contains("android.permission.QUERY_ALL_PACKAGES"))
            {
                XmlElement child = CreateElement("uses-permission");
                manifest.AppendChild(child);
                XmlAttribute newAttribute = CreateAndroidAttribute("name", "android.permission.QUERY_ALL_PACKAGES");
                child.Attributes.Append(newAttribute);
            }
            //else
            //{
            //    NRDebugger.Info("Already has the permission of 'android.permission.QUERY_ALL_PACKAGES'.");
            //}
        }

        /// <summary> Sets blue tooth permission. </summary>
        internal void SetBlueToothPermission()
        {
            var manifest = SelectSingleNode("/manifest");
            if (!manifest.InnerXml.Contains("android.permission.BLUETOOTH"))
            {
                XmlElement child = CreateElement("uses-permission");
                manifest.AppendChild(child);
                XmlAttribute newAttribute = CreateAndroidAttribute("name", "android.permission.BLUETOOTH");
                child.Attributes.Append(newAttribute);
                // newAttribute = CreateAndroidAttribute("name", "android.permission.BLUETOOTH_ADMIN");
                // child.Attributes.Append(newAttribute);
            }
            //else
            //{
            //    NRDebugger.Info("Already has the bluetooth permission.");
            //}
        }

        internal void SetAudioRecordPermission()
        {
            var manifest = SelectSingleNode("/manifest");
            if (!manifest.InnerXml.Contains("android.permission.RECORD_AUDIO"))
            {
                XmlElement child = CreateElement("uses-permission");
                manifest.AppendChild(child);
                XmlAttribute newAttribute = CreateAndroidAttribute("name", "android.permission.RECORD_AUDIO");
                child.Attributes.Append(newAttribute);
            }
        }

        /// <summary> Sets sdk meta data. </summary>
        internal void SetSDKMetaData()
        {
            var activity = SelectSingleNode("/manifest/application");

            // metadata for "nreal_sdk
            var newMetaNRSDK = SelectSingleNode("/manifest/application/meta-data[@android:name='nreal_sdk' and " +
                    "@android:value='true']", nameSpaceManager);
            var oldMetaNRSDK = SelectSingleNode("/manifest/application/meta-data[@android:name='nreal_sdk']", nameSpaceManager);
            if (newMetaNRSDK == null)
            {
                if (oldMetaNRSDK != null)
                {
                    activity.RemoveChild(oldMetaNRSDK);
                }
                XmlElement child = CreateElement("meta-data");
                activity.AppendChild(child);

                XmlAttribute newAttribute = CreateAndroidAttribute("name", "nreal_sdk");
                child.Attributes.Append(newAttribute);
                newAttribute = CreateAndroidAttribute("value", "true");
                child.Attributes.Append(newAttribute);
            }

            // metadata for "com.nreal.supportDevices"
            string supportDevices = NRProjectConfigHelper.GetProjectConfig().GetTargetDeviceTypesDesc();
            var newMetaDevices = SelectSingleNode("/manifest/application/meta-data[@android:name='com.nreal.supportDevices' and " +
                    "@android:value='']", nameSpaceManager);
            var oldMetaDevices = SelectSingleNode("/manifest/application/meta-data[@android:name='com.nreal.supportDevices']", nameSpaceManager);
            if (oldMetaDevices != null)
                activity.RemoveChild(oldMetaDevices);
            if (newMetaDevices == null)
            {
                XmlElement child = CreateElement("meta-data");
                activity.AppendChild(child);

                XmlAttribute newAttribute = CreateAndroidAttribute("name", "com.nreal.supportDevices");
                child.Attributes.Append(newAttribute);
                newAttribute = CreateAndroidAttribute("value", supportDevices);
                child.Attributes.Append(newAttribute);
            }
        }

        /// <summary> Refresh the action.main action:
        /// while on multi-resume mode, the main and launcher intent is removed from unityPlayerActivity;
        /// while on none multi-resume mode, the main and launcher intent is added to unityPlayerActivity. </summary>
        internal void RefreshActivityMainAction(bool supportMultiResume)
        {
            var activityNode = GetActivityWithLaunchIntent();
            if (activityNode == null)
            {
                activityNode = SelectSingleNode("/manifest/application/activity[@android:name='com.unity3d.player.UnityPlayerActivity']", nameSpaceManager);
            }

            if (activityNode == null)
                return;
            
            // Create or modify intent-filter
            var intentFilterNode = activityNode.SelectSingleNode("intent-filter", nameSpaceManager);
            if (supportMultiResume)
            {
                if (intentFilterNode == null)
                    return;

                var actionMainNode = intentFilterNode.SelectSingleNode("action[@android:name='android.intent.action.MAIN']", nameSpaceManager);
                if (actionMainNode != null)
                    intentFilterNode.RemoveChild(actionMainNode);
                
                var launcherNode = intentFilterNode.SelectSingleNode("category[@android:name='android.intent.category.LAUNCHER']", nameSpaceManager);
                if (launcherNode != null)
                    intentFilterNode.RemoveChild(launcherNode);
                if (!intentFilterNode.HasChildNodes)
                    activityNode.RemoveChild(intentFilterNode);
            }
            else
            {
                if (intentFilterNode == null)
                {
                    // If intent-filter does not exist, create a new one
                    intentFilterNode = CreateElement("intent-filter");
                    activityNode.AppendChild(intentFilterNode);
                }

                var actionMainNode = intentFilterNode.SelectSingleNode("action[@android:name='android.intent.action.MAIN']", nameSpaceManager);
                if (actionMainNode == null)
                {
                    // If main action does not exist, create a new one
                    XmlElement newAction = CreateElement("action");
                    XmlAttribute newActionAttr = CreateAndroidAttribute("name", "android.intent.action.MAIN");
                    newAction.Attributes.Append(newActionAttr);
                    intentFilterNode.AppendChild(newAction);
                }
                
                var launcherNode = intentFilterNode.SelectSingleNode("category[@android:name='android.intent.category.LAUNCHER']", nameSpaceManager);
                if (launcherNode == null)
                {
                    // If luancher category does not exist, create a new one
                    XmlElement newCategory = CreateElement("category");
                    XmlAttribute newLauncherAttr = CreateAndroidAttribute("name", "android.intent.category.LAUNCHER");
                    newCategory.Attributes.Append(newLauncherAttr);
                    intentFilterNode.AppendChild(newCategory);
                }
            }
        }
    }

}
