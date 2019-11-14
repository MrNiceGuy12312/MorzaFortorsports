using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance { get; private set; } = null;

    public GameObject checkpointContainer;
    public Rigidbody playerCar;
    public Rigidbody[] aiCars;
    public float aiDistanceToCover = 1.0f;
    public float respawnDelay = 5.0f;
    public int requiredLaps = 3;

    public Texture2D startRaceImage;
    public Texture2D num1Image;
    public Texture2D num2Image;
    public Texture2D num3Image;

    private int countdownDelay;
    private float countdownTimerStart;
    static public bool Started { get; private set; } = false;

    private AICarController[] aiScripts;
    private float[] aiDistanceLeftToTravel;
    private float[] aiRespawnTimers;
    private Transform[] aiWaypoints;
    private Checkpoint[] checkpoints;
    private int currentCheckpoint;

    private int[] aiLaps;
    private int playerLaps = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        CountdownTimerSet(5);
    }

    // Start is called before the first frame update
    void Start()
    {
        aiRespawnTimers = new float[aiCars.Length];
        aiDistanceLeftToTravel = new float[aiCars.Length];
        aiScripts = new AICarController[aiCars.Length];
        aiWaypoints = new Transform[aiCars.Length];
        aiLaps = new int[aiCars.Length];

        for (int i = 0; i < aiCars.Length; ++i)
        {
            aiScripts[i] = aiCars[i].GetComponent<AICarController>();
            aiRespawnTimers[i] = respawnDelay;
            aiDistanceLeftToTravel[i] = float.MaxValue;
            aiLaps[i] = 0;
            aiCars[i].isKinematic = true;
        }
        playerCar.isKinematic = true;

        checkpoints = checkpointContainer.GetComponentsInChildren<Checkpoint>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Started) return;

        for (int i = 0; i < aiCars.Length; ++i)
        {
            Transform nextWaypoint = aiScripts[i].GetCurrentWaypoint();
            float distanceCovered = (nextWaypoint.position - aiCars[i].position).magnitude;

            if (aiDistanceLeftToTravel[i] - aiDistanceToCover > distanceCovered ||
                aiWaypoints[i] != nextWaypoint)
            {
                aiDistanceLeftToTravel[i] = distanceCovered;
                aiRespawnTimers[i] = respawnDelay;
                aiWaypoints[i] = nextWaypoint;
            }
            else
            {
                aiRespawnTimers[i] -= Time.deltaTime;
            }

            if (aiRespawnTimers[i] <= 0f)
            {
                aiRespawnTimers[i] = respawnDelay;
                aiDistanceLeftToTravel[i] = float.MaxValue;
                Transform lastWaypoint = aiScripts[i].GetLastWaypoint();
                aiCars[i].velocity = Vector3.zero;
                aiCars[i].angularVelocity = Vector3.zero;
                aiCars[i].position = lastWaypoint.position;
                aiCars[i].rotation = Quaternion.LookRotation(nextWaypoint.position - lastWaypoint.position);
            }
        }

        if (Input.GetKey(KeyCode.R))
        {
            int resetCheckpoint = currentCheckpoint - 1;
            if (resetCheckpoint < 0)
            {
                resetCheckpoint = checkpoints.Length - 1;
            }

            Transform lastCheckpoint = checkpoints[resetCheckpoint].transform;
            playerCar.velocity = Vector3.zero;
            playerCar.angularVelocity = Vector3.zero;
            playerCar.position = lastCheckpoint.position;
            playerCar.rotation = Quaternion.LookRotation(checkpoints[currentCheckpoint].transform.position - lastCheckpoint.position);
        }
    }

    public void LapFinishedByAI(AICarController aiScript)
    {
        for (int i = 0; i < aiCars.Length; ++i)
        {
            if (aiScripts[i] == aiScript)
            {
                aiLaps[i] += 1;
                Debug.Log("AI finished lap!");
                if (aiLaps[i] == requiredLaps)
                {
                    // consider ending the game, this racer is done.
                }
                return;
            }
        }
    }

    public void PlayerCheckpoint(Checkpoint cp)
    {
        if (cp == checkpoints[currentCheckpoint])
        {
            currentCheckpoint += 1;
            if (currentCheckpoint == checkpoints.Length)
            {
                currentCheckpoint = 0;
                playerLaps += 1;
                Debug.Log("Player finished lap!");
                if (playerLaps == requiredLaps)
                {
                    // consider ending the game, the player is done.
                }
            }
        }
    }

    private void CountdownTimerSet(int seconds)
    {
        countdownDelay = seconds;
        countdownTimerStart = Time.time;
    }

    int CountdownTimeRemaining()
    {
        int elapsedTime = (int)(Time.time - countdownTimerStart);
        int secondsLeft = countdownDelay - elapsedTime;
        return secondsLeft;
    }

    Texture2D CountdownTimerImage()
    {
        switch (CountdownTimeRemaining())
        {
            case 3: return num3Image;
            case 2: return num2Image;
            case 1: return num1Image;
            case 0:
                foreach (Rigidbody rb in aiCars)
                {
                    rb.isKinematic = false;
                }
                playerCar.isKinematic = false;
                Started = true;
                return startRaceImage;
            default: return null;
        }
    }

    private void OnGUI()
    {
        if (Started) return;

        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(CountdownTimerImage());
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
    }
}
