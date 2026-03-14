using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
	private void Start()
	{
        if(AudioManager.instance != null && AudioManager.instance.TitleMusic != null)
	        AudioManager.instance.PlayTitle(); // Play the title music
	}
    public static void SwitchScene(string sceneName)
    {
        SwitchMusic(sceneName);
        SceneManager.LoadScene(sceneName);
    }

    public static void SwitchMusic(string sceneName)
    {
        if (AudioManager.instance != null)
        {
            if (sceneName == "LobbyScene" && AudioManager.instance.LobbyMusic != null) // Play the lobby background music
                AudioManager.instance.PlayLobby();

            if (sceneName == "BoardScene") // Play the board background music
                AudioManager.instance.PlayBoard();
        }

    }
}
