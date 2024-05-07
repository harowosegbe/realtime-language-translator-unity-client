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
    public class NRDisplaySubsystemDescriptor : IntegratedSubsystemDescriptor<NRDisplaySubsystem>
    {
        public const string Name = "Subsystem.Display";
        public override string id => Name;
    }

    public class NRDisplaySubsystem : IntegratedSubsystem<NRDisplaySubsystemDescriptor>
    {
        internal static NativeDisplay NativeDisplay { get; private set; }

        public NRDisplaySubsystem(NRDisplaySubsystemDescriptor descriptor) : base(descriptor)
        {
            NativeDisplay = new NativeDisplay();
            NRDebugger.Info("[NRDisplaySubsystem] Create");
#if !UNITY_EDITOR
            NativeDisplay.Create();
#endif
            NRDebugger.Info("[NRDisplaySubsystem] Created");
        }

        public override void Start()
        {
            base.Start();

#if !UNITY_EDITOR
            NativeDisplay.Start();
#endif
        }

        public override void Pause()
        {
            base.Pause();

#if !UNITY_EDITOR
            NativeDisplay.Pause();
#endif
        }

        public override void Resume()
        {
            base.Resume();

#if !UNITY_EDITOR
            NativeDisplay.Resume();
#endif
        }

        public override void Destroy()
        {
            base.Destroy();

            NRDebugger.Info("[NRDisplaySubsystem] Destroy");
#if !UNITY_EDITOR
            NativeDisplay.Stop();
            NativeDisplay.Destroy();
#endif
            NRDebugger.Info("[NRDisplaySubsystem] Destroyed");
        }

        internal void ListenMainScrResolutionChanged(NRDisplayResolutionCallback callback)
        {
#if !UNITY_EDITOR
            NativeDisplay.ListenMainScrResolutionChanged(callback);
#endif
        }
    }
}
