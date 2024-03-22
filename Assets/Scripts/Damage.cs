using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Damage : MonoBehaviour
{
    [SerializeField] private TMP_Text damageTMP;
    private Transform tr;
    
   
    
    //데미지 설정위치로 옮기기
    void Update()
    {
        if (tr != null)
            transform.position = tr.position;
    }

    //데미지 위치설정
    public void SetupTransform(Transform tr)
    {
        this.tr = tr;
    }

    public void Damaged(int damage)
    {
        if(damage <= 0)
            return;//데미지 안 보임
        
        GetComponent<Order>().SetOrder(1000);//레이어 1000설정
        damageTMP.text = $"-{damage}";

        Sequence sequence = DOTween.Sequence()
            .Append(transform.DOScale(Vector3.one * 1.8f, 0.5f).SetEase(Ease.InOutBack))//0.5초 동안 1.8만큼 크기를 키움
            .AppendInterval(1.2f)//1.2초 대기 후
            .Append(transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutBack))//0.5초 0만큼 크기를 축소함
            .OnComplete(() => Destroy(gameObject));//데미지 객체 파괴
    }
}
