using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//차트,UI,랭킹,게임오버
public class GameManager : MonoBehaviour
{
  public static GameManager Inst { get; private set; }

  //객체등록
  [SerializeField] private NotificationPanel notificationPanel;
  private void Awake()
  {
      Inst = this;
  }

  void Start()
  {
      StartGame();
  }

    // Update is called once per frame
    void Update()
    {
       #if UNITY_EDITOR
        InputCheatKey();
        #endif
    }

    #region Methods

    private void InputCheatKey()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))//1을 누르면
            TurnManager.OnAddCard?.Invoke(true);//내 카드에 추가, OnAddCard 이벤트를 호출, 이벤트에 연결된 메서드(이벤트 핸들러)가 실행
        
        if (Input.GetKeyDown(KeyCode.Alpha2))//2를 누르면
            TurnManager.OnAddCard?.Invoke(false); //상대카드를 추가
        
        if (Input.GetKeyDown(KeyCode.Alpha3))//3를 누르면
            TurnManager.Inst.EndTurn(); //턴 바뀜
      
    }

    public void StartGame()
    {
        StartCoroutine(TurnManager.Inst.StartGameCo());
    }

    //TurnManager에서 호출됨
    public void Notification(string _message)
    {
        notificationPanel.Show(_message);
    }

    #endregion
}
