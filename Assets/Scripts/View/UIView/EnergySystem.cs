using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergySystem : MonoBehaviour
{
    public int currentEnergy; // 当前能量值
    public List<EnergyBar> energyBars; // 能量条列表

    private void Start()
    {
        currentEnergy = CollectionSystem.Instance.stageScanPoint;
        Init();
    }
    public void ConsumeEnergy(int amount)
    {
        if (currentEnergy < amount)
        {
            Debug.Log("Not enough energy to consume.");
            return;
        }

        currentEnergy -= amount;

        // 统一刷新，避免两套逻辑不一致
        RefreshBars();

        Debug.Log("Consumed Energy: " + amount + ", Current Energy: " + currentEnergy);
    }

    public void AddEnergy(int amount)
    {
        currentEnergy += amount;
        if (currentEnergy > energyBars.Count) currentEnergy = energyBars.Count;
        if (currentEnergy < 0) currentEnergy = 0;

        RefreshBars();

        Debug.Log("Added Energy: " + amount + ", Current Energy: " + currentEnergy);
    }
    public void Init()
    {
        if (currentEnergy > energyBars.Count) currentEnergy = energyBars.Count;
        if (currentEnergy < 0) currentEnergy = 0;
        RefreshBars();
    }
    public void SetEnergy(int amount)
    {
        currentEnergy = amount;
        if (currentEnergy > energyBars.Count) currentEnergy = energyBars.Count;
        if (currentEnergy < 0) currentEnergy = 0;
        RefreshBars();
    }

    private void RefreshBars()
    {
        for (int i = 0; i < energyBars.Count; i++)
        {
            if (i < currentEnergy) energyBars[i].Show(0.2f);
            else energyBars[i].Hide(0.2f);
        }
    }

    public void SlowlyRecover()
    {
        if(currentEnergy>=1)
        {
            return;
        }
        energyBars[0].Show(3f, () => { currentEnergy++;CollectionSystem.Instance.stageScanPoint++; });
        Debug.Log("Current Energy: " + currentEnergy);
    }
}
