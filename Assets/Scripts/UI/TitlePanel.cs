using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TitlePanel : MonoBehaviour
{
   public void StartGameClick()//버튼을 누르면
   {
      GameManager.Inst.StartGame();//게임시작
      Active(false);//Start 비활성화
   }

   //Start창 비활성화
   public void Active(bool isActive)
   {
      gameObject.SetActive(isActive);//비활성화
   }
}
