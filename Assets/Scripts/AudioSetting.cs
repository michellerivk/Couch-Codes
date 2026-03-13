using TMPro;
using UnityEngine;

public class AudioSetting : MonoBehaviour
{
    //[SerializeField] private TextMeshProUGUI _buttonText;
    //[SerializeField] private GameObject _settingsWindow;


    [SerializeField] private Animator animator;

    private bool isOpen;

    //private string _arrowUp = "▲";
    //private string _arrowDown = "▼";

    //public void ChangeButtonString(bool turnedOn)
    //{
    //    if (!turnedOn)
    //    {
    //        _buttonText.text = $"Audio {_arrowUp}";
    //        _settingsWindow.gameObject.SetActive(false);
    //    }
    //    else
    //    {
    //        _buttonText.text = $"Audio {_arrowDown}";
    //        _settingsWindow.gameObject.SetActive(true);
    //    }
    //}

    public void ToggleAudioPanel()
    {
        isOpen = !isOpen;
        animator.SetBool("IsOpen", isOpen);
    }

}
