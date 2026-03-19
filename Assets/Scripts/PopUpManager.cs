using UnityEngine;

public class PopUpManager : MonoBehaviour
{
    [SerializeField] private GameObject _popUp;
    [SerializeField] private Animator _animWindow;

    public void Start()
    {
        _animWindow.SetBool("IsOpen", false);
    }

    public void TogglePopUp(bool on)
    {
        //_popUp.SetActive(on);
        _animWindow.SetBool("IsOpen", on);

        if (AudioManager.instance != null && AudioManager.instance.TitleMusic != null)
        {
            if (on) AudioManager.instance.LowerTitle();
            else AudioManager.instance.IncreaseTitle(); 
        }
    }
   
}
