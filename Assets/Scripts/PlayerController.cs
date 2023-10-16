//using System.Collections;
//using UnityEngine;
//using UnityEngine.InputSystem;
//using static UnityEditor.Searcher.SearcherWindow.Alignment;

//public class PlayerController : MonoBehaviour {
//    private float horizontalVelocityInput;
//    private float speed = 8f;
//    private float jumpingPower = 16f;
//    private bool isFacingRight = true;

//    private bool isJumping;
//    private bool jumpCooldown;

//    private float coyoteTime = 0.2f;
//    private float coyoteTimeCounter;

//    private float jumpBufferTime = 0.2f;
//    private float jumpBufferCounter;

//    private bool canDash = true;
//    private bool isDashing;
//    [SerializeField] private float dashingPower = 24f;
//    private float dashingTime = 0.2f;
//    private float dashingCooldown = 1f;
//    private float dashDirection;
//    [SerializeField] private TrailRenderer trailRenderer;

//    [SerializeField] private Rigidbody2D rb;
//    [SerializeField] private Transform groundCheck;
//    [SerializeField] private LayerMask groundLayer;

//    private void Start() {
//        Cursor.visible = false;
//    }

//    void Update() {
//        if (isDashing) {
//            return;
//        }


//        if (IsGrounded()) {
//            coyoteTimeCounter = coyoteTime;
//        } else {
//            coyoteTimeCounter -= Time.deltaTime;
//        }

//        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !jumpCooldown) {
//            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
//            jumpBufferCounter = 0f;

//            StartCoroutine(JumpCooldown());

//            if (!isJumping) {
//                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
//            }
//        } else {
//            jumpBufferCounter -= Time.deltaTime;
//        }

//        if (isDashing == true && canDash) {
//            Debug.Log("Dash");
//            StartCoroutine(Dash());
//        }



//        TurnCheck();
//    }

//    private void FixedUpdate() {
//        rb.velocity = new Vector2(horizontalVelocityInput * speed, rb.velocity.y);

//        if (horizontalVelocityInput > 0f || horizontalVelocityInput < 0f) {

//            TurnCheck();
//        }
//    }

//    public void Move(InputAction.CallbackContext context) {
//        horizontalVelocityInput = context.ReadValue<Vector2>().x;
//    }

//    public void Jump(InputAction.CallbackContext context) {
//        if (context.performed) {
//            isJumping = true;
//            jumpBufferCounter = jumpBufferTime;
//        }

//        if (context.canceled) {
//            isJumping = false;

//            if (rb.velocity.y > 0f) {
//                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
//                coyoteTimeCounter = 0f;
//            }
//        }
//    }

//    public void Dash(InputAction.CallbackContext context) {
//        if (context.performed) {
//            isDashing = true;
//        }

//        if(context.canceled) {
//            isDashing = false;
//        }
//    }

//    private bool IsGrounded() {
//        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
//    }

//    private void TurnCheck() {
//        if (horizontalVelocityInput > 0f && !isFacingRight) {
//            Turn();
//        } else if (horizontalVelocityInput < 0f && isFacingRight) {
//            Turn();
//        }
//    }

//    private void Turn() {
//        if (isFacingRight) {

//            Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
//            transform.rotation = Quaternion.Euler(rotator);
//            isFacingRight = !isFacingRight;

//            //_cameraFollowObject.CallTurn();
//        } else {
//            Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
//            transform.rotation = Quaternion.Euler(rotator);
//            isFacingRight = !isFacingRight;

//            //_cameraFollowObject.CallTurn();
//        }
//    }

//    private IEnumerator JumpCooldown() {
//        jumpCooldown = true;
//        yield return new WaitForSeconds(0.4f);
//        jumpCooldown = false;
//    }

//    private IEnumerator Dash() {

//        canDash = false;
//        isDashing = true;

//        float originalGravity = rb.gravityScale;
//        rb.gravityScale = 0;

//        dashDirection = isFacingRight ? 1f : -1f;
//        rb.velocity = new Vector2(dashDirection * dashingPower, 0f);
//        trailRenderer.emitting = true;

//        yield return new WaitForSeconds(dashingTime);
//        trailRenderer.emitting = false;

//        rb.gravityScale = originalGravity;
//        isDashing = false;
//        yield return new WaitForSeconds(dashingCooldown);
//        canDash = true;
//    }
//}



//===============================================================================================================
//===============================================================================================================
//===============================================================================================================


using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpingPower = 16f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    [SerializeField] private float dashingPower = 24f;
    [SerializeField] private float dashingTime = 0.2f;
    [SerializeField] private float dashingCooldown = 1f;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private InputAction movementInput;
    [SerializeField] private InputAction jumpInput;
    [SerializeField] private InputAction dashInput;

    private float horizontalVelocityInput;
    private bool isFacingRight = true;
    private bool isJumping;
    private bool jumpCooldown;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool canDash = true;
    private bool isDashing;
    private float dashDirection;

    private void Start() {
        Cursor.visible = false;
    }

    private void OnEnable() {
        movementInput.Enable();
        jumpInput.Enable();
        dashInput.Enable();
    }

    private void OnDisable() {
        movementInput.Disable();
        jumpInput.Disable();
        dashInput.Disable();
    }

    private void Update() {
        if (isDashing) {
            return;
        }

        horizontalVelocityInput = movementInput.ReadValue<Vector2>().x;

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

        if (dashInput.triggered && canDash) {
            Debug.Log("Dash");
            StartCoroutine(Dash());
        }

        TurnCheck();
    }

    private void FixedUpdate() {
        rb.velocity = new Vector2(horizontalVelocityInput * speed, rb.velocity.y);

        if (horizontalVelocityInput > 0f || horizontalVelocityInput < 0f) {
            TurnCheck();
        }
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

    private bool IsGrounded() {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void TurnCheck() {
        if (horizontalVelocityInput > 0f && !isFacingRight) {
            Turn();
        } else if (horizontalVelocityInput < 0f && isFacingRight) {
            Turn();
        }
    }

    private void Turn() {
        Vector3 rotator = new Vector3(transform.rotation.x, isFacingRight ? 180f : 0f, transform.rotation.z);
        transform.rotation = Quaternion.Euler(rotator);
        isFacingRight = !isFacingRight;
    }

    private IEnumerator JumpCooldown() {
        jumpCooldown = true;
        yield return new WaitForSeconds(0.4f);
        jumpCooldown = false;
    }

    private IEnumerator Dash() {
        canDash = false;
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;

        dashDirection = isFacingRight ? 1f : -1f;
        rb.velocity = new Vector2(dashDirection * dashingPower, 0f);
        trailRenderer.emitting = true;

        yield return new WaitForSeconds(dashingTime);
        trailRenderer.emitting = false;

        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}

