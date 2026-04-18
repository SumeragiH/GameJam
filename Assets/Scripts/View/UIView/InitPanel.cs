using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitPanel : BasePanel
{
    public Button btnStart;
    public Button btnEnd;
    public Mask mask;//遮罩
    public Text gameTitle;//游戏名字
    public RawImage backGround;//背景
    public float maskSpeed = 5f;

    public static InitPanelTypeEnum typeEnum = InitPanelTypeEnum.end;//根据不同的枚举显示不同的内容

    private Vector2 targetRect = new Vector2(300,100);
    private Vector2 originalRect = new Vector2(0, 0);

    private bool isShowing = false;
    void Start()
    {
        if (typeEnum == InitPanelTypeEnum.begin)
        {
            gameTitle.text = "开始场景";
            //如果是开始场景
            targetRect = new Vector2(300, 100);
            originalRect = new Vector2(0, 0);
            mask.rectTransform.sizeDelta = originalRect;
        }
        else if(typeEnum == InitPanelTypeEnum.end)
        {
            gameTitle.text = "结束场景";
            //如果是结束场景
            targetRect = new Vector2(0, 0);
            originalRect = new Vector2(300, 100);
            mask.rectTransform.sizeDelta = originalRect;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift)||Input.GetKeyDown(KeyCode.RightShift))
        {
            isShowing = !isShowing;
        }
        if (isShowing)
        {
            mask.rectTransform.sizeDelta = Vector2.Lerp(mask.rectTransform.sizeDelta, targetRect, Time.deltaTime * maskSpeed);

        }
        else
        {
            mask.rectTransform.sizeDelta = Vector2.Lerp(mask.rectTransform.sizeDelta, originalRect, Time.deltaTime * maskSpeed);
        }
    }
}
