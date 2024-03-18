using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order : MonoBehaviour
{
    [SerializeField] Renderer[] backRenderers = null;
    [SerializeField] Renderer[] middleRenderers = null;//카드
    [SerializeField] string sortingLayerName = null;

    private void Start()
    {
        SetOrder(0);
    }

    //카드 레이어 순서
    private void SetOrder(int order)
    {
        int mulOrder = order * 10;//카드가 순서대로 위로 올라가게 하기위해서 10을 곱함

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
