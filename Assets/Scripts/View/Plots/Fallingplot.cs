using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fallingplot : MonoBehaviour
{
    [Header("Detect Area (Local)")]
    [SerializeField] private Vector2 detectBoxSize = new Vector2(2f, 3f);   // 检测框大小
    [SerializeField] private Vector2 detectBoxOffset = new Vector2(0f, -2f); // 相对物块中心的偏移（默认在下方）

    [Header("Fall")]
    [SerializeField] private float fallGravityScale = 3f; // 触发后重力
    [SerializeField] private float lifeAfterFall = 3f;    // 下落后多久删除
    [SerializeField] private bool triggerOnce = true;     // 是否只触发一次

    [Header("Debug")]
    [SerializeField] private bool _showGizmos = true;

    private Rigidbody2D PlotCollider;
    private bool isFalling;
    private bool hasTriggered;

    private void Awake()
    {
        PlotCollider = GetComponent<Rigidbody2D>();

        // 初始不下落（由脚本控制触发）
        PlotCollider.bodyType = RigidbodyType2D.Dynamic;
        PlotCollider.gravityScale = 0f;
        PlotCollider.velocity = Vector2.zero; 
        PlotCollider.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        if (isFalling) return;
        if (triggerOnce && hasTriggered) return;

        if (IsPlayerInDetectArea())
        {
            StartFalling();
        }
    }

    private bool IsPlayerInDetectArea()
    {
        Vector2 center = (Vector2)transform.position + detectBoxOffset;

        // 只检测 Player Layer 也可以改成 OverlapBox + tag 校验
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, detectBoxSize, 0f);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] != null && hits[i].CompareTag("Player"))
                return true;
        }

        return false;
    }

    private void StartFalling()
    {
        isFalling = true;
        hasTriggered = true;

        PlotCollider.gravityScale = fallGravityScale;
        PlotCollider.velocity = Vector2.zero; 

        if (lifeAfterFall > 0f)
        {
            Destroy(gameObject, lifeAfterFall);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!_showGizmos) return;

        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + (Vector3)detectBoxOffset;
        Gizmos.DrawWireCube(center, detectBoxSize);
    }
}
