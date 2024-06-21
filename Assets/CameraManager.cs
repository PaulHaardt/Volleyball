using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Components.
    private List<Player> _players;
    private GameManager _gameManager;
    private Camera _camera;

    // Shake settings.
    [Header("Shake settings")] [Range(0, 20)]
    public float shakeForce;

    [Range(0, 20)] public float shakeSpeed;
    [Range(0, 1)] public float shakeDuration;
    private Vector3 _cameraShakeOffset;
    private Coroutine _shakeCameraCoroutine;
    private Vector3 _cameraPosition;

    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
        _camera = GetComponent<Camera>();
        _players = _gameManager.players;
        _cameraPosition = _camera.transform.position;
    }

    private void Start()
    {
        foreach (Player player in _players)
        {
            player.OnGroundPoundLand += () =>
            {
                if (_shakeCameraCoroutine != null)
                    StopCoroutine(_shakeCameraCoroutine);
                _shakeCameraCoroutine = StartCoroutine(ShakeCamera());
            };
        }
    }

    private IEnumerator ShakeCamera()
    {
        float endTime = Time.time + shakeDuration;

        while (Time.time < endTime)
        {
            _cameraShakeOffset.Set(
                (Mathf.PerlinNoise(0, Time.time * shakeSpeed) - .5f) * shakeForce,
                (Mathf.PerlinNoise(Time.time * shakeSpeed, 0) - .5f) * shakeForce, 0);

            _camera.transform.position = _cameraPosition + _cameraShakeOffset;
            yield return null;
        }

        _cameraShakeOffset = Vector3.zero;
        _camera.transform.position = _cameraPosition + _cameraShakeOffset;
    }

    private bool IsObjectInCameraView(Vector2 objectPosition)
    {
        return _camera.pixelRect.Contains(objectPosition);
    }
}