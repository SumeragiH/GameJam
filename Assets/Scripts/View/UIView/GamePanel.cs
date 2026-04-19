using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : SingletonBaseWithMono<GamePanel>
{
    public Text txtCollectionNum;
    public Button btnReset;
    //public Button btnRevert;
    
    public void Start()
    {
        txtCollectionNum.text = CollectionSystem.Instance.permanentCollectionPoints.ToString();
        btnReset.onClick.AddListener(OnResetButtonClicked);
        //btnRevert.onClick.AddListener(OnRevertButtonClicked);
    }

    public void UpdateCollectionNum(int num)
    {
        txtCollectionNum.text = num.ToString();
    }

    private void OnResetButtonClicked()
    {
        CheckPointSystem.Instance.LoadCheckPointWithReset();
    }
    //private void OnRevertButtonClicked()
    //{
    //    PlayerView.Instance.Revert();
    //}


}
