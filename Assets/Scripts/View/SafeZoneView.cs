using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 安全区View，大小固定不变，用于判断玩家是否在安全区中，是否可以触发扫描
/// </summary>
public class SafeZoneView : MonoBehaviour
{
    [SerializeField] private SafeZoneCoverView coverView;
    [SerializeField] private string _playerTag = "Player";
    [SerializeField] private bool _keepWorldScale = true;
    public bool safeZoneEnable = true;

    private readonly HashSet<int> _insidePlayerActorIds = new HashSet<int>();
    private Vector3 _desiredWorldScale;

    private void Awake()
    {
        _desiredWorldScale = transform.lossyScale;
    }

    private void LateUpdate()
    {
        if (!_keepWorldScale || transform.parent == null)
        {
            return;
        }

        Vector3 parentScale = transform.parent.lossyScale;
        transform.localScale = new Vector3(
            _desiredWorldScale.x / Mathf.Max(Mathf.Abs(parentScale.x), 0.0001f),
            _desiredWorldScale.y / Mathf.Max(Mathf.Abs(parentScale.y), 0.0001f),
            _desiredWorldScale.z / Mathf.Max(Mathf.Abs(parentScale.z), 0.0001f)
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayerCollider(other) || coverView == null || !safeZoneEnable)
        {
            return;
        }

        int actorId = GetPlayerActorId(other);
        if (actorId == int.MinValue || !_insidePlayerActorIds.Add(actorId))
        {
            return;
        }

        if (_insidePlayerActorIds.Count == 1)
        {
            int safeZoneIndex = coverView.safeZoneIndex;
            EventCenter.Instance.EventTrigger("玩家进入安全区", safeZoneIndex);
            Debug.Log($"玩家进入安全区 {safeZoneIndex}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayerCollider(other) || coverView == null || !safeZoneEnable)
        {
            return;
        }


        int actorId = GetPlayerActorId(other);
        if (actorId == int.MinValue || !_insidePlayerActorIds.Remove(actorId))
        {
            return;
        }

        if (_insidePlayerActorIds.Count == 0)
        {
            int safeZoneIndex = coverView.safeZoneIndex;
            EventCenter.Instance.EventTrigger("玩家离开安全区", safeZoneIndex);
            Debug.Log($"玩家离开安全区 {safeZoneIndex}");
        }
    }

    private bool IsPlayerCollider(Component other)
    {
        if (other == null || other.gameObject == null)
        {
            return false;
        }

        if (other.CompareTag(_playerTag))
        {
            return true;
        }

        if (other is CapsuleCollider2D collider2D && collider2D.attachedRigidbody != null && collider2D.attachedRigidbody.CompareTag(_playerTag))
        {
            return true;
        }

        return other.transform.root != null && other.transform.root.CompareTag(_playerTag);
    }

    private int GetPlayerActorId(Component other)
    {
        if (other is Collider2D collider2D && collider2D.attachedRigidbody != null)
        {
            return collider2D.attachedRigidbody.GetInstanceID();
        }

        if (other != null && other.transform.root != null)
        {
            return other.transform.root.GetInstanceID();
        }

        return int.MinValue;
    }

    private void OnDisable()
    {
        _insidePlayerActorIds.Clear();
    }

}
