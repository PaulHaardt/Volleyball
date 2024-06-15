using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

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

    [SerializeField, Range(0, 100)] private float cameraRectExtendSpeed;
    [SerializeField, Range(0, 10)] private float cameraRectReduceSpeed;
    [SerializeField, Range(1, 50)] private float targetSizeRatio;
    [SerializeField, Range(0, 1)] private float groundVisibilityPercent;

    [Range(1, 4)] public int playerCount = 1;
    private float _initialCameraRectSize;

    // Match variables.
    public int leftTeamScore;
    public int rightTeamScore;
    private int _unfreezerTeamNumber = 0;
    [Range(1, 2)] public float timeScaleIncreaseFactor = 1.1f;

    // UI.
    [Header("UI Components")] [SerializeField]
    private TMP_Text textLeftTeamScore;
    [SerializeField] private TMP_Text textRightTeamScore;
    private const string PrefixTextLeftTeam = "";
    private const string PrefixTextRightTeam = "";

    private void Awake()
    {
        _camera = FindObjectOfType<Camera>();
        _initialCameraRectSize = _camera.orthographicSize;
        _ball = FindObjectOfType<VolleyBall>();

        SpawnPlayers(playerCount);

        if (players.Count == 0)
        {
            players = FindObjectsOfType<Player>().ToList();
        }

        _objectsToFollow.AddRange(players.ToList().ConvertAll(p => p.transform));
        _objectsToFollow.Add(_ball.transform);

        // Set Score text.
        textLeftTeamScore.text = PrefixTextLeftTeam + leftTeamScore;
        textRightTeamScore.text = PrefixTextRightTeam + rightTeamScore;

        // Start match.
        ResetPlayerPositions();
        _ball.SetMovingState(false);
        bool doesRightStart =Random.value > .5;
        _unfreezerTeamNumber = doesRightStart ? 1 : 0;
        _ball.ResetBallPosition((doesRightStart ? 8 : -8) * Vector2.right, 8);
    }

    // Update is called once per frame

    private bool IsObjectInCameraView(Vector2 objectPosition)
    {
        return _camera.pixelRect.Contains(objectPosition);
    }

    private void OnEnable()
    {
        _ball.OnGroundHit += PrepareNextMatch;
        foreach (var player in players)
        {
            player.OnActionTriggered += () =>
            {
                if (player.teamNumber == _unfreezerTeamNumber) _ball.SetMovingState(true);
            };
        }
    }

    private void PrepareNextMatch()
    {
        bool didLeftScore = UpdateScore();
        _ball.ResetBallPosition((didLeftScore ? 8 : -8) * Vector2.right, 8);
        _ball.ResetVelocity();
        _unfreezerTeamNumber = didLeftScore ? 1 : 0;
        ResetPlayerPositions();
        Time.timeScale = 1;
        _ball.SetMovingState(false);
    }

    private IEnumerator WaitToStart()
    {
        yield return new WaitForSeconds(1);
    }

    private bool UpdateScore()
    {
        bool didLeftScore = _ball.transform.position.x > 0;
        if (didLeftScore)
        {
            leftTeamScore += 1;
            textLeftTeamScore.text = PrefixTextLeftTeam + leftTeamScore;
        }
        else
        {
            rightTeamScore += 1;
            textRightTeamScore.text = PrefixTextRightTeam + rightTeamScore;
        }

        return didLeftScore;
    }

    private void ResetPlayerPositions()
    {
        for (int i = 0; i < playerCount; i++)
        {
            players[i].transform.position = GetResetPlayerPosition(i);
        }
    }

    private void Update()
    {
        Vector3 sourceCameraPosition = _camera.transform.position;
        List<Vector3> objectsPosition = _objectsToFollow.ConvertAll(t => t.position);

        Vector3 nextCameraPosition = objectsPosition.Aggregate((c, t) => c + t) / objectsPosition.Count;
        nextCameraPosition = Vector3.Lerp(sourceCameraPosition, nextCameraPosition, Time.deltaTime * cameraFollowSpeed);

        float targetMinX = objectsPosition.Min(a => a.x);
        float targetMaxX = objectsPosition.Max(a => a.x);
        // float targetMinY = objectsPosition.Min(a => a.y);
        // float targetMaxY = objectsPosition.Max(a => a.y);

        // OrthographicSize
        float cameraOrthographicSize = _camera.orthographicSize;
        float targetSize = Math.Max(_initialCameraRectSize, (targetMaxX - targetMinX) / targetSizeRatio);
        float rectSpeed = Mathf.Sign(targetSize - cameraOrthographicSize) > 0
            ? cameraRectExtendSpeed
            : cameraRectReduceSpeed;
        float finalOrthographicSize =
            Mathf.Lerp(cameraOrthographicSize, targetSize, Time.deltaTime * rectSpeed);

        nextCameraPosition.x = sourceCameraPosition.x;
        nextCameraPosition.y = Mathf.Max(groundVisibilityPercent * finalOrthographicSize, nextCameraPosition.y);
        nextCameraPosition.z = sourceCameraPosition.z;

        _camera.orthographicSize = finalOrthographicSize;
        _camera.transform.position = nextCameraPosition;
    }

    private void SpawnPlayers(int count)
    {
        for (var i = 0; i < count; i++)
        {
            GameObject newPlayerGameObject =
                Instantiate(playerPrefab, GetResetPlayerPosition(i), Quaternion.identity);
            Player newPlayer = newPlayerGameObject.GetComponentInChildren<Player>();

            int teamNumber = i % 2;

            InitializePlayer(newPlayer, i, teamNumber);

            players.Add(newPlayer);
        }
    }

    private Vector2 GetResetPlayerPosition(int playerNumber)
    {
        Vector2 initialPosition = Vector2.zero;
        initialPosition.x = Mathf.Ceil(.5f * (playerNumber + 1)) * distanceBetweenPlayers;
        initialPosition.y = 1;
        
        if (playerNumber % 2 == 0) initialPosition.x *= -1;

        return initialPosition;
    }

    private void InitializePlayer(Player newPlayer, int id, int teamNumber)
    {
        newPlayer.ID = id;
        newPlayer.teamNumber = teamNumber;
        newPlayer.SetDefaultColor(playerColors[id]);
        newPlayer.SetInput(id);
    }
}