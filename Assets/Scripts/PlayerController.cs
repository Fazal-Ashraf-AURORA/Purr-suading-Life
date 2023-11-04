using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {

    [Header("Player Movement Properties")]
    [SerializeField]private float playerMovementSpeed = 8f;
    [SerializeField] private float jumpingPower = 16f;
    private float horizontalMovementInput;
    private float verticalMovementInput;


    [Tooltip("increasing will enable player to jump even after not grounded")]
    [SerializeField] private float coyoteTime = 0.2f; //increasing will enable player to jump even after not grounded
    private float coyoteTimeCounter;

    [Tooltip("increasing will add buffer to the jump")]
    [SerializeField] private float jumpBufferTime = 0.2f;//increasing will add buffer to the jump 
    private float jumpBufferCounter;

    [SerializeField] private float dashingPower = 24f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;
    private float dashDirection;

    [SerializeField] private float wallSlidingSpeed = 2f;

    private float wallJumpingDirection;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(10f, 20f);

    //delays for animations
    private float attackDelay = 0.417f;
    private float jumpDelay;


    [Header("Player Animator")]
    //[SerializeField] private float speedThreshold = 0.1f;
    [SerializeField] private Animator playerAnimator;
    private string currentState;

    //Animations States

    const string PLAYER_IDLE = "idle";
    const string PLAYER_DEATH = "death";
    const string PLAYER_RUN = "run";
    const string PLAYER_JUMP = "jump";
    const string PLAYER_FALL = "fall";
    const string PLAYER_ATTACK = "attack";
    const string PLAYER_DASH = "dash";


    [Header("Camera Tracking")]
    [SerializeField] private GameObject _cameraFollowGO;
    private CameraFollowObject _cameraFollowObject;


    //flags
    private bool death = false;
    public bool isFacingRight = true;
    private bool isJumping;
    private bool jumpCooldown;
    private bool canDash = true;
    private bool isDashing;
    private bool isWallsliding;
    private bool isWallJumping;
    private bool isAttacking;
    private bool isAttackPressed;

    [Header("Wall Checks")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    
    [Header("Ground Checks")]
    [SerializeField] private Transform leftGroundCheck;
    [SerializeField] private Transform rightGroundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private Rigidbody2D rigidbody2d;
    [SerializeField] private TrailRenderer trailRenderer;

    

    private void Start() {
        Application.targetFrameRate = 60;// limiting in-game FPS to 60
        Screen.sleepTimeout = SleepTimeout.NeverSleep;//this will make sure that our 

        Cursor.visible = false;
        _cameraFollowObject = _cameraFollowGO.GetComponent<CameraFollowObject>();
    }

    void Update() {

        

        if (IsGrounded()) {
            coyoteTimeCounter = coyoteTime;
        } else {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !jumpCooldown) {
            rigidbody2d.velocity = new Vector2(rigidbody2d.velocity.x, jumpingPower);
            jumpBufferCounter = 0f;
            ChangeAnimationState(PLAYER_JUMP);

            StartCoroutine(JumpCooldown());

            if (!isJumping) {
                rigidbody2d.velocity = new Vector2(rigidbody2d.velocity.x, rigidbody2d.velocity.y * 0.5f);
                ChangeAnimationState(PLAYER_JUMP);
            }
        } else {
            jumpBufferCounter -= Time.deltaTime;
        }

        WallSlide();
        WallJump();

    }

    private bool IsWalled() {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }


    private void FixedUpdate() {
        
        if (isDashing) {
            return;
        }

        if (!isWallJumping && !isAttacking) {
            rigidbody2d.velocity = new Vector2(horizontalMovementInput * playerMovementSpeed, rigidbody2d.velocity.y);
        }
        

        if (horizontalMovementInput > 0f || horizontalMovementInput < 0f) {

            TurnCheck();
        }
    }



    #region Input Actions Events
    
    //Event for Move Input
    public void Move(InputAction.CallbackContext context) {
        horizontalMovementInput = context.ReadValue<Vector2>().x;
        verticalMovementInput = context.ReadValue<Vector2>().y;
    }

    //Event for Jump Input
    public void Jump(InputAction.CallbackContext context) {
        if (context.performed) {
            isJumping = true;
            jumpBufferCounter = jumpBufferTime;
        }

        if (context.canceled) {
            isJumping = false;

            if (rigidbody2d.velocity.y > 0f) {
                rigidbody2d.velocity = new Vector2(rigidbody2d.velocity.x, rigidbody2d.velocity.y * 0.5f);
                coyoteTimeCounter = 0f;
            }
        }
    }

    //Event for Dash Input
    public void Dash(InputAction.CallbackContext context) {
        if (context.performed && canDash) {
            StartCoroutine(Dash());
        }
    }

    //Event for Attack Input
    public void Attack(InputAction.CallbackContext context) {
        if (context.performed) {
            isAttackPressed = true;
            //ChangeAnimationState(PLAYER_ATTACK);
           
        }
    }

    //Event for Interact Event

    public void Interact(InputAction.CallbackContext context) {
        if (context.performed) {
            //Debug.Log("Interact");
        }
    }
    #endregion

    private void WallSlide() {
        if (IsWalled() && !IsGrounded() && horizontalMovementInput != 0f) {
            isWallsliding = true;
            rigidbody2d.velocity = new Vector2(rigidbody2d.velocity.x, Mathf.Clamp(rigidbody2d.velocity.y, -wallSlidingSpeed, float.MaxValue));
        } else {
            isWallsliding = false;
        }
    }

    private void WallJump() {
        if (isWallsliding && isJumping) {
            wallJumpingDirection = isFacingRight ? -1f : 1f; // Determine the direction of the wall jump
            rigidbody2d.velocity = new Vector2(wallJumpingPower.x * wallJumpingDirection, wallJumpingPower.y);
            isWallJumping = true;
            wallJumpingCounter = wallJumpingDuration;

            // Rotate the character to face the direction of the wall jump
            Vector3 newRotation = transform.rotation.eulerAngles;
            newRotation.y = isFacingRight ? 0f : 180f;
            transform.rotation = Quaternion.Euler(newRotation);
        }

        if (isWallJumping) {
            wallJumpingCounter -= Time.deltaTime;

            // Check if the wall jump duration has ended
            if (wallJumpingCounter <= 0f) {
                isWallJumping = false;
            }
        }
    }

    private IEnumerator Dash() {

        canDash = false;
        isDashing = true;
        

        float originalGravity = rigidbody2d.gravityScale;
        rigidbody2d.gravityScale = 0;

        dashDirection = isFacingRight ? 1f : -1f;
        rigidbody2d.velocity = new Vector2(dashDirection * dashingPower, 0f);
        trailRenderer.emitting = true;

        yield return new WaitForSeconds(dashingTime);
        trailRenderer.emitting = false;

        rigidbody2d.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    public bool IsGrounded() {
        Vector2 lGroundCheckPoint = leftGroundCheck.TransformPoint(Vector3.zero);
        Vector2 rGroundCheckPoint = rightGroundCheck.TransformPoint(Vector3.zero);

        return Physics2D.OverlapArea(lGroundCheckPoint, rGroundCheckPoint, groundLayer);
    }

    private IEnumerator JumpCooldown() {
        jumpCooldown = true;
        yield return new WaitForSeconds(0.4f);
        jumpCooldown = false;
    }

    private void TurnCheck() {
        if (horizontalMovementInput > 0f && !isFacingRight) {
            Turn();
        } else if (horizontalMovementInput < 0f && isFacingRight) {
            Turn();
        }
    }

    private void Turn() {
        if (isFacingRight) {

            Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            isFacingRight = !isFacingRight;

            _cameraFollowObject.CallTurn();
        } else {
            Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            isFacingRight = !isFacingRight;

            _cameraFollowObject.CallTurn();
        }
    }


    #region Player Animations

    void ChangeAnimationState(string newState) {

        //stop the same animation from interrupting itself
        if(currentState == newState) return;

        //play the animation
        playerAnimator.Play(newState);

        //reassign the current state
        currentState = newState;

    }

    private void LateUpdate() {
        var velocity = rigidbody2d.velocity;
        var yVelocity = velocity.y;


        //Debug.Log(velocity.x);



        if(isDashing ) {
            ChangeAnimationState(PLAYER_DASH);
        }

        if (IsGrounded()  && !isJumping && !isAttacking) {

            if(velocity.x != 0f) {
                ChangeAnimationState(PLAYER_RUN);
            } else {
                ChangeAnimationState(PLAYER_IDLE);
            }
        }

        if (isAttackPressed /*&& velocity.x == 0 */) {
            velocity.x = 0f; velocity.y = 0f;
            isAttackPressed = false;

            if (!isAttacking) {
                isAttacking = true;

                if(IsGrounded()) {
                ChangeAnimationState(PLAYER_ATTACK);
                }

            
            Invoke("AttackComplete", attackDelay);
            }
        }

        if(yVelocity < 0f && !IsGrounded()) {

            ChangeAnimationState(PLAYER_FALL);
            isJumping = false;
        }

        if (death) {
            ChangeAnimationState(PLAYER_DEATH);

            Invoke("ResetLevel", 1f);
        }
    }

    

    void AttackComplete() {
        isAttacking = false;
    }
    #endregion

    void ResetLevel() {
        Debug.Log("Reset level");
        SceneManager.LoadScene("Gameplay");
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Spikes")) {
            Die();
        }
    }

    private void Die() {
        death = true;
    }
}







