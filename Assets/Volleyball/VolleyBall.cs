using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolleyBall : MonoBehaviour
{
    // Components.
    private Rigidbody2D _rigidbody2D;

    public delegate void GroundHitEvent();

    public GroundHitEvent OnGroundHit;

    public delegate void PlayerHitEvent();

    public PlayerHitEvent OnPlayerHit;
    private GameManager _gameManager;
    private SpriteRenderer _spriteRenderer;

    // Hit variables.
    public int HitCount { get; private set; }
    public float colorVelocityFactor = 10;
    public Color slowSpeedColor;
    public Color fastSpeedColor;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _gameManager = FindObjectOfType<GameManager>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        OnPlayerHit += OnPlayerHitEvent;
    }

    private void OnPlayerHitEvent()
    {
        HitCount++;
    }

    public void ResetBallPosition(float x, float y)
    {
        transform.position = new Vector3(x, y, 0);
    }

    public void SetMovingState(bool shouldMove)
    {
        _rigidbody2D.simulated = shouldMove;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if ((col.gameObject.layer & LayerMask.NameToLayer("Floor")) != 0)
        {
            OnGroundHit?.Invoke();
        }
        else if ((col.gameObject.layer & LayerMask.NameToLayer("Player")) != 0)
        {
            OnPlayerHit?.Invoke();
        }
    }

    private void Update()
    {
        _spriteRenderer.color = Color.Lerp(slowSpeedColor, fastSpeedColor,
            Mathf.Sqrt(_rigidbody2D.velocity.magnitude) / colorVelocityFactor
        );

        Debug.DrawRay(transform.position + Vector3.back, _rigidbody2D.velocity, Color.green);
    }

    private void FixedUpdate()
    {
        if (_rigidbody2D.velocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(_rigidbody2D.velocity.y, _rigidbody2D.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
    }

    public void ResetVelocity()
    {
        _rigidbody2D.velocity = Vector2.zero;
        ;
        _rigidbody2D.angularVelocity = 0;
    }
}