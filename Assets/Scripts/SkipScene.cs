using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkipScene : MonoBehaviour
{
    public string SceneName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Skip();
        }
    }

    public void Skip()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneName);
    }
}
