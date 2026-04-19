using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneTimePlot : MonoBehaviour
{

    [Header("Trigger")]
    [SerializeField] private bool triggerOnlyFromAbove = true;
    [SerializeField] private float topTolerance = 0.05f;

    [Header("Timing")]
    [SerializeField] private float delayBeforeDisappear = 0.8f; // 踩上后多久消失
    [SerializeField] private bool respawn = true;               // 是否重生
    [SerializeField] private float respawnDelay = 2.0f;         // 消失后多久重生

    [Header("Warning Blink")]
    [SerializeField] private bool enableWarningBlink = true;    // 是否开启消失前闪烁预警
    [SerializeField] private float blinkDuration = 0.5f;        // 闪烁总时长（应 <= delayBeforeDisappear）
    [SerializeField] private float blinkInterval = 0.08f;       // 闪烁间隔
    [SerializeField] private float blinkAlpha = 0.25f;          // 闪烁时透明度

    [Header("Optional Visual")]
    [SerializeField] private bool hideRendererWhenDisabled = true;
    public Collider2D PlotCollider;
    public SpriteRenderer PlotRenderer;

    private bool _isRunningCycle;
    private bool _isActive = true;

    private Color baseColor;

    private void Awake()
    {
        PlotCollider = GetComponent<Collider2D>();
        PlotRenderer = GetComponent<SpriteRenderer>();
        baseColor = PlotRenderer.color;

        SetPlatformActive(true);
        SetAlpha(1f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isActive || _isRunningCycle) return;
        if (!collision.collider.CompareTag("Player")) return;

        if (triggerOnlyFromAbove && !IsFromAbove(collision)) return;

        StartCoroutine(DisappearCycle());
    }

    private bool IsFromAbove(Collision2D collision)
    {
        Bounds platformBounds = PlotCollider.bounds;
        Bounds playerBounds = collision.collider.bounds;
        return playerBounds.min.y >= platformBounds.max.y - topTolerance;
    }

    private IEnumerator DisappearCycle()
    {
        _isRunningCycle = true;

        float safeBlinkDuration = Mathf.Clamp(blinkDuration, 0f, delayBeforeDisappear);
        float normalWait = delayBeforeDisappear - safeBlinkDuration;

        // 1) 先正常等待一段时间
        if (normalWait > 0f)
            yield return new WaitForSeconds(normalWait);

        // 2) 消失前闪烁预警
        if (enableWarningBlink && safeBlinkDuration > 0f)
            yield return StartCoroutine(BlinkWarning(safeBlinkDuration));

        // 3) 消失
        SetPlatformActive(false);
        SetAlpha(1f); // 复位，避免重生后半透明

        // 4) 是否重生
        if (respawn)
        {
            yield return new WaitForSeconds(respawnDelay);
            SetPlatformActive(true);
            SetAlpha(1f);
            _isRunningCycle = false;
        }
        else
        {
            // 永不重生
            _isRunningCycle = true;
        }
    }

    private IEnumerator BlinkWarning(float duration)
    {
        float elapsed = 0f;
        bool low = false;
        float interval = Mathf.Max(0.02f, blinkInterval);

        while (elapsed < duration)
        {
            low = !low;
            SetAlpha(low ? blinkAlpha : 1f);

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        // 结束时确保可见
        SetAlpha(1f);
    }

    private void SetPlatformActive(bool active)
    {
        _isActive = active;
        PlotCollider.enabled = active;

        if (hideRendererWhenDisabled && PlotRenderer != null)
            PlotRenderer.enabled = active;
    }

    private void SetAlpha(float a)
    {
        if (PlotRenderer == null) return;
        Color c = baseColor;
        c.a = Mathf.Clamp01(a);
        PlotRenderer.color = c;
    }
}
