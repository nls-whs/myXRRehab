using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Audio;

public class SpeechLogic : MonoBehaviour
{
    private TextToSpeech _texttoSpeech;
    public string speakText;
    private int i = 0;

    // Start is called before the first frame update
    void Start()
    {
        _texttoSpeech = GetComponent<TextToSpeech>();
    }

    // Update is called once per frame
    void Update()
    {
        if (i == 0)
        {
            var msg = string.Format(speakText, _texttoSpeech.Voice.ToString());

            _texttoSpeech.StartSpeaking(msg);
            i = 1;
        }
    }
}
