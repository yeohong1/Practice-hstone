using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnButton : MonoBehaviour
{
   [SerializeField] private Sprite active;
   [SerializeField] private Sprite inactive;
   [SerializeField] private TMP_Text btnText;

   private void Start()
   {
      Setup(false);
      TurnManager.OnTurnStarted += Setup;//내 턴이면 true, 상대면 false
   }

   private void OnDestroy()//호출 안됨
   {
      TurnManager.OnTurnStarted -= Setup;
   }

   public void Setup(bool _isActive)
   {
      GetComponent<Image>().sprite = _isActive ? active : inactive;
      GetComponent<Button>().interactable = _isActive;
      btnText.color = _isActive ? new Color32(225, 195, 90, 255) : new Color32(55, 55, 55, 255);
   }
}
