/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

namespace NRKernal.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using UnityEngine;

    /// <summary> NR world anchor store. </summary>
    public class NRWorldAnchorStore : IDisposable
    {
        /// <summary> The native mapping. </summary>
        private NativeMapping m_NativeMapping;
        /// <summary> Dictionary of anchors. </summary>
        private Dictionary<UInt64, NRWorldAnchor> m_AnchorDict = new Dictionary<UInt64, NRWorldAnchor>();
        /// <summary> Dictionary of anchor uuid and UserDefinedKey. </summary>
        private Dictionary<string, string> m_Anchor2ObjectDict = new Dictionary<string, string>();
        /// <summary> The NRWorldAnchorStore instance </summary>
        public static NRWorldAnchorStore Instance;

        public event Action<string> OnNotifyMessage;

        #region state maintaining
        /// <summary>
        /// The handle of anchor under creating.
        /// Set to 0 when creating finishes.
        /// </summary>
        public UInt64 CreatingAnchorHandle => m_CreatingAnchorHandle;
        private UInt64 m_CreatingAnchorHandle;

        /// <summary>
        /// The handle of anchor under remapping.
        /// Set to 0 when remapping finishes.
        /// </summary>
        public UInt64 RemappingAnchorHandle => m_RemappingAnchorHandle;
        private UInt64 m_RemappingAnchorHandle;


        public bool IsCreatingNewAnchor
        {
            get
            {
                return m_CreatingAnchorHandle != 0;
            }
        }

        public bool IsRemapping
        {
            get
            {
                return m_RemappingAnchorHandle != 0;
            }
        }
        #endregion

        /// <summary> Pathname of the map folder. </summary>
        public const string MapFolder = "XrealMaps";
        /// <summary> Path of the map folder. </summary>
        public readonly string MapPath;
        /// <summary> The anchor to object file. </summary>
        public const string Anchor2ObjectFile = "anchor2object.json";

        /// <summary> Default constructor. </summary>
        internal NRWorldAnchorStore()
        {
#if !UNITY_EDITOR
            m_NativeMapping = new NativeMapping(NRSessionManager.Instance.NativeAPI);
#endif
            Instance = this;
            MapPath =
#if UNITY_EDITOR
                Path.Combine(Directory.GetCurrentDirectory(), MapFolder);
#else
                Path.Combine(Application.persistentDataPath, MapFolder);
#endif
            if (!Directory.Exists(MapPath))
                Directory.CreateDirectory(MapPath);
            string path = Path.Combine(MapPath, Anchor2ObjectFile);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                NRDebugger.Info("[NRWorldAnchorStore] Anchor2Object json: {0}", json);
                m_Anchor2ObjectDict = LitJson.JsonMapper.ToObject<Dictionary<string, string>>(json);
                for (int i = 0; i < m_Anchor2ObjectDict.Count;)
                {
                    var item = m_Anchor2ObjectDict.ElementAt(i).Key;
                    if (File.Exists(Path.Combine(MapPath, item)))
                        i++;
                    else
                        m_Anchor2ObjectDict.Remove(item);
                }
            }

            NRKernalUpdater.OnUpdate += OnUpdate;
        }

        /// <summary> Cleans up the WorldAnchorStore and releases memory. </summary>
        public void Dispose()
        {
            m_NativeMapping = null;
            NRKernalUpdater.OnUpdate -= OnUpdate;
        }

        /// <summary> Executes the 'update' action. </summary>
        private void OnUpdate()
        {
            if (m_AnchorDict.Count == 0)
                return;

#if UNITY_EDITOR
            return;
#endif

            var listhandle = m_NativeMapping.CreateAnchorList();
            m_NativeMapping.UpdateAnchor(listhandle);
            var size = m_NativeMapping.GetAnchorListSize(listhandle);
            for (int i = 0; i < size; i++)
            {
                var anchorhandle = m_NativeMapping.AcquireItem(listhandle, i);
                if (m_AnchorDict.ContainsKey(anchorhandle))
                {
                    var anchor = m_AnchorDict[anchorhandle];

                    anchor.CurrentTrackingState = m_NativeMapping.GetTrackingState(anchorhandle);
                    anchor.CurrentAnchorState = m_NativeMapping.GetAnchorState(anchorhandle);

                    if (anchor.CurrentTrackingState == TrackingState.Tracking)
                    {
                        Pose pose = ConversionUtility.ApiWorldToUnityWorld(m_NativeMapping.GetAnchorPose(anchorhandle));
                        anchor.UpdatePose(pose);
                    }
                }
            }
            m_NativeMapping.DestroyAnchorList(listhandle);
        }



        /// <summary> Creates an NRWorldAnchor. </summary>
        /// <param name="anchor"> The NRWorldAnchor handler.</param>
        /// <returns> The new anchor. </returns>
        public bool CreateAnchor(NRWorldAnchor anchor)
        {
            NRDebugger.Info("[NRWorldAnchorStore] Create a new NRWorldAnchor handle");
            Pose pose = new Pose(anchor.transform.position, anchor.transform.rotation);
            UInt64 handle = 0;
#if UNITY_EDITOR
            handle = (ulong)UnityEngine.Random.Range(1, int.MaxValue);
#else
            handle = m_NativeMapping.AddAnchor(pose);
#endif
            if (handle == 0)
                return false;
#if UNITY_EDITOR
            anchor.UUID = Guid.NewGuid().ToString();

#else
            anchor.UUID = m_NativeMapping.GetAnchorUUID(handle);
#endif
            anchor.AnchorHandle = handle;
            m_AnchorDict[handle] = anchor;

            m_CreatingAnchorHandle = handle;

            NRDebugger.Info($"[NRWorldAnchorStore] Created {handle}  {anchor.UUID}");
            return true;
        }

        /// <summary>
        /// Bind an anchor to an existing handle.
        /// </summary>
        /// <param name="anchor"> The NRWorldAnchor to be associated with. </param>
        /// <param name="handle"> The handle to be associated with. </param>
        public void BindAnchor(NRWorldAnchor anchor, UInt64 handle)
        {
            anchor.AnchorHandle = handle;
            m_AnchorDict[handle] = anchor;
        }

        /// <summary>
        /// 更新anchor的建图质量
        /// </summary>
        /// <param name="anchor"></param>
        /// <param name="pose"></param>
        /// <returns></returns>
        public NREstimateQuality UpdateMapQuality(NRWorldAnchor anchor, Pose pose)
        {
            NRDebugger.Debug($"[{this.GetType()}] {nameof(UpdateMapQuality)} anchor {anchor.AnchorHandle} {anchor.CurrentAnchorState} pos {pose.position} rot{pose.rotation.eulerAngles}");
#if UNITY_EDITOR
            return NREstimateQuality.NR_ANCHOR_QUALITY_GOOD;
#endif
            return m_NativeMapping.EstimateMapQuality(anchor.AnchorHandle, pose);
        }

        public NRAnchorState UpdateAnchorState(NRWorldAnchor anchor)
        {
#if UNITY_EDITOR
            return NRAnchorState.NR_ANCHOR_STATE_SUCCESS;
#endif
            return m_NativeMapping.GetAnchorState(anchor.AnchorHandle);
        }

        public bool Remap(NRWorldAnchor anchor)
        {
#if UNITY_EDITOR
            m_RemappingAnchorHandle = anchor.AnchorHandle;
            return true;
#endif
            if (m_NativeMapping.Remap(anchor.AnchorHandle))
            {
                m_RemappingAnchorHandle = anchor.AnchorHandle;
                return true;
            }
            return false;
        }

        public bool SetEstimateRange(UInt64 anchor_handle, float angle, NREstimateDistance distance)
        {
#if UNITY_EDITOR
            return true;
#endif
            return m_NativeMapping.SetEstimateAngleRange(anchor_handle, angle) &&
                m_NativeMapping.SetEstimateDistanceRange(anchor_handle, distance);
        }


        public bool GetEstimateRange(UInt64 anchor_handle, ref float angle, ref NREstimateDistance distance)
        {

#if UNITY_EDITOR
            angle = 180;
            distance = NREstimateDistance.NR_ESTIMATE_DISTANCE_MEDIUM;
            return true;
#endif
            return m_NativeMapping.GetEstimateAngleRange(anchor_handle, ref angle) &&
                m_NativeMapping.GetEstimateDistanceRange(anchor_handle, ref distance);
        }

        public void DisableAllAnchors()
        {
            var anchors = m_AnchorDict.Values.ToArray();
            foreach (var item in anchors)
            {
                if (item.AnchorHandle == m_CreatingAnchorHandle || item.AnchorHandle == m_RemappingAnchorHandle)
                    item.CurrentAnchorState = NRAnchorState.NR_ANCHOR_STATE_FAILURE;
                else
                    item.CurrentTrackingState = TrackingState.Stopped;
            }
        }

        /// <summary>
        /// Saves an NRWorldAnchor to the disk 
        /// </summary>
        /// <param name="anchor"> The NRWorldAnchor to be saved. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool SaveAnchor(NRWorldAnchor anchor)
        {
            NRDebugger.Info("[NRWorldAnchorStore] Save Anchor: {0}", anchor.UserDefinedKey);
            if (m_Anchor2ObjectDict.ContainsKey(anchor.UUID))
            {
                NRDebugger.Warning("[NRWorldAnchorStore] Save a new anchor that has already been saved.");
                m_Anchor2ObjectDict.Remove(anchor.UUID);
            }

            try
            {
                // 保存操作比较耗时，因此在子线程中调用
                AsyncTaskExecuter.Instance.RunAction(() =>
                {
                    bool success = true;

#if UNITY_EDITOR                    
                    if (true)
                    {
                        Thread.Sleep(1000);
                        File.Create(Path.Combine(MapPath, anchor.UUID)).Dispose();
                    }
                    else
#endif
                    {
                        success = m_NativeMapping.SaveAnchor(anchor.AnchorHandle, Path.Combine(MapPath, anchor.UUID));
                    }

                    if (success)
                    {
                        m_Anchor2ObjectDict.Add(anchor.UUID, anchor.UserDefinedKey);
                        saveAnchor2ObjectFile();
                        OnNotifyMessage?.Invoke($"Save Anchor success {anchor.UUID}");
                    }
                    else
                    {
                        OnNotifyMessage?.Invoke($"Save Anchor failed {anchor.UUID}");
                        MainThreadDispather.QueueOnMainThread(() =>
                        {
                            NRDebugger.Info("[NRWorldAnchorStore] Save Anchor failed.");
                            InternalEraseAnchorWithUUID(anchor.UUID);
                        });
                    }
                });

                return true;
            }
            catch (Exception e)
            {
                NRDebugger.Warning("[NRWorldAnchorStore] Write anchor to object dict exception:" + e.ToString());
                return false;
            }
        }


        /// <summary> Saves all NRWorldAnchor. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool SaveAllAnchors()
        {
            NRDebugger.Info("[NRWorldAnchorStore] Save all worldanchors: {0}.", m_AnchorDict.Count);
            foreach (var item in m_AnchorDict.Values)
            {
                SaveAnchor(item);
            }
            return true;
        }

        /// <summary> Destroy a NRWorldAnchor from the memory. </summary>
        /// <param name="anchor"> The NRWorldAnchor to be destroyed. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool DestroyAnchor(NRWorldAnchor anchor)
        {
            NRDebugger.Info("[NRWorldAnchorStore] Destroy Anchor {0}.", anchor.UUID);

            if (m_AnchorDict.ContainsKey(anchor.AnchorHandle))
            {
                m_AnchorDict.Remove(anchor.AnchorHandle);
            }
#if !UNITY_EDITOR
            AsyncTaskExecuter.Instance.RunAction(() =>
                {
                    m_NativeMapping.DestroyAnchor(anchor.AnchorHandle);
                }
            );
#endif
            removeProcessingAnchorHandle(anchor.AnchorHandle, anchor.CurrentAnchorState);
            GameObject.Destroy(anchor.gameObject);
            return true;
        }

        /// <summary> Destroy all NRWorldAnchors. </summary>
        public void Destroy()
        {
            NRDebugger.Info("[NRWorldAnchorStore] Destroy all worldanchors: {0}.", m_AnchorDict.Count);
            var keys = m_AnchorDict.Keys.ToArray();
            foreach (var key in keys)
            {
                if (m_AnchorDict.TryGetValue(key, out var anchor))
                {
                    DestroyAnchor(anchor);
                }
            }
        }

        /// <summary> Erase a NRWorldAnchor from disk </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool EraseAnchor(NRWorldAnchor anchor)
        {
            NRDebugger.Info("[NRWorldAnchorStore] Erase Anchor: {0}.", anchor.UUID);
            return InternalEraseAnchorWithUUID(anchor.UUID);
        }

        public void EraseAllAnchors()
        {
            var list = GetLoadableAnchorUUID();
            foreach (var uuid in list.Keys)
            {
                InternalEraseAnchorWithUUID(uuid);
            }
        }

        /// <summary> Loads a NRWorldAnchor from disk for given identifier.</summary>
        /// <param name="uuid"> anchor uuid .</param>
        /// <param name="action"> Execute in main thread after success load.</param>
        public void LoadwithUUID(string uuid, Action<UInt64> action)
        {
            if (m_Anchor2ObjectDict.ContainsKey(uuid))
            {
                string path = Path.Combine(MapPath, uuid);
                if (File.Exists(path))
                {
                    AsyncTaskExecuter.Instance.RunAction(() =>
                    {
                        UInt64 handle = 0;
#if UNITY_EDITOR
                        handle = (ulong)UnityEngine.Random.Range(1, int.MaxValue);
#else
                        handle = m_NativeMapping.LoadAnchor(path);
#endif
                        MainThreadDispather.QueueOnMainThread(() =>
                        {
                            if (handle == 0)
                            {
                                NRDebugger.Info("[NRWorldAnchorStore] Load Anchor failed: {0}.", uuid);
                                m_Anchor2ObjectDict.Remove(uuid);
                            }
                            else
                                action?.Invoke(handle);
                        });
                    });
                }
            }
        }

        /// <summary>
        /// Retrieves a dictionary of loadable anchor UUIDs that are not currently loaded in the session.
        /// </summary>
        /// <returns> A dictionary of UUIDs and user-defined keys.</returns>
        public Dictionary<string, string> GetLoadableAnchorUUID()
        {
            var existingUUID = m_AnchorDict.Select(x => x.Value.UUID).ToList();
            return m_Anchor2ObjectDict.Where(x => !existingUUID.Contains(x.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Clear CreatingAnchorHandle or RemappingHandle when mapping finishes.
        /// </summary>
        /// <param name="anchorHandle">Anchor Handle</param>
        /// <param name="state">Anchor state</param>
        public void handleProcessingFinish(ulong anchorHandle, NRAnchorState state)
        {
            if (state == NRAnchorState.NR_ANCHOR_STATE_SUCCESS
                || state == NRAnchorState.NR_ANCHOR_STATE_FAILURE)
            {
                removeProcessingAnchorHandle(anchorHandle, state);
            }
        }

        public void removeProcessingAnchorHandle(ulong anchorHandle, NRAnchorState state)
        {
            if (m_CreatingAnchorHandle == anchorHandle)
            {
                m_CreatingAnchorHandle = 0;
                NRDebugger.Info($"[{this.GetType()}] {nameof(removeProcessingAnchorHandle)} creating {anchorHandle}:{state}");
            }
            if (m_RemappingAnchorHandle == anchorHandle)
            {
                NRDebugger.Info($"[{this.GetType()}] {nameof(removeProcessingAnchorHandle)} remapping {anchorHandle}:{state}");
                m_RemappingAnchorHandle = 0;
            }
        }

        private bool InternalEraseAnchorWithUUID(string UUID)
        {
            if (m_Anchor2ObjectDict.ContainsKey(UUID))
            {
                m_Anchor2ObjectDict.Remove(UUID);
                saveAnchor2ObjectFile();
            }

            string path = Path.Combine(MapPath, UUID);
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            return false;
        }

        private void saveAnchor2ObjectFile()
        {
            string json = LitJson.JsonMapper.ToJson(m_Anchor2ObjectDict);
            string path = Path.Combine(MapPath, Anchor2ObjectFile);
            NRDebugger.Info("[NRWorldAnchorStore] Save to the path:" + path + " json:" + json);
            File.WriteAllText(path, json);
        }

    }
}
