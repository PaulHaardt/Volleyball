using System;
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

    // Game.
    public int? ID = null;

    // Input.
    private PlayerInput _playerInput;
    private float _movementDirection;

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
        Debug.Log($"moveDirection {_movementDirection}");
        if (_movementDirection != 0)
        {
            Move(_movementDirection * speed);
        }
        else
        {
            Vector2 velocity = _rigidbody2D.velocity;
            velocity.x *= Time.deltaTime * slowdownX;
            _rigidbody2D.velocity = velocity;

            // Debug.Log("No movement detected");
        }
    }

    public void Jump(InputAction.CallbackContext ctx)
    {
            _rigidbody2D.AddForce(Vector2.up, ForceMode2D.Impulse);
    }

    public void ResetBall(InputAction.CallbackContext ctx)
    {
        _ball.transform.position = transform.position + Vector3.up * 10;
    }

    public void Move(InputAction.CallbackContext ctx)
    {
        _movementDirection = ctx.ReadValue<float>();
    }

    private void Move(float velocityX)
    {
        Vector2 localRightVector2D = _rigidbody2D.transform.worldToLocalMatrix * Vector2.right;
        _rigidbody2D.velocity = localRightVector2D * velocityX;
    }

    private void OnDrawGizmos()
    {
        // Gizmos.DrawFrustum(_camera.transform.position, _camera.fieldOfView, 1000, 0, 1);
        // Rect ballRectangle = new Rect(_ball.transform.position, new Vector2(1, 1));
        // Gizmos.DrawSphere(new Vector3(ballRectangle.center.x, ballRectangle.center.y, 0), 1);
    }

    public void SetColor(Color playerColor)
    {
        _spriteRenderer.color = playerColor;
    }

    public void SetInput(int id)
    {
        _playerInput.SwitchCurrentActionMap($"Player{id + 1}");
    }
}