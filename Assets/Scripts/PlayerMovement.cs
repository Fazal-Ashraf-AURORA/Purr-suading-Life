using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;


public class PlayerMovement : MonoBehaviour {

    [Header("Camera Tracking")]
    [SerializeField] private GameObject _cameraFollowGO;

    public Animator animator;
    private float horizontal;
    [SerializeField]private float speed = 8f;
    [SerializeField] private float jumpingPower = 16f;
    public bool isFacingRight = true;

    private bool canDash = true;
    private bool isDashing;
    [SerializeField] private float dashingPower = 24f;
    private float dashingTime = 0.4f;
    private float dashingCooldown = .7f;
    private float dashDirection;

    [SerializeField] private bool isWallsliding;
    private float wallSlidingSpeed = 1.3f;

    [SerializeField] private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(8f, 16f);

    private CameraFollowObject _cameraFollowObject;
    private float _fallSpeedYDampingChangeThreshold;

    private bool isFalling = false; // Track if the player is falling


    private bool isJumping;

    [SerializeField] private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    [SerializeField] private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;


    private void Start() {
        Cursor.visible = false;
        _cameraFollowObject = _cameraFollowGO.GetComponent<CameraFollowObject>();

        _fallSpeedYDampingChangeThreshold = CameraManager.instance._fallSpeedYDampingChangeThreshold;
    }


    private void Update() {
        if (isDashing) {
            return;
        }
        horizontal = Input.GetAxisRaw("Horizontal");


        animator.SetFloat("Speed", Mathf.Abs(horizontal));

        if (IsGrounded()) {
            animator.SetBool("isJumping", false);
            coyoteTimeCounter = coyoteTime;
        } else {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump")) {
            jumpBufferCounter = jumpBufferTime;
        } else {
            jumpBufferCounter -= Time.deltaTime;
        }

        if(Input.GetKeyDown(KeyCode.LeftShift) && canDash) {
            StartCoroutine(Dash());
        }

        if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f && !isJumping) {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            animator.SetBool("isJumping", true);

            jumpBufferCounter = 0f;

            StartCoroutine(JumpCooldown());
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f) {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            animator.SetBool("isJumping", true);
            coyoteTimeCounter = 0f;
        }

        WallSlide();
        WallJump();

        if(!isWallJumping) {
       TurnCheck();

        }

        // Check if the player is falling and notify CameraManager to change YDamping
        if (rb.velocity.y < _fallSpeedYDampingChangeThreshold && !isFalling) {
            isFalling = true;
            StartFalling(); // Call this when the player starts falling
        } else if (rb.velocity.y >= 0f && isFalling) {
            isFalling = false;
            StartStandingOrMoving(); // Call this when the player is standing or moving
        }
    }

    private void FixedUpdate() {
        if (isDashing) {
            return;
        }
        
        if(horizontal > 0f || horizontal < 0f) {

        TurnCheck() ;
        }


        if (!isWallJumping) {
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);         
        }
    }

    private bool IsGrounded() {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private bool IsWalled() {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    private void WallSlide() {
        if(IsWalled() && !IsGrounded() /*&& horizontal != 0f*/) {
            isWallsliding = true;
            animator.SetBool("isWallSliding", true);
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        } else {
            isWallsliding = false;
            animator.SetBool("isWallSliding", false);
        }
    }

    // When the player starts falling, call this function to set Y damping to 0.25f
    void StartFalling() {
        CameraManager.instance.ChangeYDamping(0.25f);
    }

    // When the player is standing or moving horizontally, call this function to set Y damping to 2.0f
    void StartStandingOrMoving() {
        CameraManager.instance.ChangeYDamping(2.0f);
    }

    private void WallJump() {
        if (isWallsliding && Input.GetButtonDown("Jump")) {
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


    private IEnumerator JumpCooldown() {
        isJumping = true;
        yield return new WaitForSeconds(0.4f);
        isJumping = false;
    }

    private IEnumerator Dash() {

        canDash = false;
        isDashing = true;
        animator.SetBool("isDashing", true);
        

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        
        dashDirection = isFacingRight ? 1f : -1f;
        rb.velocity = new Vector2(dashDirection * dashingPower, 0f);
        trailRenderer.emitting = true;

        yield return new WaitForSeconds(dashingTime);
        trailRenderer.emitting = false;
        
        rb.gravityScale = originalGravity;
        isDashing = false;
        animator.SetBool("isDashing", false);
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    private void TurnCheck() {
        if(horizontal > 0f && !isFacingRight) {
            Turn();
        }else if(horizontal < 0f && isFacingRight) {
            Turn();
        }
    }

    private void Turn() {
        if(isFacingRight) {

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