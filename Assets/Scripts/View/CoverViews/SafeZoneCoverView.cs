using UnityEngine;

public class SafeZoneCoverView : CoverView 
{
    /// <summary>
    /// 是否被按shift放大
    /// </summary>
    private bool isZoomed = false;
    [Range(1, 4)] private float zoomedScale = 2.0f;

    private CircleRegionProvider _circleRegionProvider;
    private CircleCollider2D _circleCollider;
    private float _baseColliderRadius;

    protected override void Start()
    {
        base.Start();
        _circleRegionProvider = GetComponent<CircleRegionProvider>();
        _circleCollider = GetComponent<CircleCollider2D>();

        if (_circleCollider != null)
        {
            _baseColliderRadius = _circleCollider.radius;
        }

        ApplyZoomState();
    }

    /// <summary>
    /// 按shift，放大SafeZone
    /// </summary>
    public override void ShiftState()
    {
        isZoomed = !isZoomed;
        ApplyZoomState();
    }

    private void ApplyZoomState()
    {
        float currentScale = isZoomed ? zoomedScale : 1f;

        if (_circleCollider != null)
        {
            _circleCollider.radius = _baseColliderRadius * currentScale;
        }

        if (_circleRegionProvider != null)
        {
            _circleRegionProvider.SetZoomScale(zoomedScale);
            _circleRegionProvider.IsZoomed = isZoomed;
        }
    }

    private void OnValidate()
    {
        zoomedScale = Mathf.Max(1f, zoomedScale);
        if (Application.isPlaying)
        {
            ApplyZoomState();
        }
    }

    public override void ResetCover()
    {
        isZoomed = false;
        ApplyZoomState();
    }
}
