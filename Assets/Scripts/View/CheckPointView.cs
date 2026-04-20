using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 必然关联一个安全区遮罩
/// </summary>
public class CheckPointView : MonoBehaviour
{
    public CheckPointData checkPointData;//一个检查点数据对象，用于存储当前检查点的信息
    public int checkPointID;//检查点的ID，用于区分不同的检查点

    [SerializeField] public SafeZoneCoverView safeZoneCoverView;//检查点关联的安全区遮罩

    public bool hasBeenSaved = false; // 这个记录点历史上是否被存过

    [SerializeField] private bool shouldMoveCamera = false; // 是否需要在这个检查点移动摄像头
    [SerializeField] private CameraView targetCamera;
    [SerializeField] private CheckPointLogicView checkPointLogicView; // 关联的检查点逻辑组件

    public void Start()
    {
        //除了第一次进入场景时，其他时候都需要隐藏检查点，通过扫描来显示
        if (checkPointID!=0)
            HideCheckPoint();
        checkPointData = new CheckPointData();
        checkPointData.SetCheckPointData(Vector2.zero, 0, safeZoneCoverView.safeZoneIndex);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("触发检查点");
            checkPointData.SetCheckPointData(PlayerView.Instance.transform.position, CollectionSystem.Instance.stageScanPoint, safeZoneCoverView.safeZoneIndex);
            CheckPointSystem.Instance.SaveCheckPoint(this);
            //自身消失
            HideCheckPoint();
            hasBeenSaved = true;
            if (shouldMoveCamera)
            {
                targetCamera?.MoveNext();
            }
            checkPointLogicView?.ReachCheckpoint();
        }
    }

    public void ShowCheckPoint()
    {
        if (!hasBeenSaved)
            gameObject.SetActive(true);
    }

    public void HideCheckPoint()
    {
        gameObject.SetActive(false);
    }


}
