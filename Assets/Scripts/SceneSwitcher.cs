using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public static void SwitchScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
