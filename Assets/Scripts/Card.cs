using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;//부드러운 이동을 도움

public class Card : MonoBehaviour
{
   [SerializeField] private SpriteRenderer card;//SpriteRenderer: 스프라이트를 렌더링하는 데 사용되는 컴포넌트를 참조
   [SerializeField] private SpriteRenderer character;
   [SerializeField] private TMP_Text nameTMP;
   [SerializeField] private TMP_Text attackTMP;
   [SerializeField] private TMP_Text healthTMP;
   [SerializeField] private Sprite cardFront;// Sprite: 실제 스프라이트 이미지 자체를 나타내는 변수
   [SerializeField] private Sprite cardBack;

   public Item item;
   private bool isFront;
   public PRS originPRS;//이동을 해도 다시 원본으로 위치로 오기 위함

   #region Events

   //마우스가 콜라이더 영역에 올라가 있는 상태 시 호출
   private void OnMouseOver()
   {
      if(isFront)
         CardManager.Inst.CardMouseOver(this);
   }

   //마우스가 콜라이더 영역에 올라지 않은 상태 시 호출
   private void OnMouseExit()
   {
      if(isFront)
         CardManager.Inst.CardMouseExit(this);
   }
   
   //마우스를 누르는 순간 호출
    void OnMouseDown()
   {
      if (isFront)
         CardManager.Inst.CardMouseDown();
   }

   //마우스를 떼는 순간 호출
    void OnMouseUp()
   {
      if(isFront)
         CardManager.Inst.CardMouseUp();
   }


   #endregion
   
   
   
   
   #region Methods
   
   public void Setup(Item _item, bool _isFront)//해당 리스트에서 지워진 아이템객체 받음,내 카드인지 아닌지 받음
   {
      this.item = _item;//_item을 받아서 item속성 넣어줌
      this.isFront = _isFront;

      if (this.isFront)//만약 카드가 내 카드라면(앞면)
      {
         character.sprite = this.item.Sprite;
         nameTMP.text = this.item.name;
         attackTMP.text = this.item.attack.ToString();
         healthTMP.text = this.item.health.ToString();
      }
      else
      {
         card.sprite = cardBack;
         nameTMP.text = "";
         attackTMP.text = "";
         healthTMP.text = "";
      }
   }

   //카드 움직임
   public void MoveTransform(PRS prs, bool useDotween, float dotweenTime = 0)
   {
      //만약 참이면 DoTween 라이브러리를 사용하여 부드러운 애니메이션 처리를 수행
      //주어진 시간(dotweenTime) 동안에 변화를 적용
      if (useDotween)
      {
         transform.DOMove(prs.pos, dotweenTime);//주어진 위치로 dotweenTime동안에 이동
         transform.DORotateQuaternion(prs.rot, dotweenTime);//회전
         transform.DOScale(prs.scale, dotweenTime);//크기
      }
      else
      {
         transform.position = prs.pos;
         transform.rotation = prs.rot;
         transform.localScale = prs.scale;
      }
   }
   #endregion
   

   

}
