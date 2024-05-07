
#if USING_XR_MANAGEMENT && USING_XR_SDK_XREAL
#define USING_XR_SDK
#endif

using NRKernal;
using UnityEngine;
#if USING_XR_SDK
using UnityEngine.XR;
using System.Collections.Generic;
#endif

public class XRAPIPanel : MonoBehaviour
{
    public const string display_match = "NRSDK Display";
    public const string input_match = "NRSDK Head Tracking";
#if USING_XR_SDK
    private XRDisplaySubsystem mXrealXRDisplaySubsystem;
    private XRInputSubsystem mXrealXRInputSubsystem;
#endif

    // Start is called before the first frame update
    void Start()
    {
#if USING_XR_SDK && !UNITY_EDITOR
        List<XRDisplaySubsystemDescriptor> displays = new List<XRDisplaySubsystemDescriptor>();
        List<XRInputSubsystemDescriptor> inputs = new List<XRInputSubsystemDescriptor>();
        SubsystemManager.GetSubsystemDescriptors(displays);

        Debug.Log("[XRAPIPanel] Number of display providers found: " + displays.Count);

        foreach (var d in displays)
        {
            Debug.Log("[XRAPIPanel] Scanning display id: " + d.id);

            if (d.id.Contains(display_match))
            {
                Debug.Log("[XRAPIPanel] Creating display " + d.id);
                mXrealXRDisplaySubsystem = d.Create();

                if (mXrealXRDisplaySubsystem != null)
                {
                    Debug.Log("[XRAPIPanel] Starting display " + d.id);
                    mXrealXRDisplaySubsystem.Start();
                }
            }
        }

        SubsystemManager.GetSubsystemDescriptors(inputs);
        Debug.Log("[XRAPIPanel] Number of input providers found: " + inputs.Count);
        foreach (var i in inputs)
        {
            Debug.Log("[XRAPIPanel] Scanning input id: " + i.id);

            if (i.id.Contains(input_match))
            {
                Debug.Log("[XRAPIPanel] Creating input " + i.id);
                mXrealXRInputSubsystem = i.Create();

                if (mXrealXRInputSubsystem != null)
                {
                    Debug.Log("[XRAPIPanel] Starting input " + i.id);
                    mXrealXRInputSubsystem.Start();
                }
            }
            else
            {
                Debug.Log("[XRAPIPanel] input id not match: " + i.id);
            }
        }
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (NRInput.GetButtonDown(ControllerButton.TRIGGER))
        {
            PrintLog();
        }
    }

    private void PrintLog()
    {
#if USING_XR_SDK && !UNITY_EDITOR
        var reprojectionMode = mXrealXRDisplaySubsystem.reprojectionMode;
        var contentProtectionEnabled = mXrealXRDisplaySubsystem.contentProtectionEnabled;
        var displayOpaque = mXrealXRDisplaySubsystem.displayOpaque;
        var singlePassRenderingDisabled = mXrealXRDisplaySubsystem.singlePassRenderingDisabled;
        var renderPassCount = mXrealXRDisplaySubsystem.GetRenderPassCount();
        float appGPUTimeLastFrame = 0f;
        mXrealXRDisplaySubsystem.TryGetAppGPUTimeLastFrame(out appGPUTimeLastFrame);
        float gpuTimeLastFrameCompositor = 0f;
        mXrealXRDisplaySubsystem.TryGetCompositorGPUTimeLastFrame(out gpuTimeLastFrameCompositor);
        float displayRefreshRate = 0f;
        mXrealXRDisplaySubsystem.TryGetDisplayRefreshRate(out displayRefreshRate);
        int droppedFrameCount;
        mXrealXRDisplaySubsystem.TryGetDroppedFrameCount(out droppedFrameCount);
        int framePresentCount;
        mXrealXRDisplaySubsystem.TryGetFramePresentCount(out framePresentCount);
        float motionToPhoton;
        mXrealXRDisplaySubsystem.TryGetMotionToPhoton(out motionToPhoton);

        Debug.LogFormat("[XRAPIPanel] PrintLog---- reprojectionMode:{0} contentProtectionEnabled:{1} displayOpaque:{2} singlePassRenderingDisabled:{3} " +
            "renderPassCount:{4} appGPUTimeLastFrame:{5} gpuTimeLastFrameCompositor:{6} displayRefreshRate:{7} " +
            "droppedFrameCount:{8} framePresentCount:{9} motionToPhoton:{10}",
            reprojectionMode,
            contentProtectionEnabled,
            displayOpaque,
            singlePassRenderingDisabled,
            renderPassCount,
            appGPUTimeLastFrame.ToString("F3"),
            gpuTimeLastFrameCompositor.ToString("F3"),
            displayRefreshRate,
            droppedFrameCount,
            framePresentCount,
            motionToPhoton);
#endif
    }
}
