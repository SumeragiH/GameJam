using System.Collections.Generic;
using UnityEngine;

public class ScanCoverView : CoverView
{
    [Header("Scan Binding")]
    [SerializeField] private ScanRegionProvider _scanRegionProvider;
    [SerializeField] private PolygonCollider2D _scanCollider;
    [SerializeField] private bool _syncTransformToRegion = true;
    [SerializeField] private bool _syncPolygonToRegion = true;

    [Header("Scan Controls")]
    [Range(0, 3)] [SerializeField] private int _currentState = 0;
    [SerializeField, Min(0.01f)] private float _moveDuration = 1.0f;
    [SerializeField, Min(0.05f)] private float _widthScale = 1.0f;

    private readonly Vector2[] _polygonPoints = new Vector2[4];
    private readonly List<Collider2D> _scanOverlapResults = new List<Collider2D>();
    private ContactFilter2D _scanOverlapFilter;

    protected override void Start()
    {
        base.Start();
        if (_scanRegionProvider == null)
        {
            _scanRegionProvider = GetComponent<ScanRegionProvider>();
        }

        if (_scanCollider == null)
        {
            _scanCollider = GetComponent<PolygonCollider2D>();
        }

        _scanOverlapFilter = default;
        _scanOverlapFilter.NoFilter();
        ApplyControlToProvider();
    }

    private void LateUpdate()
    {
        if (_scanRegionProvider == null)
        {
            return;
        }

        if (!_scanRegionProvider.TryGetWorldParallelogram(
                out Vector3 centerWorld,
                out Vector3 topLeftWorld,
                out Vector3 topRightWorld,
                out Vector3 bottomRightWorld,
                out Vector3 bottomLeftWorld))
        {
            return;
        }

        if (_syncTransformToRegion)
        {
            Vector3 position = transform.position;
            transform.position = new Vector3(centerWorld.x, centerWorld.y, position.z);
        }

        if (_syncPolygonToRegion && _scanCollider != null)
        {
            _polygonPoints[0] = transform.InverseTransformPoint(topLeftWorld);
            _polygonPoints[1] = transform.InverseTransformPoint(topRightWorld);
            _polygonPoints[2] = transform.InverseTransformPoint(bottomRightWorld);
            _polygonPoints[3] = transform.InverseTransformPoint(bottomLeftWorld);
            _scanCollider.SetPath(0, _polygonPoints);
        }

        TryActivateSafeZoneWhileScanning();
    }

    private void OnValidate()
    {
        _currentState = Mathf.Clamp(_currentState, 0, 3);
        _moveDuration = Mathf.Max(0.01f, _moveDuration);
        _widthScale = Mathf.Max(0.05f, _widthScale);
        ApplyControlToProvider();
    }

    public override void ShiftState()
    {
        if (_scanRegionProvider == null)
        {
            return;
        }

        if (!_scanRegionProvider.TryShiftNextState())
        {
            return;
        }

        if (_currentState == 3)
        {
            CoverEnabled = false;
        }
        else
        {
            _currentState = Mathf.Clamp(_currentState + 1, 0, 3);
        }
    }

    private void ApplyControlToProvider()
    {
        if (_scanRegionProvider == null)
        {
            return;
        }

        _scanRegionProvider.MoveDuration = _moveDuration;
        _scanRegionProvider.WidthScale = _widthScale;
        _scanRegionProvider.SetStateIndex(_currentState);
    }

    public override void ResetCover()
    {
        _currentState = 0;
        ApplyControlToProvider();
        CoverEnabled = true;
    }

    private void TryActivateSafeZoneWhileScanning()
    {
        if (_scanRegionProvider == null || _scanCollider == null || !_scanRegionProvider.IsMoving)
        {
            return;
        }

        _scanOverlapResults.Clear();
        int overlapCount = _scanCollider.OverlapCollider(_scanOverlapFilter, _scanOverlapResults);
        for (int i = 0; i < overlapCount && i < _scanOverlapResults.Count; i++)
        {
            Collider2D hitCollider = _scanOverlapResults[i];
            if (hitCollider == null)
            {
                continue;
            }

            SafeZoneCoverView safeZoneView = hitCollider.GetComponent<SafeZoneCoverView>();
            if (safeZoneView == null)
            {
                continue;
            }
            SafeZoneSystem.Instance.TryActivateNextSafeZoneByScan(safeZoneView.safeZoneIndex);
        }
    }
}
