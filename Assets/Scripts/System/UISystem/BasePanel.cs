using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BasePanel : MonoBehaviour
{
    //控制淡入淡出
    public CanvasGroup canvasGroup;//控制淡入淡出的组件
    [SerializeField] private float alphaChangeSpeed = 10f;//控制淡入淡出的速度

    private bool isShowing = true;//是否正在显示

    public UnityAction showCallBack;//显示回调事件
    public UnityAction hideCallBack;//隐藏回调事件

    protected virtual void Awake()//因为子类需要重写Awake方法，所以这里使用protected virtual修饰符
    {
        //获取CanvasGroup组件，如果没有就添加一个，这样就可以控制这个面板的透明度了
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    /// <summary>
    /// 控制面板淡入
    /// </summary>
    /// <param name="show">淡入完成后执行的委托</param>
    public virtual void Show(UnityAction show = null)
    {
        if (isShowing)
            return;//如果已经在显示了，就不需要再次调用Show方法
        isShowing = true;
        canvasGroup.alpha = 0f;//从完全透明开始淡入
        showCallBack += show;//将传入的显示回调事件添加到showCallBack事件中，这样在淡入完成后就会调用这个回调事件,比如说在淡入完成后播放一个动画或者声音
    }

    /// <summary>
    /// 控制面板的淡出
    /// </summary>
    /// <param name="hide">淡出完成后执行的委托</param>
    public virtual void Hide(UnityAction hide)
    {
        if (!isShowing) return;//如果已经在隐藏了，就不需要再次调用Hide方法
        isShowing = false;
        canvasGroup.alpha = 1f;//从完全不透明开始淡出
        hideCallBack += hide;//将传入的隐藏回调事件添加到hideCallBack事件中，这样在淡出完成后就会调用这个回调事件,比如说在淡出完成后销毁这个面板对象
    }

    protected virtual void Update()
    {
        //根据isShowing的值来调整canvasGroup的alpha值，从而实现淡入淡出的效果
        if (isShowing)//淡入
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 1f, alphaChangeSpeed * Time.deltaTime);
            if (canvasGroup.alpha >= 0.99f)
            {
                canvasGroup.alpha = 1f;//确保alpha值不会超过1
                showCallBack?.Invoke();//调用显示回调事件
                showCallBack = null;//调用完显示回调事件之后就将showCallBack事件置空，这样就不会在下次调用Show方法的时候再次调用这个回调事件了
            }
        }
        else
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, alphaChangeSpeed * Time.deltaTime);
            if (canvasGroup.alpha <= 0.01f)
            {
                canvasGroup.alpha = 0f;//确保alpha值不会小于0
                hideCallBack?.Invoke();//调用隐藏回调事件
                hideCallBack = null;//调用完隐藏回调事件之后就将hideCallBack事件置空，这样就不会在下次调用Hide方法的时候再次调用这个回调事件了
            }
        }
    }
}
