using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EntityManager : MonoBehaviour
{
    public static EntityManager Inst { get; private set; }//싱글톤

    private void Awake()
    {
        Inst = this;
    }

    [SerializeField] private GameObject entityPrefab;
    //리스트
    [SerializeField] private List<Entity> myEntities;
    [SerializeField] private List<Entity> otherEntities;
    
    [SerializeField] private Entity myEmptyEntity;
    [SerializeField] private Entity mybossEntity;
    [SerializeField] private Entity otherBossEntity;

    private const int MAX_ENTITY_COUNT = 6;
    public bool IsFullMyEntities => myEntities.Count >= MAX_ENTITY_COUNT && !ExistMyEmptyEntity;
    private bool IsFullOtherEntities => otherEntities.Count >= MAX_ENTITY_COUNT;
    private bool ExistMyEmptyEntity => myEntities.Exists(x => x == myEmptyEntity);//Exists:리스트 내에서 myEmptyEntity객체가 있는지 검사, 조건을 만족한다면 true를 반환
    private int MyEmptyEntityIndex => myEntities.FindIndex(x => x == myEmptyEntity);//myEmptyEntity객체가 없으면 -1반환, 조건을 만족한다면 해당 요소의 인덱스를 반환

    private WaitForSeconds delay1 = new WaitForSeconds(1);
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
    
    
    #region Methods

    private void OnTurnStarted(bool myTurn)
    {
        if (!myTurn)//내 턴이 아니면
            StartCoroutine(AICo());
    }

    //상대 entity 내기
    IEnumerator AICo()
    {
        CardManager.Inst.TryPutCard(false);
        yield return delay1;//1초 대기
        
        //공격로직
        TurnManager.Inst.EndTurn();//호출하면 내 턴으로 돌아옴
    }
    
    private void EntityAlignment(bool _isMine)
    {
        float targetY = _isMine ? -4.35f : 4.15f;//내 카드면 아래쪽에 위치 아니면 위쪽에 위치
        var targetEntities = _isMine ? myEntities : otherEntities;//해당 리스트 할당

        for (int i = 0; i < targetEntities.Count; i++)//해당 리스트만큼 반복
        {
            float targetX = (targetEntities.Count - 1) * -3.4f + i * 6.8f;//각 카드마다 x위치 계산

            var targetEntity = targetEntities[i];
            targetEntity.originPos = new Vector3(targetX, targetY, 0);//각 카드 위치설정
            targetEntity.MoveTransform(targetEntity.originPos,true,0.5f);//카드 움직임
            targetEntity.GetComponent<Order>()?.SetOriginOrder(i);//레이어설정(카드index가 커질수록 앞쪽에 위치)
        }
    }

    // myEmptyEntity를 특정 위치에 삽입
    public void InsertMyEmptyEntity(float _xPos)
    {
        if(IsFullMyEntities)
            return;
        if(!ExistMyEmptyEntity)//내 Entity가 존재하지 않으면 
            myEntities.Add(myEmptyEntity);//내 Entity리스트에 entity추가, 최소 1개의 Entity있음

        Vector3 emptyEntitiyPos = myEmptyEntity.transform.position;//myEmptyEntity의 현재위치 가져옴
        emptyEntitiyPos.x = _xPos;//매개변수로 전달된 x 좌표값 할당
        myEmptyEntity.transform.position = emptyEntitiyPos;//myEmptyEntity위치 설정

        int _emptyEntityIndex = MyEmptyEntityIndex;//index번호 가져옴   
        
        myEntities.Sort((entity1,entity2)=>entity1.transform.position.x.CompareTo(entity2.transform.position.x));//매개변수 비교하며 정렬
        if (MyEmptyEntityIndex != _emptyEntityIndex)//인덱스 바뀜을 판단하여 바꼈으면 
            EntityAlignment(true);//정렬
        
    }

    //필드에 카드 취소하기
    public void RemoveMyEmptyEntity()
    {
        if(!ExistMyEmptyEntity)//내 EmptyEntity가 존재하지 않으면
            return;
        
        myEntities.RemoveAt(MyEmptyEntityIndex);//제거할 요소의 인덱스를 받아 리스트에서 지움 
        EntityAlignment(true);//정렬
    }

    //entity 낼 때
    public bool SpawnEntity(bool _isMine, Item _item, Vector3 _sqawnPos)
    {
        if (_isMine)
        {
            if (IsFullMyEntities || !ExistMyEmptyEntity)//내 엔티티가 다 차있거나 엔티티가 존재하지 않으면
                return false;
        }
        else//상대가
        {
            if (IsFullOtherEntities)//만약 엔티티가 다 차 있으면
                return false;
        }

        var entityObject = Instantiate(entityPrefab, _sqawnPos, Utils.QI);
        var entity = entityObject.GetComponent<Entity>();

        if (_isMine)
            myEntities[MyEmptyEntityIndex] = entity;
        else //상대
            otherEntities.Insert(Random.Range(0, otherEntities.Count),entity);

        entity.isMine = _isMine;
        entity.Setup(_item);//해당 entity속성 설정
        EntityAlignment(_isMine);//내 Entity 정렬

        return true;
    }

    #endregion
}
