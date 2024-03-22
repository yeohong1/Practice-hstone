using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order : MonoBehaviour
{
    [SerializeField] Renderer[] backRenderers = null;
    [SerializeField] Renderer[] middleRenderers = null;//카드
    [SerializeField] string sortingLayerName = null;//렌더링 순서를 제어하는 데 사용
    private int originOrder=0;

    public void SetOriginOrder(int originOrder)
    {
        this.originOrder = originOrder;
        SetOrder(originOrder);
    }

    public void SetMostFrontOrder(bool isMostFront)
    {
        SetOrder(isMostFront ? 100 : originOrder);
    }

   
    //카드 레이어 순서
    public void SetOrder(int order)
    {
        int mulOrder = order * 10;//각 카드가 위로 올라가는 정렬 순서를 조절

        foreach (var renderer in backRenderers)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = mulOrder;
        }

        foreach (var renderer in middleRenderers)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = mulOrder + 1; 
        }
    }
}
