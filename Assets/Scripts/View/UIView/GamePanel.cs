using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : SingletonBaseWithMono<GamePanel>
{
    public Text txtCollectionNum;
    public Button btnReset;
    
    public void Start()
    {
        txtCollectionNum.text = CollectionSystem.Instance.permanentCollectionPoints.ToString();
        btnReset.onClick.AddListener(OnResetButtonClicked);
    }

    public void UpdateCollectionNum(int num)
    {
        txtCollectionNum.text = num.ToString();
    }

    private void OnResetButtonClicked()
    {
        CheckPointSystem.Instance.LoadCheckPoint();
    }


}
