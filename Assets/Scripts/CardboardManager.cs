// using UnityEngine;
// using Google.XR.Cardboard;

// public class CardboardManager : MonoBehaviour
// {
//     void Start()
//     {
//         Screen.sleepTimeout = SleepTimeout.NeverSleep;

//         if (!Api.HasDeviceParams())
//         {
//             Api.ScanDeviceParams();
//         }
//     }

//     void Update()
//     {
//         if (Api.IsGearButtonPressed)
//         {
//             Api.ScanDeviceParams();
//         }

//         if (Api.IsCloseButtonPressed)
//         {
//             Application.Quit();
//         }

//         if (Api.IsTriggerHeldPressed)
//         {
//             Api.Recenter();
//         }

//         if (Api.HasNewDeviceParams())
//         {
//             Api.ReloadDeviceParams();
//         }

//         Api.UpdateScreenParams();
//     }
// }


using UnityEngine;
using Google.XR.Cardboard;
using UnityEngine.XR.Management;
using System.Collections;

public class CardboardManager : MonoBehaviour
{
    private bool cardboardReady = false;

    IEnumerator Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Debug.Log("Starting Cardboard initialization...");

        // Check XR General Settings
        XRGeneralSettings xrSettings = XRGeneralSettings.Instance;

        if (xrSettings == null)
        {
            Debug.LogError("XRGeneralSettings.Instance is NULL! Check XR Plugin Management settings.");
            yield break;
        }

        // Get the XR Manager
        XRManagerSettings xrManager = xrSettings.Manager;

        if (xrManager == null)
        {
            Debug.LogError("XR Manager is NULL! Check your XR Plugin Management configuration.");
            yield break;
        }

        Debug.Log("XR Manager found. Waiting for initialization...");

        // Wait until XR initialization finishes
        while (!xrManager.isInitializationComplete)
        {
            yield return null;
        }

        // Verify that the XR loader initialized successfully
        if (xrManager.activeLoader == null)
        {
            Debug.LogError("Cardboard XR Loader failed to initialize!");
            yield break;
        }

        Debug.Log("XR Loader initialized successfully!");

        // Cardboard is now ready
        cardboardReady = true;

        Debug.Log("Cardboard XR initialized successfully!");

        // Check for saved device parameters
        if (!Api.HasDeviceParams())
        {
            Debug.Log("No saved device parameters found. Starting scan...");
            Api.ScanDeviceParams();
        }
        else
        {
            Debug.Log("Existing Cardboard device parameters found.");
        }
    }

    void Update()
    {
        // Don't execute Cardboard functions before initialization
        if (!cardboardReady)
            return;

        // Gear button: Scan device parameters
        if (Api.IsGearButtonPressed)
        {
            Api.ScanDeviceParams();
        }

        // Close button: Exit application
        if (Api.IsCloseButtonPressed)
        {
            Application.Quit();
        }

        // Trigger: Recenter headset
        if (Api.IsTriggerHeldPressed)
        {
            Api.Recenter();
        }

        // Reload device parameters when updated
        if (Api.HasNewDeviceParams())
        {
            Api.ReloadDeviceParams();
        }

        // Update screen parameters
        Api.UpdateScreenParams();
    }
}
