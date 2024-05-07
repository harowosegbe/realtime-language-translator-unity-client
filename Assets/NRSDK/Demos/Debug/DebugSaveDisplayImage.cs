using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class DebugSaveDisplayImage : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (NRInput.GetButtonUp(ControllerButton.APP))
        {
            NRSessionManager.Instance?.NRSwapChainMan.NRDebugSaveToPNG();
        }
    }
}
