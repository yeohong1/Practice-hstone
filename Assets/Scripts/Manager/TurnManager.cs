using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TurnManager : MonoBehaviour
{
   public static TurnManager Inst { get; private set; }//싱글톤

   [Header("Develop")] 
   [SerializeField] [Tooltip("시작 턴 모드를 정합니다")]ETurnMode eTurnMode;//마우스를 올렸을 때 글자가 뜨게 함
   [SerializeField] [Tooltip("카드 배분이 매우 빨라집니다")] private bool fastMode;
   [SerializeField] [Tooltip("시작 카드 개수를 정합니다")]private int startCardCont;

   [Header("Properties")] 
   public bool isLoading;//게임 끝나면 isLoading을 true로 하면 카드와 엔티티 클릭방지
   public bool myTurn;

   enum ETurnMode{Random,My,other}
   private WaitForSeconds delay05 = new WaitForSeconds(0.5f);
   private WaitForSeconds delay07 = new WaitForSeconds(0.7f);

   public static Action<bool> OnAddCard;//델리게이트 이벤트 타입, 카드가 추가될 때 호출될 콜백(이벤트 핸들러)을 나타냄
   public static event Action<bool> OnTurnStarted;//_isMine이 들어감 
   
   
   
   #region Events
   private void Awake()
   {
      Inst = this;
   }
   #endregion

   #region Methods
   //게임 순서 정함
   private void GameSetup()
   {
      if (fastMode)
         delay05 = new WaitForSeconds(0.05f);
      
      switch (eTurnMode)
      {
         case ETurnMode.Random:
            myTurn = Random.Range(0, 2) == 0;//0-1 중 만약 0이면 true
            break;
         case ETurnMode.My:
            myTurn = true;
            break;
         case ETurnMode.other:
            myTurn = false;
            break;
         
      }
   }

   //카드배분
   public IEnumerator StartGameCo()
   {
      GameSetup();//게임 순서 정함
      isLoading = true;//카드 클릭방지

      for (int i = 0; i < startCardCont; i++)//시작 카드개수 만큼 반복
      {
         yield return delay05; //0.5초 대기 후   
         OnAddCard?.Invoke(false);//null이 아니면 false(상대카드)
         yield return delay05;//0.5초 대기 후 
         OnAddCard?.Invoke(true);////null이 아니면 true(내카드)
      }

      StartCoroutine(StartTurnCo());
   }

   //턴이 시작될 시 카드 1장 추가 위함
   IEnumerator StartTurnCo()
   {
      isLoading = true;//카드 클릭방지
      
      if(myTurn)
         GameManager.Inst.Notification("My Turn");//GameManager 함수 Notification에 메세지 전달

      yield return delay07;//0.7초 대기 후 
      OnAddCard?.Invoke(myTurn);//만약 내 턴이면 나한테 카드 1개 추가, 아니면 상대한테 카드 1개 추가
      yield return delay07;
      isLoading = false;
      OnTurnStarted?.Invoke(myTurn);
   }

   //턴 넘기기 위함
   public void EndTurn()
   {
      myTurn = !myTurn;//myTurn을 뒤집음
      StartCoroutine(StartTurnCo());
   }
   
   

   #endregion
}
