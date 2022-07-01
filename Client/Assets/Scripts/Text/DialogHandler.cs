using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;

public class DialogHandler : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro buttonText;
    public TextMeshPro ButtonText
    {
        get => buttonText;
        set => buttonText = value;
    }
    /// <summary>
    /// A reference to the Dialog that this button is on.
    /// </summary>
    public Dialog ParentDialog { get; set; }

    /// <summary>
    /// The type description of the button
    /// </summary>
    public DialogButtonType ButtonTypeEnum;

    /// <summary>
    /// Event handler that runs when button is clicked.
    /// Dismisses the parent dialog.
    /// </summary>
    /// <param name="obj">Caller GameObject</param>
    public void OnButtonClicked(GameObject obj)
    {

        if (ParentDialog != null)
        {
            ParentDialog.Result.Result = ButtonTypeEnum;
            ParentDialog.DismissDialog();
            Debug.Log("parent dialog found");
        }
        if (ButtonTypeEnum == DialogButtonType.Yes)
        {
            Debug.Log("Yes clicked");
            TextHandler.Instance.InstructionSteps++;
            obj.SetActive(false);

        }

    }

    /// <summary>
    /// Setter method to set the text at the top of the Dialog.
    /// </summary>
    /// <param name="title">Title of the button</param>
    public void SetTitle(string title)
    {
        if (ButtonText)
        {
            ButtonText.text = title;
        }
    }
}
