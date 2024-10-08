using System;
using UnityEngine;


//플레이어 이동 상태
public enum EPlayerMoveState{
    ForceStop = -1,  //ForceStop은 이동을 하면 안되는 상태로 정의
    Idle = 0,
    Run = 1,
    Jump= 2,
}

//플레이어 공중/지상 상태 여부
public enum EPlayerLandState{
    Land,
    Koyote,//공중에 떠도 잠깐 Land처럼 인식하는 상태
    Air
}

public enum EPlayerBehaviourState{
    Normal, //아무것도 안한 일반 상태(공격이 가능 함)
    Attack, //우클릭 누를 시
    AttackWait, //공격 후 대기 상태(공격이 종료되긴 했지만, 쿨타임 등으로 인해 공격을 할 수 없음)
    Dead,   //죽었을 때 (아무것도 못하게 막아야 함)
}

[Serializable]
public class PlayerStateMachine{
    public EPlayerMoveState _playerMoveState;
    public EPlayerLandState _playerLandState;
    public EPlayerBehaviourState _playerBehaviourState;
    public EPlayerElementalType _playerElementalType;

}
//플레이어와 관련된 모든 데이터를 보유하고 있는 클래스 (보유 관리. getter, setter 제공)

public interface IGetPlayerData{
    public ref PhysicsStats GetPlayerPhysicsStats();
    public ref InputState GetPlayerInputState();
    public ref PlayerComponent GetPlayerComponent();
    public ref AttackData GetAttackData();
}

public interface IGetPlayerStateData{
    public ref PlayerStateMachine GetPlayerStateMachine();
}


public class PlayerData : MonoBehaviour, IGetPlayerData, IGetPlayerStateData
{
    [Header("Player Data")]
    [SerializeField] private PhysicsStats _playerStats;
    [SerializeField] private PlayerComponent _playerComponent;
    [SerializeField] private InputState _playerInputState;
    [SerializeField] private AttackData _attackData;
    [SerializeField] private PlayerStateMachine _stateMachine;

    void Awake(){
        SettingInitialize();
        ComponentInitialize();
    }

    private void SettingInitialize()
    {
        _playerInputState = new InputState(){
            GravityDirection = Vector2.down,
        };
        _playerInputState.GravityDirection = Vector2.down;
    }

    private void ComponentInitialize()
    {
        _playerComponent.CapsuleCollider2D =
        _playerComponent.CapsuleCollider2D == null ? GetComponent<CapsuleCollider2D>() : _playerComponent.CapsuleCollider2D;

        _playerComponent.Rigidbody2D =
        _playerComponent.Rigidbody2D == null ? GetComponent<Rigidbody2D>() : _playerComponent.Rigidbody2D;

        _playerComponent.SpriteRenderer =
        _playerComponent.SpriteRenderer == null ? GetComponentInChildren<SpriteRenderer>(true) : _playerComponent.SpriteRenderer;

        _playerComponent.Animator = 
        _playerComponent.Animator == null ? GetComponentInChildren<Animator>(true) : _playerComponent.Animator;
    }

    public ref PhysicsStats GetPlayerPhysicsStats() => ref _playerStats;

    public ref InputState GetPlayerInputState() => ref _playerInputState;

    public ref PlayerComponent GetPlayerComponent() => ref _playerComponent;

    public ref PlayerStateMachine GetPlayerStateMachine() => ref _stateMachine;

    public ref AttackData GetAttackData() => ref _attackData;

}
