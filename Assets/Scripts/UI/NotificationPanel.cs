using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class NotificationPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text notificationTMP;//알림 메시지를 표시

    //알림 패널에 표시
    public void Show(string _message)
    {
        notificationTMP.text = _message;
        Sequence sequence = DOTween.Sequence()//Sequence: 여러개의 트윈을 순차적으로 샐행하는 기능
            .Append(transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.InOutQuad))// 알림 패널의 스케일을 키우고
            .AppendInterval(0.9f)//일정 시간(0.9초) 동안 유지한 후
            .Append(transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InOutQuad));//다시 스케일을 줄이는 애니메이션을 추가
    }

    void Start()
    {
        ScaleZero();//패널 초기화
    }

    //패널이 화면에 나타날 때 사용
    [ContextMenu("ScaleOne")]//Inspector 창의 컴포넌트에 특정한 컨텍스트 메뉴를 추가하고, 해당 메뉴를 클릭했을 때 ScaleOne()이 실행 
    void ScaleOne()
    {
        transform.localScale = Vector3.one;
    }

    //패널이 화면에 나타나지 않도록 숨겨두는 역할
    [ContextMenu("ScaleZero")]
    public void ScaleZero()
    {
        transform.localScale = Vector3.zero;
    }
}
