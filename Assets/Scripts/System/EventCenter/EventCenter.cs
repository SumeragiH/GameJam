using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//空接口，也就是基类
public interface IEventInfo
{

}
//泛型变量，本质就是继承了IEventInfo的UnityAction<T>，用来存储事件中心容器中的委托
//存储有参数的委托
public class EventInfo<T> : IEventInfo
{
    public UnityAction<T> actions;
    public EventInfo(UnityAction<T> action)
    {
        this.actions += action;
    }
}

//非泛型变量，存储没有参数的委托
public class EventInfo : IEventInfo
{
    public UnityAction actions;
    public EventInfo(UnityAction action)
    {
        this.actions += action;
    }
}

//存储有两个参数的委托
public class EventInfo<T, K> : IEventInfo
{
    public UnityAction<T, K> actions;
    public EventInfo(UnityAction<T, K> action)
    {
        this.actions += action;
    }
}

public class EventCenter : SingletonBaseWithMono<EventCenter>
{
    //事件中心容器，ksy是事件名称，value是事件基类，实现单例模式存储泛型和非泛型的委托
    public Dictionary<string, IEventInfo> eventDic = new Dictionary<string, IEventInfo>();


    /// <summary>
    /// 添加事件监听
    /// </summary>
    /// <param name="name">要监听的事件名称</param>
    /// <param name="action">准备用来处理事件的委托函数</param>
    public void AddListener<T>(string name, UnityAction<T> action)
    {

        if(string.IsNullOrEmpty(name) || action == null)
            return;
        //如果事件中心容器中有这个委托了
        if (eventDic.ContainsKey(name))
        {
            //那么要执行的函数直接加到已有的委托后面
            (eventDic[name] as EventInfo<T>).actions += action;
        }
        else//否则直接添加一个委托
        {
            eventDic.Add(name, new EventInfo<T>(action));
        }
    }
    /// <summary>
    /// 添加事件监听
    /// </summary>
    /// <param name="name">要监听的事件名称</param>
    /// <param name="action">准备用来处理事件的委托函数</param>
    public void AddListener(string name, UnityAction action)
    {
        if (string.IsNullOrEmpty(name) || action == null)
            return;
        //如果事件中心容器中有这个委托了
        if (eventDic.ContainsKey(name))
        {
            //那么要执行的函数直接加到已有的委托后面
            (eventDic[name] as EventInfo).actions += action;
        }
        else//否则直接添加一个委托
        {
            eventDic.Add(name, new EventInfo(action));
        }
    }
    /// <summary>
    /// 添加事件监听
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="K"></typeparam>
    /// <param name="name"></param>
    /// <param name="action"></param>
    public void AddListener<T, K>(string name, UnityAction<T, K> action)
    {
        if (string.IsNullOrEmpty(name) || action == null)
            return;
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo<T, K>).actions += action;
        }
        else
        {
            eventDic.Add(name, new EventInfo<T, K>(action));
        }
    }

    /// <summary>
    /// 删除事件监听，当玩家等监听的对象被删除了，那么他们的监听方法也删除
    /// </summary>
    /// <param name="name">要删除的事件名称</param>
    /// <param name="action">要删除的委托函数</param>>
    public void RemoveListener<T>(string name, UnityAction<T> action)
    {
        if (string.IsNullOrEmpty(name) || action == null)
            return;
        //如果事件中心的容器中有这个委托，就移除
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo<T>).actions -= action;
        }
    }
    /// <summary>
    /// 删除事件监听，当玩家等监听的对象被删除了，那么他们的监听方法也删除
    /// </summary>
    /// <param name="name">要删除的事件名称</param>
    /// <param name="action">要删除的委托函数</param>
    public void RemoveListener(string name, UnityAction action)
    {
        if (string.IsNullOrEmpty(name) || action == null)
            return;
        //如果事件中心的容器中有这个委托，就移除
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo).actions -= action;
        }
    }

    public void RemoveListener<T, K>(string name, UnityAction<T, K> action)
    {
        if (string.IsNullOrEmpty(name) || action == null)
            return;
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo<T, K>).actions -= action;
        }
    }

    /// <summary>
    /// 事件触发
    /// </summary>
    /// <param name="name">触发的事件名称</param>
    /// <param name="obj">这个事件的参数</param>
    public void EventTrigger<T>(string name, T obj)//触发哪个事件
    {
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo<T>).actions.Invoke(obj);
        }
    }
    /// <summary>
    /// 事件触发
    /// </summary>
    /// <param name="name">触发的事件名称</param>
    public void EventTrigger(string name)
    {
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo).actions.Invoke();
        }
    }
    /// <summary>
    /// 事件触发（两个参数）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="K"></typeparam>
    /// <param name="name"></param>
    /// <param name="obj1"></param>
    /// <param name="obj2"></param>
    public void EventTrigger<T, K>(string name, T obj1, K obj2)
    {
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo<T, K>).actions.Invoke(obj1, obj2);
        }
    }

    /// <summary>
    /// 事件中心清空，用在过场景的时候
    /// </summary>
    public void Clean()
    {
        eventDic.Clear();
    }
}