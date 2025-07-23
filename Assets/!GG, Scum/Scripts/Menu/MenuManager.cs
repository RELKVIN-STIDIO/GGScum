using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI labelText;

    private void Start()
    {
        labelText.text = "<shake>GG, Scum<shake>";
    }

    #region Button Methods
    public void PlayButton()
    {
        Debug.Log("Play Button Clicked");
        labelText.text = "Play";
    }
    public void SettingButton()
    {
        Debug.Log("Setting Button Clicked");
        labelText.text = "Settings";
    }
    public void CreditsButton()
    {
        Debug.Log("Credits Button Clicked");
        labelText.text = "Credits";
    }
    #endregion
}
