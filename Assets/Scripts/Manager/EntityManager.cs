using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using DG.Tweening;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;


public class EntityManager : MonoBehaviour
{
    public static EntityManager Inst { get; private set; }//싱글톤

    private void Awake()
    {
        Inst = this;
    }
    
    //프리팹
    [SerializeField] private GameObject entityPrefab;
    [SerializeField] private GameObject damagePrefab;
    
    //리스트
    [SerializeField] private List<Entity> myEntities;
    [SerializeField] private List<Entity> otherEntities;

    [SerializeField] private GameObject TargetPicker;
    [SerializeField] private Entity myEmptyEntity;
    [SerializeField] private Entity mybossEntity;
    [SerializeField] private Entity otherBossEntity;

    private const int MAX_ENTITY_COUNT = 6;
    public bool IsFullMyEntities => myEntities.Count >= MAX_ENTITY_COUNT && !ExistMyEmptyEntity;
    private bool IsFullOtherEntities => otherEntities.Count >= MAX_ENTITY_COUNT;
    private bool ExistTargetPickEntity => targetPickEntity != null;//마우스를 끌어다가 선택한 entity가 null이 아닐 때
    private bool ExistMyEmptyEntity => myEntities.Exists(x => x == myEmptyEntity);//Exists:리스트 내에서 myEmptyEntity객체가 있는지 검사, 조건을 만족한다면 true를 반환
    private int MyEmptyEntityIndex => myEntities.FindIndex(x => x == myEmptyEntity);//myEmptyEntity객체가 없으면 -1반환, 조건을 만족한다면 해당 요소의 인덱스를 반환
    private bool CanMouseInput => TurnManager.Inst.myTurn && !TurnManager.Inst.isLoading;//나의 턴이면서 isLoading이 아니면 마우스입력을 받을 수 있음

    private Entity selectEntity;//공격할 entity를 선택
    private Entity targetPickEntity;//마우스를 끌어다가 선택
    private WaitForSeconds delay1 = new WaitForSeconds(1);
    private WaitForSeconds delay2 = new WaitForSeconds(2);
    
    #region Events

    private void Start()
    {
        TurnManager.OnTurnStarted += OnTurnStarted;
    }

    private void Update()
    {
        ShowTargetPicker(ExistTargetPickEntity);
    }
    

    private void OnDestroy()
    {
        TurnManager.OnTurnStarted -= OnTurnStarted;
    }

    #endregion
    
    
    #region Methods

    private void OnTurnStarted(bool myTurn)
    {
        AttackableReset(myTurn);//턴이 넘어왔을 때 attackable=true로 설정,공격할 수 있는 상태가 됨
        
        if (!myTurn)//내 턴이 아니면
            StartCoroutine(AICo());
    }

    //상대 entity 내기
    IEnumerator AICo()
    {
        CardManager.Inst.TryPutCard(false);
        yield return delay1;//1초 대기
        
        //공격로직
        //attackable이 true인 모든 otehrENtites를 가져와 순서를 섞는다
        var attackers = new List<Entity>(otherEntities.FindAll(x => x.attackable == true));//상대 entity모두 공격을 할수 있게 함
        for (int i = 0; i < attackers.Count; i++)//상대 entity수 만큼
        {
            int rand = Random.Range(i, attackers.Count);//공격할 entity를 랜덤으로 할당
            Entity temp = attackers[i];// 공격할 원래 entity를 임시보관
            attackers[i] = attackers[rand];//현재 entity에 랜덤 entity할당
            attackers[rand] = temp;//랜덤 entity에 보관해뒀던 원래 entity할당

        }
        
        //보스를 보함한 myEntities를 랜덤하게 시간차 공격
        foreach (var attacker in attackers)
        {
            var defenders = new List<Entity>(myEntities);//내 entity를 리스트로 만듦
            defenders.Add(mybossEntity);//보스까지 넣음
            int rand = Random.Range(0, defenders.Count);//내 entity리스트 수 중 랜덤으로
            Attack(attacker,defenders[rand]);//랜덤으로 만든 상대 entity가 랜덤으로 내 entity공격
            
            if(TurnManager.Inst.isLoading)//isLoading이면 코루틴 종료
                yield break;

            yield return new WaitForSeconds(2);//2초대기 후
            
        }
        
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

    //공격할 entity선택
    public void EntityMouseDown(Entity entity)
    {
        if(!CanMouseInput)
            return;

        selectEntity = entity;//클릭한  entity할당
    }

    //공격할 entity를 null로 설정
    public void EntityMouseup()
    {
        if(!CanMouseInput)
            return;
        
        //selectEntity, targetPickEntity 둘 다 존재하면 공격하고 바로 null로 설정함
        if(selectEntity && targetPickEntity && selectEntity.attackable)
            Attack(selectEntity,targetPickEntity);

        selectEntity = null;//공격할 entity null
        targetPickEntity = null;//공격 null
    }

    
    //내 entity와 충돌한 상대entity 찾기
    public void EntityMouseDrag()
    {
        if(!CanMouseInput || selectEntity ==null)
            return;
        
        //other 카겟엔티티 찾기
        bool existTarget = false;
        foreach (var hit in Physics2D.RaycastAll(Utils.MousePos,Vector3.forward))
        {
            Entity entity = hit.collider?.GetComponent<Entity>();//충돌한 entity스크립트 참조
            if (entity != null && !entity.isMine && selectEntity)//entity가 null이 아니고 상태entity,selectEntity일 때
            {
                targetPickEntity = entity;//충돌한 entity를 할당
                existTarget = true;
                break;
            }
        }

        if (!existTarget)//타겟 entity가 존재하지 않으면
            targetPickEntity = null;
    }

    //공격
    private void Attack(Entity attacker, Entity defender)
    {
        //attacker가 defender의 위치로 이동하다 원래 위치로 오다, 이때 order가 높다
        attacker.attackable = false;//공격을 했으니 공격을 못하게 false
        attacker.GetComponent<Order>().SetMostFrontOrder(true);

        Sequence sequence = DOTween.Sequence()
            .Append(attacker.transform.DOMove(defender.originPos, 0.4f)).SetEase(Ease.InSine)//공격할 entity로 이동
            .AppendCallback(() =>
            {
                //데미지 주고받기   
                attacker.Damaged(defender.attack);//막은 상대의 공격력 전달, 공격한 entity 데미지 계산
                defender.Damaged(attacker.attack);//공격한 상대의 공격력 전달, 막은 entity 데미지 계산
                
                SpawnDamage(defender.attack, attacker.transform);//공격위치에서 막은 공격력 값
                SpawnDamage(attacker.attack, defender.transform);//막은 위치에서 공격한 공격력 값
            })
            .Append(attacker.transform.DOMove(attacker.originPos, 0.4f)).SetEase(Ease.OutSine)//원리위치로 이동
            .OnComplete(() => AttackCallback(attacker,defender));//죽음처리
    }

    private void AttackCallback(params Entity[] entities)
    {
        //죽을 사람 골라서 죽음 처리
        entities[0].GetComponent<Order>().SetMostFrontOrder(false);//attacke를 원래order로 설정 

        foreach (var entity in entities)
        {
            if(!entity.isDie||entity.isBossOrEmpty)//entity가 죽지않았거나 
                continue;//죽었으면 아래 부분 실행

            if (entity.isMine)//내 entity이면
                myEntities.Remove(entity);//리스트에서 해당 entity를 지움
            else//상대 entity면
            {
                otherEntities.Remove(entity);//상대 리스트에서 해당 entity지움
            }

            Sequence sequence = DOTween.Sequence()
                .Append(entity.transform.DOShakePosition(1.3f)) //1.3초동안 흔든다
                .Append(entity.transform.DOScale(Vector3.zero, 0.3f)).SetEase(Ease.OutCirc) //크기를 0.3초동안 0으로
                .OnComplete(() =>
                {
                    EntityAlignment(entity.isMine); //리스트에서 제거했으니, 나 or 상대 entity정렬
                    Destroy(entity.gameObject); //entity객체 지움
                });
        }

        StartCoroutine(CheckBossDie());//보스 죽음 체크

    }

    //보스 죽음 체크
    IEnumerator CheckBossDie()
    {
        yield return delay2;

        if (mybossEntity.isDie)//내 보스가 죽으면
            StartCoroutine(GameManager.Inst.GameOver(false));//게임오버

        if (otherBossEntity.isDie)//상대 보스가 죽으면
            StartCoroutine(GameManager.Inst.GameOver(true));//게임 클리어
    }
    
    //보스 데미지 계산
    public void DamageBoss(bool isMine, int damage)
    {
        var targetBossEntity = isMine ? mybossEntity : otherBossEntity;
        targetBossEntity.Damaged(damage);//해당 보스 데미지 계산
        StartCoroutine(CheckBossDie());//죽음 체크
    }
    
    //TargetPicker위치 설정
    private void ShowTargetPicker(bool isShow)
    {
        TargetPicker.SetActive(isShow);
        if (ExistTargetPickEntity)
            TargetPicker.transform.position = targetPickEntity.transform.position;//TargetPicker위치를 공격할 entity위치로 설정

    }

    //데미지 생성
    private void SpawnDamage(int damage, Transform tr)
    {
        if(damage <= 0)
            return;
        
        var damageComponent = Instantiate(damagePrefab).GetComponent<Damage>();//데미지 프리팹 생성, Damage스크립트 가져옴
        damageComponent.SetupTransform(tr);//데미지 생성할 위치 전달
        damageComponent.Damaged(damage);//데미지 값 전달
    }

    public void AttackableReset(bool isMine)
    {
        var targetEntites = isMine ? myEntities : otherEntities; 
        targetEntites.ForEach(x=>x.attackable = true);// 해당 리스트에서 존재하는 모든 entity attackable을 true로 설정
    }

    #endregion
}
