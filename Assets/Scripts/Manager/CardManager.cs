using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class CardManager : MonoBehaviour
{
   public static CardManager Inst { get; private set; }// private set으로 설정되어 외부에서 값을 설정할 수 없음, 외부에서 읽기만 가능
   void Awake() => Inst = this;

   [SerializeField] private ItemSO itemSO;
   [SerializeField] private GameObject cardPrefab;
   [SerializeField] private List<Card> myCards;
   [SerializeField] private List<Card> otherCards;
   [SerializeField] private Transform cardSpawnPoint;
   [SerializeField] private Transform otherCardSpawnPoint;
   [SerializeField] private Transform myCardLeft;
   [SerializeField] private Transform myCardRight;
   [SerializeField] private Transform otherCardLeft;
   [SerializeField] private Transform otherCardRight;
   [SerializeField] private ECardState eCardState;
   
   private List<Item> itemBuffer;//카드 아이템을 임시로 저장하는데 사용 
   private Card selectCard;
   private bool isMyCardDrag;
   private bool onMyCardArea;
   private int myPutCount = 0;

   //마우스 상태
   enum ECardState
   {
      Nothing,CanMouseOver,CanMouseDrag
   }

   #region Events

   private void Start()
   {
      SetupItemBuffer();
      TurnManager.OnAddCard += AddCard;//OnAddCard 이벤트에 이벤트 핸들러(AddCard)를 추가
      TurnManager.OnTurnStarted += OnTurnStarted;
   }

   private void Update()
   {
      if (isMyCardDrag)
         CardDrag();

      DetectCardArea();
      SetECardState();
   }
   
   private void OnDestroy()
   {
      TurnManager.OnAddCard -= AddCard;//OnAddCard 이벤트에 이벤트 핸들러(AddCard)를 제거
      TurnManager.OnTurnStarted -= OnTurnStarted;
   }

   private void OnTurnStarted(bool _myTurn)
   {
      if (_myTurn)
         myPutCount = 0;//entity놓은 수 초기화
   }

   #endregion

   #region Methods

   //카드를 뽑을 때 호출되는 함수
   public Item PopItem()
   {
      //만약 리스트가 0이 되면
      if (itemBuffer.Count == 0) 
      {
         SetupItemBuffer(); //다시 채워줌
      }

      Item item = itemBuffer[0];//맨 첫번째 아이템
      itemBuffer.RemoveAt(0);//맨 첫번째 인덱스를 지움
      return item;//지워진 아이템 반환
      
   }
   
   //랜덤으로 카드 속성 넣기
  private void SetupItemBuffer()
   {
      itemBuffer = new List<Item>(112);//112개의 공간 잡아놈
      for (int i = 0; i < itemSO.items.Length; i++)//itemSO.items.Length=12개
      {
         Item item = itemSO.items[i];//직접 넣은 아이템 속성을 item에 넣기
         for (int j = 0; j < item.percent; j++)//넣은 item에 item.percent 속성만큼 itemBuffer리스트에 추가,  아이템의 등장 확률을 고려하여 percent만큼 itemBuffer 리스트에 아이템을 추가
         {
            itemBuffer.Add(item);
         }
      }

         for (int i = 0; i < itemBuffer.Count; i++)// itemBuffer 리스트에 있는 아이템들의 순서를 무작위로 섞는 과정
         {
            int rand = Random.Range(i, itemBuffer.Count);//현재 인덱스(i)부터 리스트의 크기(itemBuffer.Count) 사이에서 무작위로 인덱스를 선택
            Item temp = itemBuffer[i];//현재 인덱스[i]에 있는 아이템을 temp에 임시로 넣어줌
            itemBuffer[i] = itemBuffer[rand];//현재 아이템을 무작위로 변경함
            itemBuffer[rand] = temp;// 임시보관한 원래 현재 아이템을 rand에 넣어줌
         }
      
   }

  //카드 뽑을 시
   private void AddCard(bool isMine)
   {
      var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);//프리팹 생성
      var card = cardObject.GetComponent<Card>();//card프리팹에 있는 컴포넌트 Card스크립트 참조
      card.Setup(PopItem(),isMine);//리스트에서 지워진 아이템,내 카드인지 상대편 카드인지 bool값 전달
      (isMine ? myCards: otherCards).Add(card);//내 카드면 myCards리스트에 카드 추가, 상대편 카드면 otherCards리스트에 카드 추가
      
      SetOriginOrder(isMine);
      CardAlignment(isMine);
   }

   //카드 뽑을 때 레이어 설정
   private void SetOriginOrder(bool isMine)
   {
      int count = isMine ? myCards.Count : otherCards.Count;
      for (int i = 0; i < count; i++)
      {
         var targetCard = isMine ? myCards[i] : otherCards[i];//해당리스트에 있는 카드 Count
         targetCard?.GetComponent<Order>().SetOriginOrder(i);//null이 아니면 SetOriginOrder호출
      }
   }

   //뽑은카드 위치 설정
   private void CardAlignment(bool isMine)
   {
      List<PRS> originCardPRSs = new List<PRS>();
      if (isMine)//내 카드면 
         originCardPRSs = RoundAlignment(myCardLeft, myCardRight, myCards.Count, 0.5f, Vector3.one * 1.9f);//List<PRS>리스트로 반환을 받음
      else
      {
         originCardPRSs = RoundAlignment(otherCardLeft, otherCardRight, otherCards.Count, -0.5f, Vector3.one * 1.9f);
      }
      
      var targetCards = isMine ? myCards : otherCards;
      for (int i = 0; i < targetCards.Count; i++)
      {
         var targetCard = targetCards[i];//targetCard=현재의 카드를 나타냄

         targetCard.originPRS = originCardPRSs[i];//해당 카드의 초기 위치, 회전, 크기 정보를 설정, 해당리스트 카드프리팹에 Card스크립트가 있어 originPRS호출 가능
         targetCard.MoveTransform(targetCard.originPRS,true,0.7f);//Card스크립트 MoveTransform함수에 값 전달
      }
   }

   List<PRS> RoundAlignment(Transform leftTr, Transform rightTr, int objCount, float height, Vector3 scale) //RoundAlignment함수를 호출하면, 둥글게 정렬을하여 List<PRS>를 반환함
   {
      float[] objLerps = new float[objCount];//0-1까지 위치에서 자신의 위치가 얼마나 변하는지, objCont= 해당리스트 Count
      List<PRS> results = new List<PRS>(objCount);//반환할 리스트

      switch (objCount)
      {
         case 1://첫번째 카드면
            objLerps = new float[] { 0.5f };//카드를 중앙에 위치 0.5f 반환
            break;
         case 2://두번째 카드
            objLerps = new float[] { 0.27f,0.73f};
            break;
         case 3://세번째 카드
            objLerps = new float[] { 0.1f,0.5f,0.9f };
            break;
         default://카드가 4개이상 부터 회전 공식
            float interval = 1f / (objCount - 1);//카드 사이의 간격 계산, 각 카드 사이의 거리가 동일하게 유지하기 위함
            for (int i = 0; i < objCount; i++)//카드수만큼 반복
               objLerps[i] = interval * i;//각 카드사이의 간격 * 현재 리스트에 있는 카드 수 (0~1 카드수만큼 쪼갬), objLerps[i]=카드의 위치를 나타내는 값들을 담는 배열
            break;
      }
      
      //원의 방정식
      for (int i = 0; i < objCount; i++)
      {
         var targetPos = Vector3.Lerp(leftTr.position, rightTr.position, objLerps[i]);//(보간시작점, 보간끝점, 보간의 가중치(0-1)),현재 카드의 위치 계산
         var targetRot = Utils.QI;//현재 카드의 회전을 기본값으로 설정
         if (objCount >= 4)//카드가 4이상이면
         {
            float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));// // 카드의 높이에 따른 곡선을 계산
            curve = height >= 0 ? curve : -curve;//높이가 0이상이면(내 카드) curve,미만이면(상대카드) -curve, 카드의 높이에 따라서 곡선이 위로 또는 아래로 향하게 함
            targetPos.y += curve;//곡선을 적용하여 카드의 y 축 위치를 조정
            targetRot = Quaternion.Slerp(leftTr.rotation, rightTr.rotation, objLerps[i]); //왼쪽에서 오른쪽으로 이동하는 동안 카드의 회전을 부드럽게 보간
         }
         results.Add(new PRS(targetPos,targetRot,scale));//카드가 4미만이면 바로 반환, 각각의 카드에 대한 위치, 회전, 크기를 PRS 객체로 묶어서 results 리스트에 추가
      }  // results 리스트는 모든 카드에 대한 위치, 회전, 크기 정보를 담고 있음

      return results;
   }

   //entity를 놓을 시
   public bool TryPutCard(bool _isMine)
   {
      if (_isMine && myPutCount >= 1)//나의 entity이고 낸 카드가 1이상으면
         return false;//더이상 카드를 내지 못함

      if (!_isMine && otherCards.Count <= 0)//상대이고 상대방카드 리스트가 0이하이면
         return false;//카드를 내지 못함

      Card card = _isMine ? selectCard : otherCards[Random.Range(0,otherCards.Count)];//내 카드면 확대한 카드할당 상대면 랜덤으로 카드 할당
      var spawnPos = _isMine ? Utils.MousePos : otherCardSpawnPoint.position;//내 카드면 마우스드레그 위치로
      var targetCards = _isMine ? myCards : otherCards;

      if (EntityManager.Inst.SpawnEntity(_isMine, card.item, spawnPos))
      {
         targetCards.Remove(card);//카드를 냈으니까 해당 리스트에서 지움
         card.transform.DOKill();//Transform에 적용된 모든 DOTween 애니메이션을 즉시 중지
         DestroyImmediate(card.gameObject);//즉시 해당 카드(프리팹)를 지움
         if (_isMine)
         {
            selectCard = null;//다음 카드를 받을 준비를 해줌
            myPutCount++;//카드를 냈다라는 것을 저장해줌
         }
         CardAlignment(_isMine);
         return true;
      }
     
         targetCards.ForEach(x=>x.GetComponent<Order>().SetMostFrontOrder(false));//모두 원래 레이어로 설정
         CardAlignment(_isMine);//정렬
         return false;
      
      
   }

   #region MyCard

   public void CardMouseOver(Card _card)//마우스 올렸을 때
   {
      if(eCardState == ECardState.Nothing)//카드배분상태 시
         return;//아무런 동작 안 함
     
         selectCard = _card;//확대 한 카드를 할당
         EnlargeCard(true,_card);
      
     
   }

   public void CardMouseExit(Card _card)//마우스 안 올렸을 때
   {
      EnlargeCard(false,_card);
   }

   //마우스를 누르는 순간 드레그 가능
   public void CardMouseDown()
   {
      if(eCardState != ECardState.CanMouseDrag)//내 턴이 아니면
         return;//아무런 동작도 하지않음
      
         isMyCardDrag = true;
      
     
   }

   //마우스를 떼는 순간 드레그 못함
   public void CardMouseUp()
   {
      isMyCardDrag = false;
      
      if(eCardState != ECardState.CanMouseDrag)//내 턴이 아니면
         return;//아무런 동작도하지않음

      if (onMyCardArea) //마우스가 해당영역에 있으면
         EntityManager.Inst.RemoveMyEmptyEntity(); //내 entity를 리스트에서 지움
      else//마우스가 해당영역에 없으면
         TryPutCard(true);

   }
   
   private void CardDrag()
   {
      if(eCardState != ECardState.CanMouseDrag)//내 턴이 아니면
         return;//아무런 동작도 하지않음
      
      if (!onMyCardArea)//마우스가 해당 영역을 벗어나면
      {
         selectCard.MoveTransform(new PRS(Utils.MousePos,Utils.QI,selectCard.originPRS.scale),false);//해당 카드 위치설정:마우스위치, 회전:기본값, 크기는: 초기크기로
         EntityManager.Inst.InsertMyEmptyEntity(Utils.MousePos.x);//마우스 위치를 전달
      }
   }

   //마우스가 해당 영역에 들어가 있는지 여부를 판단
   private void DetectCardArea()
   {
      RaycastHit2D[] hits = Physics2D.RaycastAll(Utils.MousePos, Vector3.forward);//마우스 위치에서 z 축 방향으로 Raycast를 발사하여 충돌하는 모든 객체를 가져옴
      int layer = LayerMask.NameToLayer("MyCardArea");//"MyCardArea"라는 이름의 레이어의 정수 값을 가져옴
      onMyCardArea = Array.Exists(hits, x => x.collider.gameObject.layer == layer);//LINQ메서드 중 하나, hits배열을 반복하면서 충돌한 객체 레이어가 MyCardArea리면 true반환

   }

   //카드확대 시킴
   private void EnlargeCard(bool _isEnlarge, Card _card)
   {
      if (_isEnlarge)
      {
         Vector3 enlargePos = new Vector3(_card.originPRS.pos.x, -4.8f, -10f);//위치 조정
         _card.MoveTransform(new PRS(enlargePos,Utils.QI,Vector3.one *3.5f),false);//해당카드에 위치,회전,크기 설정
      }
      else
      {
         _card.MoveTransform(_card.originPRS,false);//원래 위치 그대로
         
        
      }
      _card.GetComponent<Order>().SetMostFrontOrder(_isEnlarge);//레이어 설정
     
   }
   
   //카드 드레그 및 올렸을 때 상태, update에서 계속 확인함
   private void SetECardState()
   {
      if (TurnManager.Inst.isLoading)//카드배분상태
         eCardState = ECardState.Nothing;//Nothing
      
      else if (!TurnManager.Inst.myTurn||myPutCount==1||EntityManager.Inst.IsFullMyEntities)//상태턴 or entity를1개 놈 or entity가 다 차있을 때
         eCardState = ECardState.CanMouseOver;//CanMouseOver만
      
      else if (TurnManager.Inst.myTurn && myPutCount==0)//내 턴이고 entity를 놓지 않았을 때
         eCardState = ECardState.CanMouseDrag;//CanMouseDrag
   }

   #endregion
   #endregion


}
