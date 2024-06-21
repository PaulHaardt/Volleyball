using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    // Components.
    private VolleyBall _ball;

    // Players.
    [Header("Player Setup")] [SerializeField]
    public List<Player> players = new List<Player>();

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

    // Match variables.
    public enum Team
    {
        Left,
        Right
    }

    public int leftTeamScore;
    public int rightTeamScore;
    private Team _teamControllingBallUnfreeze;
    [Range(0.1f, 1)] public float countdownDuration;
    private bool _isCountdownOngoing;

    // UI.
    [Header("UI Components")] [SerializeField]
    private TMP_Text textLeftTeamScore;

    [SerializeField] private TMP_Text textRightTeamScore;
    [SerializeField] private TMP_Text countdownText;

    private void Awake()
    {
        _ball = FindObjectOfType<VolleyBall>();

        SpawnPlayers(playerCount);

        if (players.Count == 0)
        {
            players = FindObjectsOfType<Player>().ToList();
        }

        // Set Score text.
        textRightTeamScore.text = $"{rightTeamScore}";
        textLeftTeamScore.text = $"{leftTeamScore}";

        // Start match.
        ResetPlayerPositions();
        _ball.SetMovingState(false);
        StartCoroutine(CountdownToStart());

        // Spawns the ball above the player farthest from the net in a random chosen team.
        _teamControllingBallUnfreeze = Random.value > .5 ? Team.Right : Team.Left;
        Player any = GetFarthestPlayer(players, _teamControllingBallUnfreeze);
        _ball.ResetBallPosition(any.transform.position.x, 8);
    }

    private static Player GetFarthestPlayer(List<Player> playersList, Team team)
    {
        return playersList.FindAll(p => p.team == team)
            .OrderByDescending(p => Mathf.Abs(p.transform.position.x)).First();
    }

    private void OnEnable()
    {
        _ball.OnGroundHit += PrepareNextMatch;
        foreach (Player player in players)
        {
            player.OnActionTriggered += () =>
            {
                if (player.team == _teamControllingBallUnfreeze && !_isCountdownOngoing)
                    _ball.SetMovingState(true);
            };
        }
    }

    private void PrepareNextMatch()
    {
        _ball.SetMovingState(false);
        StartCoroutine(CountdownToStart());

        Team winningTeam = UpdateScore();

        ResetPlayerPositions();
        Player any = GetFarthestPlayer(players, winningTeam);
        _ball.ResetBallPosition(any.transform.position.x, 8);
        _ball.ResetVelocity();

        _teamControllingBallUnfreeze = winningTeam;
    }

    private IEnumerator CountdownToStart()
    {
        _isCountdownOngoing = true;

        for (int i = 3; i > 0; i--)
        {
            countdownText.text = $"{i}";
            yield return new WaitForSeconds(countdownDuration);
        }

        countdownText.text = "";
        _isCountdownOngoing = false;
    }

    private Team UpdateScore()
    {
        Team winningTeam = _ball.transform.position.x > 0 ? Team.Right : Team.Left;
        if (winningTeam is Team.Left)
        {
            leftTeamScore += 1;
            textLeftTeamScore.text = $"{leftTeamScore}";
        }
        else
        {
            rightTeamScore += 1;
            textRightTeamScore.text = $"{rightTeamScore}";
        }

        return winningTeam;
    }

    private void ResetPlayerPositions()
    {
        for (int i = 0; i < playerCount; i++)
        {
            players[i].transform.position = GetResetPlayerPosition(players[i].ID, players[i].team);
        }
    }

    private void SpawnPlayers(int count)
    {
        for (var i = 0; i < count; i++)
        {
            Team team = i % 2 == 0 ? Team.Left : Team.Right;

            GameObject newPlayerGameObject =
                Instantiate(playerPrefab, GetResetPlayerPosition(i, team), Quaternion.identity);
            Player newPlayer = newPlayerGameObject.GetComponentInChildren<Player>();

            InitializePlayer(newPlayer, i, team);
            players.Add(newPlayer);
        }
    }

    private Vector2 GetResetPlayerPosition(int id, Team team)
    {
        Vector2 initialPosition = Vector2.zero;
        initialPosition.x = Mathf.Ceil(.5f * (id + 1)) * distanceBetweenPlayers;
        initialPosition.y = 1;

        if (team == Team.Left) initialPosition.x *= -1;

        return initialPosition;
    }

    private void InitializePlayer(Player newPlayer, int id, Team team)
    {
        newPlayer.team = team;
        newPlayer.ID = id;
        newPlayer.SetDefaultColor(playerColors[id]);
        newPlayer.SetInput(id);
    }
}