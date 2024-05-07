/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

namespace NRKernal
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    public partial class NativeConfiguration
    {
        /// <summary> The native interface. </summary>
        private NativeInterface m_NativeInterface;
        /// <summary> Dictionary of trackable image databases. </summary>
        private Dictionary<string, UInt64> m_TrackableImageDatabaseDict;
        /// <summary> The native tracking handle. </summary>
        public UInt64 PerceptionHandle
        {
            get
            {
                return m_NativeInterface.PerceptionHandle;
            }
        }

        /// <summary> Handle of the configuration. </summary>
        private UInt64 m_ConfigHandle = 0;
        /// <summary> Handle of the database. </summary>
        private UInt64 m_DatabaseHandle = 0;

        /// <summary> The last session configuration. </summary>
        private NRSessionConfig m_LastSessionConfig;
        /// <summary> The native trackable image. </summary>
        private NativeTrackableImage m_NativeTrackableImage;
        /// <summary> True if is update configuration lock, false if not. </summary>
        private bool m_IsUpdateConfigLock = false;

        /// <summary> Constructor. </summary>
        /// <param name="nativeInterface"> The native interface.</param>
        public NativeConfiguration(NativeInterface nativeInterface)
        {
            m_NativeInterface = nativeInterface;
            m_NativeInterface.OnPerceptionHandleChanged += UpdateConfigHandle;
            m_LastSessionConfig = NRSessionConfig.CreateInstance(typeof(NRSessionConfig)) as NRSessionConfig;
            m_NativeTrackableImage = m_NativeInterface.NativeTrackableImage;
            m_TrackableImageDatabaseDict = new Dictionary<string, ulong>();
        }

        private async void UpdateConfigHandle()
        {
            if (m_ConfigHandle != 0)
            {
                UInt64 oldConfigHandle = m_ConfigHandle;
                m_ConfigHandle = Create();
                NRDebugger.Info($"[NativeConfigration] UpdateConfigHandle old={oldConfigHandle} new={m_ConfigHandle}");
                m_IsUpdateConfigLock = true;
                SetPlaneFindMode(m_ConfigHandle, m_LastSessionConfig.PlaneFindingMode);
                m_TrackableImageDatabaseDict.Clear();
                await UpdateImageTrackingConfig(m_LastSessionConfig);
                m_IsUpdateConfigLock = false;
            }
        }

        /// <summary> Updates the configuration described by config. </summary>
        /// <param name="config"> The configuration.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public async Task<bool> UpdateConfig(NRSessionConfig config)
        {
            if (m_IsUpdateConfigLock)
            {
                return false;
            }
            m_IsUpdateConfigLock = true;
            if (m_ConfigHandle == 0)
            {
                m_ConfigHandle = this.Create();
            }

            if (m_ConfigHandle == 0 || m_LastSessionConfig.Equals(config))
            {
                NRDebugger.Info("[NativeConfigration] Faild to Update NRSessionConfig!!!");
                m_IsUpdateConfigLock = false;
                return false;
            }

            await UpdatePlaneFindMode(config);
            await UpdateImageTrackingConfig(config);

            m_LastSessionConfig.CopyFrom(config);
            m_IsUpdateConfigLock = false;
            return true;
        }

        /// <summary> Updates the plane find mode described by config. </summary>
        /// <param name="config"> The configuration.</param>
        /// <returns> An asynchronous result. </returns>
        private Task UpdatePlaneFindMode(NRSessionConfig config)
        {
            return Task.Run(() =>
            {
                var currentmode = this.GetPlaneFindMode(m_ConfigHandle);
                if (currentmode != config.PlaneFindingMode)
                {
                    SetPlaneFindMode(m_ConfigHandle, config.PlaneFindingMode);
                }
            });
        }

        /// <summary> Updates the image tracking configuration described by config. </summary>
        /// <param name="config"> The configuration.</param>
        /// <returns> An asynchronous result. </returns>
        private Task UpdateImageTrackingConfig(NRSessionConfig config)
        {
            return Task.Run(() =>
            {
                switch (config.ImageTrackingMode)
                {
                    case TrackableImageFindingMode.DISABLE:
                        var result = SetTrackableImageDataBase(m_ConfigHandle, 0);
                        if (result)
                        {
                            m_TrackableImageDatabaseDict.Clear();
                        }
                        NRDebugger.Info("[NativeConfigration] Disable trackable image result : " + result);
                        break;
                    case TrackableImageFindingMode.ENABLE:
                        if (config.TrackingImageDatabase == null)
                        {
                            return;
                        }

                        if (!m_TrackableImageDatabaseDict.TryGetValue(config.TrackingImageDatabase.GUID, out m_DatabaseHandle))
                        {
                            DeployData(config.TrackingImageDatabase);
                            m_DatabaseHandle = m_NativeTrackableImage.CreateDataBase();
                            m_TrackableImageDatabaseDict.Add(config.TrackingImageDatabase.GUID, m_DatabaseHandle);
                        }
                        result = m_NativeTrackableImage.LoadDataBase(m_DatabaseHandle, config.TrackingImageDatabase.TrackingImageDataPath);
                        NRDebugger.Info("[NativeConfigration] LoadDataBase path:{0} result:{1} ", config.TrackingImageDatabase.TrackingImageDataPath, result);
                        result = SetTrackableImageDataBase(m_ConfigHandle, m_DatabaseHandle);
                        NRDebugger.Info("[NativeConfigration] SetTrackableImageDataBase result : " + result);
                        break;
                    default:
                        break;
                }
            });
        }

        /// <summary> Deploy data. </summary>
        /// <param name="database"> The database.</param>
        private void DeployData(NRTrackingImageDatabase database)
        {
            string deploy_path = database.TrackingImageDataOutPutPath;
            NRDebugger.Info("[TrackingImageDatabase] DeployData to path :" + deploy_path);
            ZipUtility.UnzipFile(database.RawData, deploy_path, NativeConstants.ZipKey);
        }

        /// <summary> Creates a new UInt64. </summary>
        /// <returns> An UInt64. </returns>
        private UInt64 Create()
        {
            UInt64 config_handle = 0;
            var result = NativeApi.NRConfigCreate(PerceptionHandle, ref config_handle);
            NativeErrorListener.Check(result, this, "Create");
            return config_handle;
        }

        /// <summary> Gets plane find mode. </summary>
        /// <param name="config_handle"> The Configuration handle to destroy.</param>
        /// <returns> The plane find mode. </returns>
        public TrackablePlaneFindingMode GetPlaneFindMode(UInt64 config_handle)
        {
            TrackablePlaneFindingMode mode = TrackablePlaneFindingMode.DISABLE;
            var result = NativeApi.NRConfigGetTrackablePlaneFindingMode(PerceptionHandle, config_handle, ref mode);
            NativeErrorListener.Check(result, this, "GetPlaneFindMode");
            return mode;
        }

        /// <summary> Sets plane find mode. </summary>
        /// <param name="config_handle"> The Configuration handle to destroy.</param>
        /// <param name="mode">          The mode.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool SetPlaneFindMode(UInt64 config_handle, TrackablePlaneFindingMode mode)
        {
            int mode_value;
            switch (mode)
            {
                case TrackablePlaneFindingMode.DISABLE:
                case TrackablePlaneFindingMode.HORIZONTAL:
                case TrackablePlaneFindingMode.VERTICAL:
                    mode_value = (int)mode;
                    break;
                case TrackablePlaneFindingMode.BOTH:
                    mode_value = ((int)TrackablePlaneFindingMode.HORIZONTAL) | ((int)TrackablePlaneFindingMode.VERTICAL);
                    break;
                default:
                    mode_value = (int)TrackablePlaneFindingMode.DISABLE;
                    break;
            }
            var result = NativeApi.NRConfigSetTrackablePlaneFindingMode(PerceptionHandle, config_handle, mode_value);
            NativeErrorListener.Check(result, this, "SetPlaneFindMode");
            return result == NativeResult.Success;
        }

        /// <summary> Gets trackable image data base. </summary>
        /// <param name="config_handle"> The Configuration handle to destroy.</param>
        /// <returns> The trackable image data base. </returns>
        public UInt64 GetTrackableImageDataBase(UInt64 config_handle)
        {
            UInt64 database_handle = 0;
            var result = NativeApi.NRConfigGetTrackableImageDatabase(PerceptionHandle, config_handle, ref database_handle);
            NativeErrorListener.Check(result, this, "GetTrackableImageDataBase");
            return database_handle;
        }

        /// <summary> Sets trackable image data base. </summary>
        /// <param name="config_handle">   The Configuration handle to destroy.</param>
        /// <param name="database_handle"> Handle of the database.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool SetTrackableImageDataBase(UInt64 config_handle, UInt64 database_handle)
        {
            var result = NativeApi.NRConfigSetTrackableImageDatabase(PerceptionHandle, config_handle, database_handle);
            NativeErrorListener.Check(result, this, "SetTrackableImageDataBase");
            return result == NativeResult.Success;
        }

        public bool IsTrackableAnchorEnabled()
        {
            bool enabled = false;
            if (PerceptionHandle == 0 || m_ConfigHandle == 0)
                return false;
            var result = NativeApi.NRConfigIsTrackableAnchorEnabled(PerceptionHandle, m_ConfigHandle, ref enabled);
            NRDebugger.Info("[NativeConfigration] IsTrackableAnchorEnabled : {0} {1} {2}", enabled, PerceptionHandle, m_ConfigHandle);
            NativeErrorListener.Check(result, this, "IsTrackableAnchorEnabled");
            return enabled;
        }

        public bool SetTrackableAnchorEnabled(bool enabled)
        {
            if (PerceptionHandle == 0 || m_ConfigHandle == 0)
                return false;
            NRDebugger.Info("[NativeConfigration] SetTrackableAnchorEnabled : {0} {1} {2}", enabled, PerceptionHandle, m_ConfigHandle);
            var result = NativeApi.NRConfigSetTrackableAnchorEnabled(PerceptionHandle, m_ConfigHandle, enabled);
            NativeErrorListener.Check(result, this, "SetTrackableAnchorEnabled");
            return result == NativeResult.Success;
        }

        public bool IsMeshingEnabled()
        {
            bool enabled = false;
            if (PerceptionHandle == 0 || m_ConfigHandle == 0)
                return false;
            var result = NativeApi.NRConfigIsMeshingEnabled(PerceptionHandle, m_ConfigHandle, ref enabled);
            NRDebugger.Info("[NativeConfigration] IsMeshingEnabled : {0} {1} {2}", enabled, PerceptionHandle, m_ConfigHandle);
            NativeErrorListener.Check(result, this, "IsMeshingEnabled");
            return enabled;
        }

        public bool SetMeshingEnabled(bool enabled)
        {
            if (PerceptionHandle == 0 || m_ConfigHandle == 0)
                return false;
            NRDebugger.Info("[NativeConfigration] SetMeshingEnabled : {0} {1} {2}", enabled, PerceptionHandle, m_ConfigHandle);
            var result = NativeApi.NRConfigSetMeshingEnabled(PerceptionHandle, m_ConfigHandle, enabled);
            NativeErrorListener.Check(result, this, "SetMeshingEnabled");
            return result == NativeResult.Success;
        }

        public bool IsHandTrackingEnabled()
        {
            bool enabled = false;
            if (PerceptionHandle == 0 || m_ConfigHandle == 0)
                return false;
            var result = NativeApi.NRConfigIsHandTrackingEnabled(PerceptionHandle, m_ConfigHandle, ref enabled);
            NRDebugger.Info("[NativeConfigration] IsHandTrackingEnabled : {0} {1} {2}", enabled, PerceptionHandle, m_ConfigHandle);
            NativeErrorListener.Check(result, this, "IsHandTrackingEnabled");
            return enabled;
        }

        public bool SetHandTrackingEnabled(bool enabled)
        {
            if (PerceptionHandle == 0 || m_ConfigHandle == 0)
                return false;
            var result = NativeApi.NRConfigSetHandTrackingEnabled(PerceptionHandle, m_ConfigHandle, enabled);
            NRDebugger.Info("[NativeConfigration] SetHandTrackingEnabled : {0} {1} {2}", enabled, PerceptionHandle, m_ConfigHandle);
            NativeErrorListener.Check(result, this, "SetHandTrackingEnabled");
            return enabled;
        }

        /// <summary> Destroys the given config_handle. </summary>
        /// <param name="config_handle"> The Configuration handle to destroy.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Destroy(UInt64 config_handle)
        {
            var result = NativeApi.NRConfigDestroy(PerceptionHandle, config_handle);
            NativeErrorListener.Check(result, this, "Destroy");
            return result == NativeResult.Success;
        }

        private struct NativeApi
        {
            /// <summary> Create and initialize the configuration object. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <param name="out_config_handle"> [in,out] The handle of configuration object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRConfigCreate(UInt64 perception_handle, ref UInt64 out_config_handle);

            /// <summary> Release memory used by the configuration object. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <param name="config_handle"> The handle of configuration object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRConfigDestroy(UInt64 perception_handle, UInt64 config_handle);

            /// <summary> Get the mode of searching trackable plane. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <param name="config_handle"> The handle of configuration object. </param>
            /// <param name="out_trackable_plane_finding_mode"> [in,out] The finding mode of trackable plane. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRConfigGetTrackablePlaneFindingMode(UInt64 perception_handle,
                UInt64 config_handle, ref TrackablePlaneFindingMode out_trackable_plane_finding_mode);

            /// <summary> Set the mode of searching trackable plane. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <param name="config_handle"> The handle of configuration object. </param>
            /// <param name="trackable_plane_finding_mode"> The finding mode of trackable plane. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRConfigSetTrackablePlaneFindingMode(UInt64 perception_handle,
                UInt64 config_handle, int trackable_plane_finding_mode);

            /// <summary> Get the image database handle. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <param name="config_handle"> The handle of configuration object. </param>
            /// <param name="out_trackable_image_database_handle"> [in,out] The image database handle. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRConfigGetTrackableImageDatabase(UInt64 perception_handle,
                UInt64 config_handle, ref UInt64 out_trackable_image_database_handle);

            /// <summary> Set the current trackable image database. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <param name="config_handle"> The handle of configuration object. </param>
            /// <param name="trackable_image_database_handle"> The image database handle. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRConfigSetTrackableImageDatabase(UInt64 perception_handle,
                UInt64 config_handle, UInt64 trackable_image_database_handle);

            /// <summary> Check whether the trackable-anchor-runtime is enabled. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <param name="config_handle"> The handle of configuration object. </param>
            /// <param name="out_is_enabled"> [in,out] Whether the trackable-anchor-runtime is enabled. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRConfigIsTrackableAnchorEnabled(UInt64 perception_handle,
                UInt64 config_handle, ref bool out_is_enabled);

            /// <summary> Start or stop the trackable-anchor-runtime. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <param name="config_handle"> The handle of configuration object. </param>
            /// <param name="is_enabled"> The trackable-anchor-runtime will start or stop. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRConfigSetTrackableAnchorEnabled(UInt64 perception_handle,
                UInt64 config_handle, bool is_enabled);

            /// <summary> Check whether the hand-tracking-runtime is enabled. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <param name="config_handle"> The handle of configuration object. </param>
            /// <param name="out_is_enabled"> [in,out] Whether the hand-tracking-runtime is enabled. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRConfigIsHandTrackingEnabled(UInt64 perception_handle,
                UInt64 config_handle, ref bool out_is_enabled);

            /// <summary> Start or stop the hand-tracking-runtime. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <param name="config_handle"> The handle of configuration object. </param>
            /// <param name="is_enabled"> The hand-tracking-runtime will start or stop. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRConfigSetHandTrackingEnabled(UInt64 perception_handle,
                UInt64 config_handle, bool is_enabled);

            /// <summary> Check whether the meshing-runtime is enabled. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <param name="config_handle"> The handle of configuration object. </param>
            /// <param name="out_is_enabled"> [in,out] Whether the meshing-runtime is enabled. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRConfigIsMeshingEnabled(UInt64 perception_handle,
                UInt64 config_handle, ref bool out_is_enabled);

            /// <summary> Start or stop the meshing-runtime. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <param name="config_handle"> The handle of configuration object. </param>
            /// <param name="is_enabled"> The meshing-runtime will start or stop. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRConfigSetMeshingEnabled(UInt64 perception_handle,
                UInt64 config_handle, bool is_enabled);
        };
    }
}
