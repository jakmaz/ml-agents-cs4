using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using Unity.MLAgents.Sensors;

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

    /// <summary>
    /// The area bounds.
    /// </summary>

    /// <summary>
    /// We will be changing the ground material based on success/failue
    /// </summary>

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

    void Start()
    {

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
            ResetScene();
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
        if (scoredTeam == Team.Blue)
        {
            m_BlueAgentGroup.AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_PurpleAgentGroup.AddGroupReward(-1);
        }
        else
        {
            m_PurpleAgentGroup.AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_BlueAgentGroup.AddGroupReward(-1);
        }
        m_PurpleAgentGroup.EndGroupEpisode();
        m_BlueAgentGroup.EndGroupEpisode();
        ResetScene();

    }


    public void ResetScene()
    {
        m_ResetTimer = 0;

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
}
