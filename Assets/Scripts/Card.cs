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

   public void Setup(Item _item, bool _isFront)
   {
      this.item = _item;//_item을 받아서 item속성 넣어줌
      this.isFront = _isFront;

      if (this.isFront)//만약 카드가 앞면이라면
      {
         character.sprite = this.item.Sprite;
         nameTMP.text = this.item.name;
         attackTMP.text = this.item.sttack.ToString();
         attackTMP.text = this.item.sttack.ToString();
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
      //만약 참이면 부드럽게 움직이게 해줌
      if (useDotween)
      {
         transform.DOMove(prs.pos, dotweenTime);
         transform.DORotateQuaternion(prs.rot, dotweenTime);
         transform.DOScale(prs.scale, dotweenTime);
      }
      else
      {
         transform.position = prs.pos;
         transform.rotation = prs.rot;
         transform.localScale = prs.scale;
      }
   }
   

}
