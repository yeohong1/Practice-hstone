using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//차트,UI,랭킹,게임오버
public class GameManager : MonoBehaviour
{
  public static GameManager Inst { get; private set; }

  [Multiline(10)] //해당 문자열을 입력할 때 10줄까지 허용
  [SerializeField] private string cheatInfo;
  
  //객체등록
  [SerializeField] private NotificationPanel notificationPanel;
  [SerializeField] private ResultPanel resultPanel;
  [SerializeField] private TitlePanel titlePanel;
  [SerializeField] private CameraEffect cameraEffect;
  [SerializeField] private GameObject endTurnBtn;
  
  private WaitForSeconds delay2 = new WaitForSeconds(2);
  
  private void Awake()
  {
      Inst = this;
  }

  private void Start()
  {
      UISetup();
  }

  //UI초기화
  private void UISetup()
  {
      notificationPanel.ScaleZero();//턴알림 창
      resultPanel.ScaleZero();//결과알림창
      titlePanel.Active(true);
      cameraEffect.SetGrayScale(false);
  }

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
        
        if (Input.GetKeyDown(KeyCode.Alpha3))//3을 누르면
            TurnManager.Inst.EndTurn(); //턴 바뀜

        if (Input.GetKeyDown(KeyCode.Alpha4)) //4를 누르면
            CardManager.Inst.TryPutCard(false);//상대를 강제로 카드를 내게 함
        
        if (Input.GetKeyDown(KeyCode.Alpha5)) //5를 누르면
            EntityManager.Inst.DamageBoss(true,19);//내 보스를 강제로 데미지19입힘
        
        if (Input.GetKeyDown(KeyCode.Alpha6)) //6을 누르면
            EntityManager.Inst.DamageBoss(false,19);//상대 보스를 강제로 데미지19입힘

    }
    

    //게임시작
    public void StartGame()
    {
        StartCoroutine(TurnManager.Inst.StartGameCo());
    }

    //TurnManager에서 호출됨
    public void Notification(string _message)
    {
        notificationPanel.Show(_message);
    }

    public IEnumerator GameOver(bool isMyWin)
    {
        TurnManager.Inst.isLoading = true;//클릭방지
        endTurnBtn.SetActive(false);//종료버튼 비활성화
        yield return delay2;//2초대기 후


        TurnManager.Inst.isLoading = true;
        resultPanel.Show(isMyWin ? "승리":"패배");//결과 값 전달하여 보여줌
        cameraEffect.SetGrayScale(true);
    }

    #endregion
}
