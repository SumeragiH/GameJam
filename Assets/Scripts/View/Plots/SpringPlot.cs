using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringPlot : MonoBehaviour
{
    [Header("Bounce")]
    [SerializeField] private float bounceVelocity = 14f; // 向上弹起速度
    [SerializeField] private bool onlyFromAbove = true;  // 只允许从上方触发
    [SerializeField] private float topTolerance = 0.05f; // 顶部容差

    public event Action<GameObject, float> PlayerHitSpring;

    public Collider2D PlotCollider;

    private void Awake()
    {
        if (PlotCollider == null)
            PlotCollider = GetComponent<Collider2D>();
        if(PlotCollider!= null && !PlotCollider.isTrigger)
            PlotCollider.isTrigger = true;
    }

    public void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        //if (onlyFromAbove && !IsFromAbove(other)) return;
        PlayerView playerView = other.GetComponent<PlayerView>();
        playerView.ApplySpringBounce(bounceVelocity);
    }

    private bool IsFromAbove(Collider2D other)
    {
        // 用碰撞体包围盒判断：玩家最低点 >= 弹簧最高点（带一点容差）
        Bounds spring = PlotCollider.bounds;
        Bounds player = other.bounds;

        return player.min.y >= spring.max.y - topTolerance;
    }
}
