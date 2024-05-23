using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    // Components.
    private Camera _camera;
    private VolleyBall _ball;


    // Players.
    private List<Transform> _objectsToFollow = new List<Transform>();

    [Header("Player Setup")] [SerializeField]
    private List<Player> players = new List<Player>();

    [SerializeField] private GameObject playerPrefab;

    [Header("Player Spawn Settings")] [Range(1, 10)]
    public float distanceBetweenPlayers;

    [SerializeField] private Color[] playerColors;

    // Camera follow values.
    [Header("Camera Settings")] [Range(0, 10)]
    public float cameraFollowSpeed;

    [Range(0, 10)] public float cameraRectAdjustSpeed;
    [Range(0, 1)] public float groundVisibilityPercent;

    private void Start()
    {
        _camera = FindObjectOfType<Camera>();
        _ball = FindObjectOfType<VolleyBall>();

        SpawnPlayers(1);

        if (players.Count == 0)
        {
            players = FindObjectsOfType<Player>().ToList();
        }

        _objectsToFollow.AddRange(players.ToList().ConvertAll(p => p.transform));
        _objectsToFollow.Add(_ball.transform);
    }

    // Update is called once per frame
    private void Update()
    {
        Vector3 sourceCameraPosition = _camera.transform.position;
        List<Vector3> objectsPosition = _objectsToFollow.ConvertAll(t => t.position);

        Vector3 targetCameraPosition = objectsPosition.Aggregate((c, t) => c + t) / objectsPosition.Count;
        Vector3 nextCameraPosition =
            Vector3.Lerp(sourceCameraPosition, targetCameraPosition, Time.deltaTime * cameraFollowSpeed);

        float targetMinX = objectsPosition.Min(a => a.x);
        float targetMaxX = objectsPosition.Max(a => a.x);
        // float targetMinY = objectsPosition.Min(a => a.y);
        // float targetMaxY = objectsPosition.Max(a => a.y);

        // OrthographicSize
        float cameraOrthographicSize = _camera.orthographicSize;
        float targetSize = Math.Max(5, targetMaxX - targetMinX);
        float finalOrthographicSize =
            Mathf.Lerp(cameraOrthographicSize, targetSize, Time.deltaTime * cameraRectAdjustSpeed);

        _camera.orthographicSize = finalOrthographicSize;

        nextCameraPosition.y = Mathf.Max(groundVisibilityPercent * finalOrthographicSize, nextCameraPosition.y);
        nextCameraPosition.z = sourceCameraPosition.z;
        _camera.transform.position = nextCameraPosition;
    }

    private bool IsObjectInCameraView(Vector2 objectPosition)
    {
        return _camera.pixelRect.Contains(objectPosition);
    }

    private void SpawnPlayers(int count)
    {
        Vector2 currentPosition = Vector2.zero;
        Vector2 distance = Vector2.right * distanceBetweenPlayers;

        for (var i = 0; i < count; i++)
        {
            distance.x *= -1;
            currentPosition.x *= -1;
            currentPosition += distance;

            GameObject newPlayerGameObject = Instantiate(playerPrefab, currentPosition, Quaternion.identity);
            Player newPlayer = newPlayerGameObject.GetComponentInChildren<Player>();

            InitializePlayer(newPlayer, i);

            players.Add(newPlayer);
        }
    }

    private void InitializePlayer(Player newPlayer, int id)
    {
        newPlayer.ID = id;
        newPlayer.SetColor(playerColors[id]);
        newPlayer.SetInput(id);
    }
}