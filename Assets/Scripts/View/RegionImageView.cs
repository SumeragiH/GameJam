using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegionImageView : MonoBehaviour
{
    [SerializeField] private Color dehighlightColor;
    [SerializeField] private Image imageRegion;

    void Start()
    {
        imageRegion.color = dehighlightColor;
    }
    public void SetHighlight()
    {
        imageRegion.color = Color.white;
    }

    public void InactiveHighlight()
    {
        imageRegion.color = dehighlightColor;
    }

}
