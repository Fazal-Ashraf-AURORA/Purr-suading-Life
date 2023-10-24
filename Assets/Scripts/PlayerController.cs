using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
    [Header("Camera Tracking")]
    [SerializeField] private GameObject _cameraFollowGO;

    private float horizontal;
    private float speed = 8f;
    private float jumpingPower = 16f;
    public bool isFacingRight = true;

    private bool isJumping;
    private bool jumpCooldown;

    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    private bool canDash = true;
    private bool isDashing;
    [SerializeField] private float dashingPower = 24f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;
    private float dashDirection;

    [SerializeField] private bool isWallsliding;
    [SerializeField] private float wallSlidingSpeed = 2f;

    [SerializeField] private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(10f, 20f);


    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform leftGroundCheck;
    [SerializeField] private Transform rightGroundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;


    private CameraFollowObject _cameraFollowObject;

    private void Start() {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

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
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            jumpBufferCounter = 0f;

            StartCoroutine(JumpCooldown());

            if (!isJumping) {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
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

        if (!isWallJumping) {
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
        }

        if (horizontal > 0f || horizontal < 0f) {

            TurnCheck();
        }
    }

    public void Move(InputAction.CallbackContext context) {
        horizontal = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context) {
        if (context.performed) {
            isJumping = true;
            jumpBufferCounter = jumpBufferTime;
        }

        if (context.canceled) {
            isJumping = false;

            if (rb.velocity.y > 0f) {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
                coyoteTimeCounter = 0f;
            }
        }
    }

    public void Dash(InputAction.CallbackContext context) {
        if (context.performed && canDash) {
            StartCoroutine(Dash());
        }
    }

    private void WallSlide() {
        if (IsWalled() && !IsGrounded() && horizontal != 0f) {
            isWallsliding = true;
            //animator.SetBool("isWallSliding", true);
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        } else {
            isWallsliding = false;
            //animator.SetBool("isWallSliding", false);
        }
    }

    private void WallJump() {
        if (isWallsliding && isJumping) {
            wallJumpingDirection = isFacingRight ? -1f : 1f; // Determine the direction of the wall jump
            rb.velocity = new Vector2(wallJumpingPower.x * wallJumpingDirection, wallJumpingPower.y);
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
        //animator.SetBool("isDashing", true);


        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;

        dashDirection = isFacingRight ? 1f : -1f;
        rb.velocity = new Vector2(dashDirection * dashingPower, 0f);
        trailRenderer.emitting = true;

        yield return new WaitForSeconds(dashingTime);
        trailRenderer.emitting = false;

        rb.gravityScale = originalGravity;
        isDashing = false;
        //animator.SetBool("isDashing", false);
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    private bool IsGrounded() {
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
        if (horizontal > 0f && !isFacingRight) {
            Turn();
        } else if (horizontal < 0f && isFacingRight) {
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
}




