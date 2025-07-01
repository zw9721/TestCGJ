using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public string sceneName;
    public void loadScene(string sceneName) 
    {
        Debug.Log($"Loading scene: {sceneName}");
    SceneManager.LoadScene(sceneName);
    }
    public void ExitGame() 
    {
    Application.Quit();
    }
}
