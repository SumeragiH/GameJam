using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class EnergyBar : MonoBehaviour
{
    public RectTransform Energy;

    // 显示/隐藏的锚点坐标（按你的UI布局调整）
    public Vector2 showPos = Vector2.zero;
    public Vector2 hidePos = new Vector2(-92, 0);

    public void Show(float time, UnityAction action = null)
    {
        Energy.DOAnchorPos(showPos, time)
              .SetEase(Ease.Linear)
              .OnComplete(() => action?.Invoke());
    }

    public void Hide(float time, UnityAction action = null)
    {
        Energy.DOAnchorPos(hidePos, time)
              .SetEase(Ease.Linear)
              .OnComplete(() => action?.Invoke());
    }
}