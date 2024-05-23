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
    private VolleyBall _ball;
    private SpriteRenderer _spriteRenderer;

    // Speed values.
    [Range(0, 20)] public float speed = 5;
    [Range(0, 1)] public float slowdownX;

    // Game.
    public int? ID = null;

    // Input.
    private PlayerInput _playerInput;
    private InputAction _moveAction;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        _ball = FindObjectOfType<VolleyBall>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        _playerInput = GetComponent<PlayerInput>();
        _moveAction = _playerInput.actions.FindAction("Move");
    }

    private void Start()
    {
        _moveAction.started += ctx =>
        {
            Move(speed * ctx.ReadValue<Vector2>().x);
            Debug.Log("moved");
        };
    }

    private void Update()
    {
        if (!_moveAction.WasPerformedThisFrame())
        {
            Vector2 velocity = _rigidbody2D.velocity;
            velocity.x *= Time.deltaTime * slowdownX;
            _rigidbody2D.velocity = velocity;

            Debug.Log("No movement detected");
        }
    }


    public void Move(InputAction.CallbackContext ctx)
    {
        Move(Mathf.Sign(ctx.ReadValue<Vector2>().y));
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
        // _playerInput.SwitchCurrentActionMap($"Player{id + 1}");
    }
}