using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class MovePlot : MonoBehaviour
{
    public List<Transform> movePoints; // 路径点列表
    public float moveDuration = 2f;

    public int currentPointIndex = 0; // 当前点
    private int direction = 1;         // 1: 正向, -1: 反向
    private Tween moveTween;

    private void Start()
    {
        if (movePoints == null || movePoints.Count == 0) return;

        // 可选：一开始先放到第一个点
        transform.position = movePoints[currentPointIndex].position;

        MoveNext();
    }

    private void MoveNext()
    {
        // 只有一个点就原地不动
        if (movePoints.Count == 1) return;

        moveTween?.Kill();

        moveTween = transform
            .DOMove(movePoints[currentPointIndex].position, moveDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // 到达后判断是否需要反向
                if (currentPointIndex == movePoints.Count - 1)
                    direction = -1;
                else if (currentPointIndex == 0)
                    direction = 1;

                // 计算下一个目标点
                currentPointIndex += direction;

                // 继续循环
                MoveNext();
            });
    }

    private void OnDestroy()
    {
        moveTween?.Kill();
    }
}