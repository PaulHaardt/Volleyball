using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // Components.
    private Rigidbody2D _rigidbody2D;
    private CapsuleCollider2D _capsuleCollider2D;
    private CapsuleCollider2D _rangeCollider2D;
    private VolleyBall _ball;
    private SpriteRenderer _spriteRenderer;
    private AudioSource _audioSource;

    // Speed values.
    [Range(0, 20)] public float speed = 5;
    [Range(0, 1)] public float slowdownX;
    [Range(20, 50)] public float jumpForce;

    // Game.
    public int? ID = null;
    public int teamNumber = 0;

    public delegate void ActionTriggeredEvent();

    public ActionTriggeredEvent OnActionTriggered;

    // Input.
    private PlayerInput _playerInput;
    private float _movementDirection;
    private int _currentJumpCount;
    [SerializeField, Range(1, 3)] private int maxJumpCount;

    // Color.
    [SerializeField] private Color targetColor;
    private Color _defaultColor;
    [Range(5, 50)] public float fallVelocity;

    private void Awake()
    {
        CapsuleCollider2D[] capsuleColliders2D = GetComponents<CapsuleCollider2D>();
        _capsuleCollider2D = capsuleColliders2D[0];
        _rangeCollider2D = capsuleColliders2D[1];

        _rigidbody2D = GetComponent<Rigidbody2D>();
        _ball = FindObjectOfType<VolleyBall>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();

        _playerInput = GetComponent<PlayerInput>();
        // _moveAction = _playerInput.actions.FindAction("Move");
    }

    private void Update()
    {
        if (_movementDirection != 0)
        {
            Move(_movementDirection * speed);
            OnActionTriggered?.Invoke();
        }
        else
        {
            Vector2 velocity = _rigidbody2D.velocity;
            velocity.x *= Time.deltaTime * slowdownX;
            _rigidbody2D.velocity = velocity;

            // Debug.Log("No movement detected");
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if ((col.gameObject.layer & LayerMask.NameToLayer("Volleyball")) != 0)
        {
            _spriteRenderer.color = targetColor;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if ((other.gameObject.layer & LayerMask.NameToLayer("Volleyball")) != 0)
        {
            _spriteRenderer.color = _defaultColor;
        }
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        OnActionTriggered?.Invoke();


        float rayDistance = (_capsuleCollider2D.size.y + 0.1f) / 2f;
        // Debug.DrawRay(transform.position, Vector3.down * rayDistance, Color.red, .2f, false);

        ContactFilter2D contactFilter = new ContactFilter2D();

        // Raycast test for ground.
        List<RaycastHit2D> results = new List<RaycastHit2D>();
        var hit = Physics2D.Raycast(_capsuleCollider2D.transform.position, Vector2.down, contactFilter, results,
            rayDistance);
        bool touchesGround = hit > 0 && results.Any(h =>
            (h.transform.gameObject.layer & LayerMask.NameToLayer("Floor")) != 0);

        if (touchesGround) _currentJumpCount = 0;
        else if (_currentJumpCount >= maxJumpCount) return;
        
        _currentJumpCount++;

        float jumpHoldMultiplier = Mathf.Clamp((float) ctx.duration, .5f, 1);
        
        Vector2 rigidbody2DVelocity = _rigidbody2D.velocity;
        rigidbody2DVelocity.y = jumpHoldMultiplier * jumpForce;
        _rigidbody2D.velocity = rigidbody2DVelocity;
    }

    public void ResetBall(InputAction.CallbackContext ctx)
    {
        _ball.ResetBallPosition(transform.position, 8);
        _ball.ResetVelocity();
    }

    public void Move(InputAction.CallbackContext ctx)
    {
        _movementDirection = ctx.ReadValue<float>();
    }

    public void Fall(InputAction.CallbackContext ctx)
    {
        Debug.Log("Falling");
        Vector2 rigidbody2DVelocity = _rigidbody2D.velocity;
        rigidbody2DVelocity.y = -fallVelocity;
        _rigidbody2D.velocity = rigidbody2DVelocity;
    }

    private void Move(float velocityX)
    {
        Vector2 localRightVector2D = _rigidbody2D.transform.worldToLocalMatrix * Vector2.right;
        Vector2 rigidbody2DVelocity = _rigidbody2D.velocity;

        rigidbody2DVelocity.x = localRightVector2D.x * velocityX;
        _rigidbody2D.velocity = rigidbody2DVelocity;
    }

    private void OnDrawGizmos()
    {
        // Gizmos.DrawFrustum(_camera.transform.position, _camera.fieldOfView, 1000, 0, 1);
        // Rect ballRectangle = new Rect(_ball.transform.position, new Vector2(1, 1));
        // Gizmos.DrawSphere(new Vector3(ballRectangle.center.x, ballRectangle.center.y, 0), 1);
    }

    public void SetDefaultColor(Color playerColor)
    {
        _defaultColor = playerColor;
        _spriteRenderer.color = playerColor;
    }

    public void SetInput(int id)
    {
        _playerInput.SwitchCurrentActionMap($"Player{id + 1}");
    }
}