using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEditor;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] private Item item;
    [SerializeField] private SpriteRenderer entity;
    [SerializeField] private SpriteRenderer character;
    [SerializeField] private TMP_Text nameTMP;
    [SerializeField] private TMP_Text attackTMP;
    [SerializeField] private TMP_Text healthTMP;
    [SerializeField] private GameObject sleepParticle;
    
    public int attack;
    public int health;
    public bool isMine;
    public bool isBossOrEmpty;
    public Vector3 originPos;//정렬을 위함
    private int liveCount;

    #region Events

    private void Start()
    {
        TurnManager.OnTurnStarted += OnTurnStarted;
    }

    private void OnDestroy()
    {
        TurnManager.OnTurnStarted -= OnTurnStarted;
    }
    

    #endregion

    void OnTurnStarted(bool myTurn)
    {
        if(isBossOrEmpty)
            return;

        if (isMine == myTurn)//나의 턴이 돌아왔을 때
            liveCount++;
        
        sleepParticle.SetActive(liveCount <1);//liveCount가 0이면 true(잠자는 파티클)
    }
    
    //해당 entity에 속성 넣음
    public void Setup(Item _item)
    {
        attack = _item.attack;//아이템 스크립트에 있는 공격력 할당
        health = _item.health;//체력할당

        this.item = _item;
        character.sprite = this.item.Sprite;//Item 스크립트에 있는 캐릭터를 현재 스크립트에 할당
        nameTMP.text = this.item.name;
        attackTMP.text = attack.ToString();
        healthTMP.text = health.ToString();
    }

    public void MoveTransform(Vector3 pos, bool useDotween, float dotweenTime = 0)
    {
        if (useDotween)
            transform.DOMove(pos, dotweenTime);
        else
            transform.position = pos;
    }
   


}
