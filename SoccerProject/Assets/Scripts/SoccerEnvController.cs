using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using Unity.MLAgents.Sensors;
using UnityEngine.Profiling;

public class SoccerEnvController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerInfo
    {
        public AgentSoccer Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }

    /// <summary>
    /// Max Academy steps before this platform resets
    /// </summary>
    /// <returns></returns>
    [Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;

    public int FieldIndex; // Unique index for each soccer field
    public GameObject ball;
    [HideInInspector]
    public Rigidbody ballRb;
    Vector3 m_BallStartingPos;

    //List of Agents On Platform
    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();

    private SoccerSettings m_SoccerSettings;


    private SimpleMultiAgentGroup m_BlueAgentGroup;
    private SimpleMultiAgentGroup m_PurpleAgentGroup;

    private int m_ResetTimer;

    // Reference to ball's sound sensor
    private SoundSensor ballSoundSensor;
    
    
    private int currentGameCount = 0;
    private int maxGames = 10;
    private bool hasCompletedGames = false; // Tracks if this field has already completed its games

    private float blueTeamRewards = 0f;
    private float blueTeamPenalties = 0f;
    private float purpleTeamRewards = 0f;
    private float purpleTeamPenalties = 0f;
    private float totalFrameTime = 0f; 
    private int frameCount = 0;        
    private Recorder cpuUsageRecorder; //CPU usage tracker

    private List<PerformanceMetrics> performanceMetricsList = new List<PerformanceMetrics>();
    
    private SoccerGameManager gameManager;


    void Start()
    {
        //Find the SoccerGameManager in the scene
        gameManager = FindObjectOfType<SoccerGameManager>();
        if (gameManager == null)
        {
            Debug.LogError("SoccerGameManager not found in the scene!");
        }
        
        m_SoccerSettings = FindObjectOfType<SoccerSettings>();

        // Initialize TeamManager
        m_BlueAgentGroup = new SimpleMultiAgentGroup();
        m_PurpleAgentGroup = new SimpleMultiAgentGroup();
        ballRb = ball.GetComponent<Rigidbody>();
        m_BallStartingPos = new Vector3(ball.transform.position.x, ball.transform.position.y, ball.transform.position.z);

        // Attach a collider and a SoundSensor to the ball for collision detection
        ballSoundSensor = ball.GetComponent<SoundSensor>();
        if(ballSoundSensor == null)
        {
            ballSoundSensor = ball.AddComponent<SoundSensor>();
            ballSoundSensor.agentTransform = ball.transform;
        }

        // Initialize performance tracking
        cpuUsageRecorder = Recorder.Get("Main Thread");

        foreach (var item in AgentsList)
        {
            item.StartingPos = item.Agent.transform.position;
            item.StartingRot = item.Agent.transform.rotation;
            item.Rb = item.Agent.GetComponent<Rigidbody>();
            if (item.Agent.team == Team.Blue)
            {
                m_BlueAgentGroup.RegisterAgent(item.Agent);
            }
            else
            {
                m_PurpleAgentGroup.RegisterAgent(item.Agent);
            }
        }
        ResetScene();
    }

    void FixedUpdate()
    {
        if (hasCompletedGames)
        {
            return;
        }
        
        // Track frame rate
        totalFrameTime += Time.deltaTime;
        frameCount++;

        m_ResetTimer += 1;
        
        // Regularly update ball's sound sensor
        if (ballSoundSensor != null)
        {
            var vectorSensor = new VectorSensor(5, "SoundSensor");
            ballSoundSensor.UpdateSensor(vectorSensor);
        }

        // Check for collisions manually (if OnCollisionEnter is not working)
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.5f); // Adjust radius as needed
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("wall"))
            {
                Debug.Log("Detected collision manually with: " + hitCollider.gameObject.name);
                HandleCollision(hitCollider); // Call your logic here
            }
        }

        if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            m_BlueAgentGroup.GroupEpisodeInterrupted();
            m_PurpleAgentGroup.GroupEpisodeInterrupted();
            EndGame(null); //If timeout-> no winner
        }
    }

    // Function to handle collision logic
    private void HandleCollision(Collider collider)
    {
        // Logic for handling collision
        float collisionSpeed = (transform.position - collider.transform.position).magnitude; // Example speed calculation
        float initialIntensity = Mathf.Clamp01(collisionSpeed / 10f); // Scale to 0-1 range
        Vector3 soundPosition = transform.position;

        if (ballSoundSensor != null)
        {
            ballSoundSensor.TriggerBallWithObjectCollision(soundPosition, initialIntensity);
            BroadcastSoundToAgents(soundPosition, SoundType.BallWithObjectCollision, initialIntensity);
        }
    }

    public void ResetBall()
    {
        var randomPosX = Random.Range(-2.5f, 2.5f);
        var randomPosZ = Random.Range(-2.5f, 2.5f);

        ball.transform.position = m_BallStartingPos + new Vector3(randomPosX, 0f, randomPosZ);
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

    }

    public void GoalTouched(Team scoredTeam)
    {
        if (m_ResetTimer == 0){
            return; //prevent goal from being processed because duration is 0
        }

        float reward = 1 - (float)m_ResetTimer / MaxEnvironmentSteps; // Calculate reward based on time efficiency

        if (scoredTeam == Team.Blue)
        {
            m_BlueAgentGroup.AddGroupReward(reward);
            m_PurpleAgentGroup.AddGroupReward(-1);

            blueTeamRewards += reward;
            purpleTeamPenalties += 1;
        }
        else
        {
            m_PurpleAgentGroup.AddGroupReward(reward);
            m_BlueAgentGroup.AddGroupReward(-1);

            purpleTeamRewards += reward;
            blueTeamPenalties += 1;
        }
        EndGame(scoredTeam);

    }


    public void ResetScene()
    {
        m_ResetTimer = 0;
        blueTeamRewards = 0f;
        blueTeamPenalties = 0f;
        purpleTeamRewards = 0f;
        purpleTeamPenalties = 0f;

        //Reset Agents
        foreach (var item in AgentsList)
        {
            var randomPosX = Random.Range(-5f, 5f);
            var newStartPos = item.Agent.initialPos + new Vector3(randomPosX, 0f, 0f);
            var rot = item.Agent.rotSign * Random.Range(80.0f, 100.0f);
            var newRot = Quaternion.Euler(0, rot, 0);
            item.Agent.transform.SetPositionAndRotation(newStartPos, newRot);

            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
        }

        //Reset Ball
        ResetBall();
    }
    private void BroadcastSoundToAgents(Vector3 soundPosition, SoundType soundType, float initialIntensity)
    {
        foreach (var player in AgentsList)
        {
            var soundSensor = player.Agent.GetComponent<SoundSensor>();
            if (soundSensor != null)
            {
                soundSensor.SetSoundData(soundPosition, soundType, initialIntensity);
                Debug.Log($"Broadcasting sound to {player.Agent.name}: Position={soundPosition}, Intensity={initialIntensity}, Type={soundType}");
            }
        }
    }

    private void EndGame(Team? winner)
    {
        if(m_ResetTimer == 0){
            Debug.LogError($"Game ended with duration 0. No winner should be declared.");
            winner = null; 
        }

        if (hasCompletedGames)
        {
            return; // Prevent further processing if the field is already marked as complete
        }

         // Calculate performance metrics
        float averageFrameRate = frameCount > 0 ? frameCount / totalFrameTime : 0f;
        float cpuUsage = cpuUsageRecorder != null ? cpuUsageRecorder.elapsedNanoseconds / 1e6f : 0f;
        long memoryUsage = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024); //converts bytes to MB

        PerformanceMetrics metrics = new PerformanceMetrics
        {
            Winner = winner, 
            GameDuration = m_ResetTimer,
            BlueRewards = blueTeamRewards,
            BluePenalties = blueTeamPenalties,
            PurpleRewards = purpleTeamRewards,
            PurplePenalties = purpleTeamPenalties,
            AverageFrameRate = averageFrameRate,
            AverageCPUUsage = cpuUsage,
            MemoryUsage = memoryUsage
        };

        performanceMetricsList.Add(metrics);
        currentGameCount++;

        //change the number of games through here pls, vs code is going crazy
        if (currentGameCount >= maxGames || winner == (null))
        {
            //Debug.Log($"Field {FieldIndex} completed its games.");
            hasCompletedGames = true;
            ResetToIdleState();
            gameManager.OnFieldCompleted(); // Notifies the manager
        }
        else
        {
            ResetScene();
        }
    }

    public List<PerformanceMetrics> GetPerformanceMetrics()
    {
        return performanceMetricsList ?? new List<PerformanceMetrics>();
    }

    private void ResetToIdleState()
    {
        // Reset Agents
        foreach (var item in AgentsList)
        {
            item.Agent.transform.position = item.StartingPos;
            item.Agent.transform.rotation = item.StartingRot;
            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;

            // Deactivate agent to stop it from playing
            item.Agent.IsActive = false;
        }

        // Reset Ball
        ball.transform.position = m_BallStartingPos;
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        
        //Debug.Log($"Field {FieldIndex} is now idle.");
    }
}
