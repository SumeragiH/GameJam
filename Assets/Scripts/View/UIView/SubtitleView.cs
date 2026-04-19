using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SubtitleView : MonoBehaviour
{
    [SerializeField] private Text _subtitleText;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private bool _hideOnAwake = true;

    private Coroutine _autoHideCoroutine;

    public static SubtitleView Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("SubtitleView already exists in scene, duplicate will be destroyed.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (_subtitleText == null)
        {
            _subtitleText = GetComponentInChildren<Text>(true);
        }

        if (_subtitleText == null)
        {
            Debug.LogError("SubtitleView needs a Text reference to show subtitle content.");
            enabled = false;
            return;
        }

        if (_hideOnAwake)
        {
            HideImmediate();
        }
    }

    public void ShowSubtitle(string content, float duration)
    {
        if (!enabled)
        {
            Debug.LogError("SubtitleView is disabled and cannot show subtitles.");
            return;
        }

        if (_autoHideCoroutine != null)
        {
            StopCoroutine(_autoHideCoroutine);
            _autoHideCoroutine = null;
        }

        _subtitleText.text = content;
        SetVisible(true);

        if (duration > 0f)
        {
            _autoHideCoroutine = StartCoroutine(AutoHideAfter(duration));
        }
    }

    public void HideSubtitle()
    {
        if (_autoHideCoroutine != null)
        {
            StopCoroutine(_autoHideCoroutine);
            _autoHideCoroutine = null;
        }

        HideImmediate();
    }

    public static void Show(string content, float duration)
    {
        if (Instance == null)
        {
            Debug.LogError("No SubtitleView found in scene. Please add one under Canvas.");
            return;
        }

        Instance.ShowSubtitle(content, duration);
    }

    private IEnumerator AutoHideAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        HideImmediate();
        _autoHideCoroutine = null;
    }

    private void HideImmediate()
    {
        _subtitleText.text = string.Empty;
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        _canvasGroup.alpha = visible ? 1f : 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    // bool isFirstTime = false;
    // void FixedUpdate()
    // {
    //     if (!isFirstTime)
    //     {
    //         isFirstTime = true;
    //         ShowSubtitle("这是一个字幕测试", 3f);
    //     }
    // }
}
