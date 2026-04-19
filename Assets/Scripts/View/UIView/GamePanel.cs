using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : SingletonBaseWithMono<GamePanel>
{
    public Text txtCollectionNum;
    public Button btnReset;
    public EnergySystem energySystem;
    public Button btnMoveNext;
    public Button btnMovePrevious;
    //public Button btnRevert;

    public void Start()
    {
        txtCollectionNum.text = CollectionSystem.Instance.permanentCollectionPoints.ToString();
        btnReset.onClick.AddListener(OnResetButtonClicked);
        //btnRevert.onClick.AddListener(OnRevertButtonClicked);
        btnMoveNext.onClick.AddListener(OnMoveNextButtonClicked);
        btnMovePrevious.onClick.AddListener(OnMovePreviousButtonClicked);
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

    private void OnMoveNextButtonClicked()
    {
        Debug.Log("Move Next Button Clicked");
        Camera.main.GetComponent<CameraView>().MoveNext();
    }

    private void OnMovePreviousButtonClicked()
    {
        Debug.Log("Move Previous Button Clicked");  
        Camera.main.GetComponent<CameraView>().MovePrevious();
    }

}
