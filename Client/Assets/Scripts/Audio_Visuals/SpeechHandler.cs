using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;

public class SpeechHandler : MonoBehaviour, IMixedRealitySpeechHandler
{
    public GameObject menu;

    public void OnSpeechKeywordRecognized(SpeechEventData eventData)
    {
        switch (eventData.Command.Keyword.ToLower())
        {
            case "menu":
                menu.SetActive(true);
                CommunicationManager.Instance.isClosed = false;
                break;
            /*case "close":
                menu.SetActive(false);
                break;*/
            default:
                Debug.Log($"Unknown option {eventData.Command.Keyword}");
                break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        CoreServices.InputSystem?.RegisterHandler<IMixedRealitySpeechHandler>(this);
    }


}
