using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultPanel : MonoBehaviour
{
   [SerializeField] private TMP_Text resultTMP;
   

   private void Start()
   {
      ScaleZero();//결과창 크기를 0으로 설정
   }
   
   //결과창 보여줌
   public void Show(string message)
   {
      resultTMP.text = message;
      transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutQuad);//0.5초동안 결과장 크기1로 설정
   }

   //다시시작
   public void Restart()
   {
      SceneManager.LoadScene(0);
   }

   //결과창 크기를 1로 설정
   [ContextMenu("ScaleOne")]
   void ScaleOne()
   {
      transform.localScale = Vector3.one;
   }

   //결과창 크기를 0으로 설정
   [ContextMenu("ScaleZero")]
   public void ScaleZero()
   {
      transform.localScale = Vector3.zero;
   }
}
