using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static GameManager;

public class Player : MonoBehaviour
{
    // Components.
    private Rigidbody2D _rigidbody2D;
    private CapsuleCollider2D _capsuleCollider2D;
    private CapsuleCollider2D _rangeCollider2D;
    private VolleyBall _ball;
    private SpriteRenderer _spriteRenderer;
    private AudioSource _audioSource;
    private Animator _animator;

    // Speed values.
    [Header("Speed")] [Range(0, 20)] public float speed = 5;
    [Range(0, 1)] public float slowdownX;

    // Game.
    [Header("Match")] public Team team;
    public int ID { get; set; }

    public bool Grounded { get; private set; }

    // Smash.
    [Header("Smash")] [Range(5, 50)] public float fallVelocity;

    public delegate void ActionTriggeredEvent();

    public ActionTriggeredEvent OnActionTriggered;

    public delegate void GroundPoundLandEvent();

    public GroundPoundLandEvent OnGroundPoundLand;

    // Input.
    [Header("Jump")] private PlayerInput _playerInput;
    private float _movementDirection;
    private int _currentJumpCount;
    [Range(20, 50)] public float jumpForce;
    [SerializeField, Range(1, 3)] private int maxJumpCount;

    // Color.
    [Header("Color")] [SerializeField] private Color targetColor;

    // Animations variables.
    private readonly int _isGroundedHash = Animator.StringToHash("isGrounded");
    private readonly int _isMovingHash = Animator.StringToHash("isMoving");
    private readonly int _velocityXHash = Animator.StringToHash("VelocityX");
    private readonly int _velocityYHash = Animator.StringToHash("VelocityY");

    private void Awake()
    {
        CapsuleCollider2D[] capsuleColliders2D = GetComponents<CapsuleCollider2D>();
        _capsuleCollider2D = capsuleColliders2D.First(c => !c.isTrigger);
        ;
        _rangeCollider2D = capsuleColliders2D.First(c => c.isTrigger);

        _rigidbody2D = GetComponent<Rigidbody2D>();
        _ball = FindObjectOfType<VolleyBall>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponentInChildren<Animator>();

        _playerInput = GetComponent<PlayerInput>();
        _playerInput.actions["Pause"].performed += FindObjectOfType<PauseMenu>().Pause;
        // _moveAction = _playerInput.actions.FindAction("Move");
    }

    private void Update()
    {
        if (_movementDirection != 0)
        {
            Move(_movementDirection * speed);

            _animator.SetBool(_isMovingHash, true);

            // Flip the sprite according to the direction.
            var velocity = _rigidbody2D.velocity;
            _spriteRenderer.flipX = velocity.x < 0;
        }
        else
        {
            Vector2 velocity = _rigidbody2D.velocity;
            velocity.x *= Time.deltaTime * slowdownX;
            _rigidbody2D.velocity = velocity;

            _animator.SetBool(_isMovingHash, false);
        }

        Vector2 rigidbody2DVelocity = _rigidbody2D.velocity;

        _animator.SetFloat(_velocityXHash, rigidbody2DVelocity.x / speed);
        _animator.SetFloat(_velocityYHash, rigidbody2DVelocity.y / speed);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if ((col.gameObject.layer & LayerMask.NameToLayer("Floor")) != 0)
        {
            _currentJumpCount = 0;
            Grounded = true;
            _animator.SetBool(_isGroundedHash, Grounded);

            if (_playerInput.actions.FindAction("Fall").IsInProgress())
            {
                OnGroundPoundLand?.Invoke();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        /*
        float rayDistance = (_capsuleCollider2D.size.y + 0.1f) / 2f;
        // Debug.DrawRay(transform.position, Vector3.down * rayDistance, Color.red, .2f, false);

        ContactFilter2D contactFilter = new ContactFilter2D();

        // Raycast test for ground.
        List<RaycastHit2D> results = new List<RaycastHit2D>();
        var hit = Physics2D.Raycast(_capsuleCollider2D.transform.position, Vector2.down, contactFilter, results,
            rayDistance);
        bool touchesGround = hit > 0 && results.Any(h =>
            (h.transform.gameObject.layer & LayerMask.NameToLayer("Floor")) != 0);
            */

        if ((other.gameObject.layer & LayerMask.NameToLayer("Floor")) != 0)
        {
            Grounded = false;
            _animator.SetBool(_isGroundedHash, Grounded);
        }
    }

    public void SetInputActive(bool setActive)
    {
        Action action = setActive ? _playerInput.ActivateInput : _playerInput.DeactivateInput;
        action.Invoke();
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        OnActionTriggered?.Invoke();

        if (_currentJumpCount >= maxJumpCount) return;

        _currentJumpCount++;

        float jumpHoldMultiplier = Mathf.Clamp((float) ctx.duration, .5f, 1);

        Vector2 rigidbody2DVelocity = _rigidbody2D.velocity;
        rigidbody2DVelocity.y = jumpHoldMultiplier * jumpForce;
        _rigidbody2D.velocity = rigidbody2DVelocity;
    }

    public void ResetBall(InputAction.CallbackContext ctx)
    {
        _ball.ResetBallPosition(transform.position.x, 8);
        _ball.ResetVelocity();
    }

    public void Fall(InputAction.CallbackContext ctx)
    {
        Vector2 rigidbody2DVelocity = _rigidbody2D.velocity;
        rigidbody2DVelocity.y = -fallVelocity;
        _rigidbody2D.velocity = rigidbody2DVelocity;
    }

    public void Move(InputAction.CallbackContext ctx)
    {
        _movementDirection = ctx.ReadValue<float>();
    }

    private void Move(float velocityX)
    {
        OnActionTriggered?.Invoke();

        Vector2 localRightVector2D = _rigidbody2D.transform.worldToLocalMatrix * Vector2.right;
        Vector2 rigidbody2DVelocity = _rigidbody2D.velocity;

        rigidbody2DVelocity.x = localRightVector2D.x * velocityX;
        _rigidbody2D.velocity = rigidbody2DVelocity;
    }

    public void SetDefaultColor(Color playerColor)
    {
        _spriteRenderer.color = playerColor;
    }

    public void SetInput(int id)
    {
        _playerInput.SwitchCurrentActionMap($"Player{id + 1}");
    }
}