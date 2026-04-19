using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraView : MonoBehaviour
{
    [Header("Path")]
    public List<Transform> movePoints;   // 路径点列表
    public float moveDuration = 1.0f;

    [Header("State")]
    public int currentPointIndex = 0;    // 当前所在点索引（逻辑上）

    private Tween moveTween;

    private void Start()
    {
        if (movePoints == null || movePoints.Count == 0) return;

        currentPointIndex = Mathf.Clamp(currentPointIndex, 0, movePoints.Count - 1);
        transform.position = movePoints[currentPointIndex].position;
    }

    /// <summary>
    /// 对外接口：移动到下一个点
    /// </summary>
    public void MoveNext()
    {
        if (movePoints == null || movePoints.Count <= 1) return;
        if (currentPointIndex >= movePoints.Count - 1) return; // 已是最后一个点

        MoveToIndex(currentPointIndex + 1);
    }

    /// <summary>
    /// 对外接口：移动到上一个点
    /// </summary>
    public void MovePrevious()
    {
        if (movePoints == null || movePoints.Count <= 1) return;
        if (currentPointIndex <= 0) return; // 已是第一个点

        MoveToIndex(currentPointIndex - 1);
    }

    private void MoveToIndex(int targetIndex)
    {
        targetIndex = Mathf.Clamp(targetIndex, 0, movePoints.Count - 1);

        moveTween?.Kill();

        moveTween = transform
            .DOMove(movePoints[targetIndex].position, moveDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                currentPointIndex = targetIndex;
            });
    }

    private void OnDestroy()
    {
        moveTween?.Kill();
    }
}
