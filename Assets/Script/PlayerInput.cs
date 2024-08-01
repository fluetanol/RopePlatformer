using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerInput : MonoBehaviour, InputInterface{

    public float MaxSpeed;
    public float NormalAccelTime;
    public float StopAccelTime;
    public float JumpHeight;
    public float Gravity;
    public float Resistence;

    private NormalMoveInterface _normalMoveInterface;
    private FallingInterface _fallingInterface;
    
    private PlayerControls _playerControls;
    private Rigidbody2D _playerRigidbody;
    
    private Vector2 _normalMove;
    private Vector2 _gravityMove;
    private Vector2 _finalMove;

    private Vector2 _jumpMove;

    void Awake(){
        PlayerInputInitialize();
        ComponentInitialize(); 
        InterfaceInitialize();
    }
    void OnEnable() => _playerControls.Enable();
    void OnDisable() => _playerControls.Disable();


    void FixedUpdate(){
         _normalMove = _normalMoveInterface.NormalMovingVector();
        _gravityMove = _fallingInterface.FallingVector();

        Vector2 currentPosition = _playerRigidbody.position;
        Vector2 expectPosition = _playerRigidbody.position + _normalMove + _gravityMove;

        Collision(currentPosition, expectPosition, 3);
        _playerRigidbody.MovePosition(_finalMove);
    }


    private void Collision(Vector2 currentPosition, Vector2 expectPosition, int depth){
        RaycastHit2D[] hits = new RaycastHit2D[1];
        for (int i=0; i<depth; i++){
            bool isCast = CollisionCast(currentPosition, expectPosition, ref hits);
            if (isCast){
                if(!CollsionHitCheck(hits, ref currentPosition, ref expectPosition)){
                    break;
                }
            }
            else if(!isCast && i == 0){
                _fallingInterface.SetGravityDirection(Vector2.down);
                break;
            }
            else break;
        }
        _finalMove = expectPosition;
    }


    private bool CollisionCast(Vector2 currentPosition, Vector2 expectPosition, ref RaycastHit2D[] hits, float skinWidth = 0.015f){
        CapsuleCollider2D collider = GetComponent<CapsuleCollider2D>();
        Vector2 CastDirection = expectPosition - currentPosition;
        float CastDistance = CastDirection.magnitude + skinWidth;

        int k = collider.Cast(CastDirection, hits, CastDistance, true);
        return k > 0 ? true : false;
    }


    private bool CollsionHitCheck(RaycastHit2D[] hits,ref Vector2 currentPosition, ref Vector2 expectPosition){
        RaycastHit2D hit = hits[0];
        Vector2 hitNormal = hit.normal;
        Vector2 velocity = expectPosition - currentPosition;
        Vector2 snapSurfaceVector = velocity.normalized * (hit.distance - 0.015f);
        
        // snap surface로 충돌면 까지 가는 벡터를 구하고,
        // 잔여 속도 벡터의 양만큼 다시 collision casting을 하는 것.
        Vector2 leftover = velocity - snapSurfaceVector;

       // if (snapSurfaceVector.magnitude <= 0.015f) snapSurfaceVector = Vector2.zero;

        float angle = Vector2.Angle(Vector2.up, hitNormal);
        
        if (angle <= 45){
            Vector2 project = ProjectAndScale(leftover, hitNormal);
            
            currentPosition = currentPosition + snapSurfaceVector;
            expectPosition = currentPosition + project;

            SlopeDirection(hitNormal,project, ref expectPosition, ref hit);
            _fallingInterface.DisableFalling();
        }

   
        else{
         /*
            float scale = 1 - Vector2.Dot(
                new Vector2(hitNormal.x,0).normalized,
                -new Vector2(0.1f, 0).normalized
            ); 

            leftover.y = 0;
            hitNormal.y = 0;
            Vector2 project = ProjectAndScale(leftover, hitNormal).normalized
            * scale;*/

            Vector2 project = ProjectAndScale(leftover, hitNormal);
            print(project +" " +snapSurfaceVector);

            //expectPosition +=_gravityMove;
            //expectPosition.x = expectPosition.x + hit.normal.x * hit.distance;
           // print(expectPosition);

            _fallingInterface.DisableFalling();
            return false;
        }

        return true;
    }
    
    private Vector2 ProjectAndScale(Vector2 vec, Vector2 normal){
        float mag = vec.magnitude;
        Vector2 project = Vector3.ProjectOnPlane(vec, normal).normalized * mag;
        return project;
    }



    private void SlopeDirection(Vector2 normal, Vector2 project, ref Vector2 expectPosition, ref RaycastHit2D hit){
        CapsuleCollider2D collider = GetComponent<CapsuleCollider2D>();
        _fallingInterface.SetGravityDirection(-normal);
       _normalMoveInterface.SetSlopeDirection(normal);
    }

    //initialize GetComponent
    private void ComponentInitialize(){
        _playerRigidbody = GetComponent<Rigidbody2D>();
    }

    private void InterfaceInitialize(){
        _normalMoveInterface = new PlayerAccelNormalMove(MaxSpeed, NormalAccelTime, StopAccelTime);
        _fallingInterface = new FallingGravityMoving(Gravity);
    }
    //initialize Player Input
    private void PlayerInputInitialize(){
        _playerControls = new();
        _playerControls.Enable();
        _playerControls.Locomotion.Move.started += OnMove;
        _playerControls.Locomotion.Move.canceled += OnMove;
        _playerControls.Locomotion.Jump.started += OnJump;

    }

    public void OnMove(InputAction.CallbackContext ctx){
        Vector2 _direction = ctx.ReadValue<Vector2>();
        _normalMoveInterface.SetDirectionVector(_direction);
    }

    public void OnJump(InputAction.CallbackContext ctx){
        _jumpMove = Vector2.up * JumpHeight;
    }

}
