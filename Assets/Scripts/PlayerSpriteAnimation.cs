using UnityEngine;

public class PlayerSpriteAnimation : MonoBehaviour{

    private PlayerController playerController;
    //to decide the minimal playerMovementSpeed
    public float speedThreshold = 0.1f;
    [SerializeField]private bool isGrounded;

    //the rigidbody2d
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField]private Animator _animator;

    private void Awake() {
        isGrounded = playerController.IsGrounded();
    }


    //cache the rigidbody2d at start (or awake)
    private void Start() {
        _rigidbody2D = playerController.GetComponent<Rigidbody2D>();
        //_animator = playerController.GetComponent<Animator>();
        //isGrounded = playerController.IsGrounded();
    }

    //I think the best place to handle animations is on LateUpdate
    private void LateUpdate() {
        //gets the velocity as a vector
        var velocity = _rigidbody2D.velocity;

        // --------------- IDLE --------------- //
        //if the current playerMovementSpeed is lower than the treshold you can set animator is idle
        if (velocity.magnitude <= speedThreshold) {
            _animator.SetTrigger("idle");
            return;
        }

        // --------------- ON GROUND --------------- //
        //if we are moving on the ground 
        if (isGrounded)
            _animator.SetTrigger("run");

        // --------------- JUMPING --------------- // 
        else if (!isGrounded &&
                 velocity.y > 0)
            _animator.SetTrigger("jump");

        // --------------- FALLING --------------- //
        else if (!isGrounded &&
                 velocity.y < 0)
            _animator.SetTrigger("fall");
    }

}
