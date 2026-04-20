using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InitPanel : BasePanel
{
    public Button btnStart;
    public Image btnStartImage;
    public Sprite SpriteA;
    public Sprite SpriteB;
    public float switchInterval = 0.5f; // 切换间隔（秒）

    private bool isA = false;
    private Coroutine switchCoroutine;

    void Start()
    {
        btnStart.onClick.AddListener(OnStartButtonClicked);
        switchCoroutine = StartCoroutine(SwitchButtonImage());
    }

    private IEnumerator SwitchButtonImage()
    {
        while (true)
        {
            isA = !isA;
            btnStartImage.sprite = isA ? SpriteA : SpriteB;
            yield return new WaitForSeconds(switchInterval);
        }
    }

    private void OnStartButtonClicked()
    {
        SceneManager.LoadScene("Level1_Cmajor");
    }

    private void OnDestroy()
    {
        if (switchCoroutine != null)
            StopCoroutine(switchCoroutine);
    }
}