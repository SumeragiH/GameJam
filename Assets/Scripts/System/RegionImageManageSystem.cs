using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionImageManageSystem : SingletonBaseWithMono<RegionImageManageSystem>
{
    [SerializeField] private List<RegionImageView> regionImages;

    public void SetHighlightedRegion(int index)
    {
        for (int i = 0; i < regionImages.Count; i++)
        {
            if (i == index)
            {
                regionImages[i].SetHighlight();
            }
            else
            {
                regionImages[i].InactiveHighlight();
            }
        }
    }

    public void ResetHighlights()
    {
        foreach (var regionImage in regionImages)
        {
            regionImage.InactiveHighlight();
        }
    }
}
