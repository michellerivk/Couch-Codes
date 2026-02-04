using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    /* 
	private void Start()
	{
	AudioManager.instance.PlayTitle(); // Play the title music
	}
	*/
    public static void SwitchScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
