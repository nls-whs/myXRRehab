using UnityEngine;

/// <summary>
/// Outputs given debug information via a 3D text into the scene.
/// This is for "printf debugging" on the HoloLens, as there is no 
/// log window. This should only be _once_ in the scene!
/// </summary>
[RequireComponent(typeof(TextMesh))]
public class WorldDebugOutput : MonoBehaviour
{
    static TextMesh LogText;
    static string TextToLog     = "";
    static bool NewLogging      = false;
    static bool NoDebug         = false;
    static bool DoFade          = false;
    static float FadeTime       = 1.0f;
    static float BackupFadeTime = 1.0f;
    static float dFadeTime      = 0.0f;

    enum LockedStatus { None, BodyLocked, WorldLocked};
    static LockedStatus ChangeLockedStatus = LockedStatus.None;

    private void Awake()
    {
        LogText = gameObject.GetComponent<TextMesh>();
        LogText.characterSize = 0.02f;
        LogText.fontSize = 24;
        LogText.anchor = TextAnchor.MiddleCenter;
        LogText.text = TextToLog;
    }

    private void Update()
    {
        if(!NoDebug)
        {
            if (NewLogging)
            {
                NewLogging = false;
                LogText.text = TextToLog;
                dFadeTime = 0.0f;
                LogText.color = new Color(LogText.color.r, LogText.color.g, LogText.color.b, 1);
            }

            if (DoFade)
            {
                if (dFadeTime < 1.0f)
                {
                    dFadeTime += Time.deltaTime * (1.0f / FadeTime);
                }
                else
                {
                    LogText.color = new Color(LogText.color.r, LogText.color.g, LogText.color.b, 0);
                    FadeTime = BackupFadeTime;
                }
            }

            if (ChangeLockedStatus == LockedStatus.BodyLocked)
            {
                ChangeLockedStatus = LockedStatus.None;
                transform.parent = Camera.main.transform;
                transform.localPosition = new Vector3(0, 0, 3);
                transform.localRotation = Quaternion.identity;
            }
            else if (ChangeLockedStatus == LockedStatus.WorldLocked)
            {
                ChangeLockedStatus = LockedStatus.None;
                transform.parent = null;
            }
        }

    }

    public static void LogAppend(string Text)
    {
        TextToLog += "\n";
        TextToLog += Text;
        NewLogging = true;
    }

    public static void Log(string Text)
    {
        TextToLog  = Text;
        NewLogging = true;
    }

    public static void ClearLog()
    {
        TextToLog  = "";
        NewLogging = true;
    }

    public static void ActivateLogFade(float NewFadeTime)
    {
        FadeTime = NewFadeTime;
        DoFade   = true;
    }

    public static void DeactivateLogFade()
    {
        LogText.color = new Color(LogText.color.r, LogText.color.g, LogText.color.b, 1);
        DoFade = false;
    }

    /// <summary>
    /// Show/don't show log text.
    /// </summary>
    public void ToggleLog()
    {
        LogText.GetComponent<MeshRenderer>().enabled = !LogText.GetComponent<MeshRenderer>().isVisible;

    }

    public static void DeactivateLog()
    {
        NoDebug = true;
    }

    public static void ActivateLog()
    {
        NoDebug = false;
    }

    public static void ShowLogFadedLog(float ShowTime = 2.0f)
    {
        BackupFadeTime = FadeTime;
        FadeTime       = ShowTime;
        LogText.color = new Color(LogText.color.r, LogText.color.g, LogText.color.b, 1);
        dFadeTime      = 0.0f;
    }

    /// <summary>
    /// Makes the output text body-locked to the main camera. 
    /// Meaning it will follow the camera movement.
    /// </summary>
    public static void MakeBodyLocked()
    {
        ChangeLockedStatus = LockedStatus.BodyLocked;
    }

    /// <summary>
    /// If the output text was locked to the camera (bodyLocked), then
    /// it removes it again and makes it static. 
    /// </summary>
    public static void MakeWorldLocked()
    {
        ChangeLockedStatus = LockedStatus.WorldLocked;
    }

    #region Method indirections in order to use them with the event system... Unity is stupid sometimes
    public void LogAppend_(string Text)
    {
        LogAppend(Text);
    }

    public void Log_(string Text)
    {
        Log(Text);
    }

    public void ClearLog_()
    {
        ClearLog();
    }

    public void ActivateLogFade_(float NewFadeTime)
    {
        ActivateLogFade(NewFadeTime);
    }
    
    public void DeactivateLogFade_()
    {
        DeactivateLogFade();
    }

    public void DeactivateLog_()
    {
        DeactivateLog();
    }

    public void ActivateLog_()
    {
        ActivateLog();
    }

    public static void MakeBodyLocked_()
    {
        ChangeLockedStatus = LockedStatus.BodyLocked;
    }

    public static void MakeWorldLocked_()
    {
        ChangeLockedStatus = LockedStatus.WorldLocked;
    }
    #endregion
}
