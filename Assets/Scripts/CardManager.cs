using System;
using System.Collections;
using System.Collections.Generic;
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
   [SerializeField] private Transform myCardLeft;
   [SerializeField] private Transform myCardRight;
   [SerializeField] private Transform otherCardLeft;
   [SerializeField] private Transform otherCardRight;
   
   private List<Item> itemBuffer;//카드 아이템을 임시로 저장하는데 사용 

   #region Events

   private void Start()
   {
      SetupItemBuffer();
   }

   private void Update()
   {
      if(Input.GetKeyDown(KeyCode.Alpha1))//내 카드 뽑을 시
         AddCard(true);
      if (Input.GetKeyDown(KeyCode.Alpha2))//상대 카드 뽑을 시
         AddCard(false);
   }

   #endregion

   #region Methods

   //카드를 뽑을 때 호출되는 함수
   public Item PopItem()
   {
      if(itemBuffer.Count == 0)//만약 리스트가 0이 되면
         SetupItemBuffer();//다시 채워줌

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
         var targetCard = isMine ? myCards[i] : otherCards[i];//해당 카드가
         targetCard?.GetComponent<Order>().SetOriginOrder(i);//null이 아니면 SetOriginOrder호출
      }
   }

   //뽑은카드 위치 설정
   private void CardAlignment(bool isMine)
   {
      List<PRS> originCardPRSs = new List<PRS>();
      if (isMine)
         originCardPRSs = RoundAlignment(myCardLeft, myCardRight, myCards.Count, 0.5f, Vector3.one * 1.9f);//List<PRS>리스트로 반환을 받음
      else
      {
         originCardPRSs = RoundAlignment(otherCardLeft, otherCardRight, otherCards.Count, -0.5f, Vector3.one * 1.9f);
      }
      
      var targetCards = isMine ? myCards : otherCards;//해당 리스트 할당
      for (int i = 0; i < targetCards.Count; i++)
      {
         var targetCard = targetCards[i];//해당 카드찾고

         targetCard.originPRS = originCardPRSs[i];//PRS설정
         targetCard.MoveTransform(targetCard.originPRS,true,0.7f);//Card스크립트 MoveTransform함수에 값 전달
      }
   }

   List<PRS> RoundAlignment(Transform leftTr, Transform rightTr, int objCount, float height, Vector3 scale) //RoundAlignment함수를 호출하면, 둥글게 정렬을하여 List<PRS>를 반환함
   {
      float[] objLerps = new float[objCount];//0-1까지 위치에서 자신의 위치가 얼마나 변하는지, objCont= 해당리스트 Count
      List<PRS> results = new List<PRS>(objCount);

      switch (objCount)
      {
         case 1:
            objLerps = new float[] { 0.5f };
            break;
         case 2:
            objLerps = new float[] { 0.27f,0.73f};
            break;
         case 3:
            objLerps = new float[] { 0.1f,0.5f,0.9f };
            break;
         default://카드가 4개이상 부터 회전 공식
            float interval = 1f / (objCount - 1);
            for (int i = 0; i < objCount; i++)
               objLerps[i] = interval * i;
            break;
      }
      
      //원의 방정식
      for (int i = 0; i < objCount; i++)
      {
         var targetPos = Vector3.Lerp(leftTr.position, rightTr.position, objLerps[i]);
         var targetRot = Utils.QI;
         if (objCount >= 4)//카드가 4이상이면
         {
            float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));//곡선 계산식
            curve = height >= 0 ? curve : -curve;//높이가 0이상이면 curve,미만이면 -curve
            targetPos.y += curve;//y축에 더함
            targetRot = Quaternion.Slerp(leftTr.rotation, rightTr.rotation, objLerps[i]);
         }
         results.Add(new PRS(targetPos,targetRot,scale));//카드가 4미만이면 바로 반환
      }

      return results;
   }
   #endregion


}
