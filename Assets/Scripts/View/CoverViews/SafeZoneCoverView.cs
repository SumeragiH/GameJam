using UnityEngine;

public class SafeZoneCoverView : CoverView 
{
    /// <summary>
    /// 是否被按shift放大
    /// </summary>
    private bool isZoomed = false;
    [SerializeField, Range(1, 4)] private float zoomedScale = 2.0f;

    private CircleRegionProvider _circleRegionProvider;
    private CircleCollider2D _circleCollider;
    private float _baseColliderRadius;
    public int safeZoneIndex = -1;
    // 安全区检测物体
    [SerializeField] private SafeZoneView linkedSafeZone;
    // 安全区链接的检查点
    [SerializeField] private CheckPointView linkedCheckPoint;

    public void SyncLinkedCheckPointBySafeZoneIndex()
    {
        if (linkedCheckPoint == null)
        {
            return;
        }

        linkedCheckPoint.checkPointID = safeZoneIndex;

        if (linkedCheckPoint.checkPointData == null)
        {
            linkedCheckPoint.checkPointData = new CheckPointData();
        }

        linkedCheckPoint.checkPointData.safeZoneIndex = safeZoneIndex;
    }

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
            _circleRegionProvider.SetRadius(_baseColliderRadius);
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

    protected override void OnCoverEnabled()
    {
        base.OnCoverEnabled();
        linkedCheckPoint?.ShowCheckPoint();
        if (linkedSafeZone != null)
        {
            linkedSafeZone.safeZoneEnable = true;
        }
    }

    protected override void OnCoverDisable()
    {
        base.OnCoverDisable();
        linkedCheckPoint?.HideCheckPoint();
        if (linkedSafeZone != null)
        {
            linkedSafeZone.safeZoneEnable = false;
        }
    }
}
