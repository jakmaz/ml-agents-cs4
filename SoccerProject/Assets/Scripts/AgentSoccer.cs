using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;

public enum Team
{
    Blue = 0,
    Purple = 1
}

public class AgentSoccer : Agent
{
    // Note that that the detectable tags are different for the blue and purple teams. The order is
    // * ball
    // * own goal
    // * opposing goal
    // * wall
    // * own teammate
    // * opposing player

    public enum Position
    {
        Striker,
        Goalie,
        Generic
    }

    [HideInInspector]
    public Team team;
    float m_KickPower;
    // The coefficient for the reward for colliding with a ball. Set using curriculum.
    float m_BallTouch;
    public Position position;

    const float k_Power = 2000f;
    float m_Existential;
    float m_LateralSpeed;
    float m_ForwardSpeed;
    public float VisionAngle => m_VisionAngle;
    private float m_VisionAngle = 0f; // Current vision angle relative to forward direction
    private float m_VisionRotateSpeed = 180f; // Degrees per second


    [HideInInspector]
    public Rigidbody agentRb;
    SoccerSettings m_SoccerSettings;
    BehaviorParameters m_BehaviorParameters;
    public Vector3 initialPos;
    public float rotSign;

    EnvironmentParameters m_ResetParams;

    [Header("Extended Features")]
    public bool avoidFouls = false;
    public bool noBackRays = false;
    public bool decoupledVision = false;
    public bool soundSensor = false;

    public override void Initialize()
    {
        SoccerEnvController envController = GetComponentInParent<SoccerEnvController>();
        if (envController != null)
        {
            m_Existential = 1f / envController.MaxEnvironmentSteps;
        }
        else
        {
            m_Existential = 1f / MaxStep;
        }

        m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        if (m_BehaviorParameters.TeamId == (int)Team.Blue)
        {
            team = Team.Blue;
            initialPos = new Vector3(transform.position.x - 5f, .5f, transform.position.z);
            rotSign = 1f;
        }
        else
        {
            team = Team.Purple;
            initialPos = new Vector3(transform.position.x + 5f, .5f, transform.position.z);
            rotSign = -1f;
        }
        if (position == Position.Goalie)
        {
            m_LateralSpeed = 1.0f;
            m_ForwardSpeed = 1.0f;
        }
        else if (position == Position.Striker)
        {
            m_LateralSpeed = 0.3f;
            m_ForwardSpeed = 1.3f;
        }
        else
        {
            m_LateralSpeed = 0.3f;
            m_ForwardSpeed = 1.0f;
        }
        m_SoccerSettings = FindObjectOfType<SoccerSettings>();
        agentRb = GetComponent<Rigidbody>();
        agentRb.maxAngularVelocity = 500;

        m_ResetParams = Academy.Instance.EnvironmentParameters;

        // Add the "No Back Rays" functionality here
        HandleNoBackRays();
    }

    private void HandleNoBackRays()
    {
        // Find all child objects with the tag "ReverseRays"
        RayPerceptionSensorComponent3D reverseRaySensor = null;
        foreach (Transform child in transform)
        {
            if (child.CompareTag("reverseRays"))
            {
                reverseRaySensor = child.GetComponent<RayPerceptionSensorComponent3D>();
                break;
            }
        }

        if (reverseRaySensor != null)
        {
            if (noBackRays)
            {
                DestroyImmediate(reverseRaySensor); // Completely remove the sensor
                Debug.Log($"ReverseRays sensor has been destroyed.");
            }
            else
            {
                Debug.Log($"ReverseRays sensor is enabled.");
            }
        }
        else
        {
            Debug.LogWarning($"No RayPerceptionSensorComponent3D with tag 'ReverseRays' found!");
        }
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        m_KickPower = 0f;

        var forwardAxis = act[0];
        var rightAxis = act[1];
        var rotateAxis = act[2];

        int visionAxis = 0;

        if (decoupledVision && act.Length > 3)
        {
            visionAxis = act[3]; // Only access visionAxis if it exists
        }

        switch (forwardAxis)
        {
            case 1:
                dirToGo = transform.forward * m_ForwardSpeed;
                m_KickPower = 1f;
                break;
            case 2:
                dirToGo = transform.forward * -m_ForwardSpeed;
                break;
        }

        switch (rightAxis)
        {
            case 1:
                dirToGo = transform.right * m_LateralSpeed;
                break;
            case 2:
                dirToGo = transform.right * -m_LateralSpeed;
                break;
        }

        switch (rotateAxis)
        {
            case 1:
                rotateDir = transform.up * -1f;
                break;
            case 2:
                rotateDir = transform.up * 1f;
                break;
        }

        switch (visionAxis)
        {
            case 1:
                m_VisionAngle -= m_VisionRotateSpeed * Time.deltaTime;
                break;
            case 2:
                m_VisionAngle += m_VisionRotateSpeed * Time.deltaTime;
                break;
        }

        // Vision control logic
        if (decoupledVision)
        {
            switch (visionAxis)
            {
                case 1:
                    m_VisionAngle -= m_VisionRotateSpeed * Time.deltaTime;
                    break;
                case 2:
                    m_VisionAngle += m_VisionRotateSpeed * Time.deltaTime;
                    break;
            }
            m_VisionAngle = Mathf.Repeat(m_VisionAngle, 360f);
        }

        // Apply movement and rotation
        transform.Rotate(rotateDir, Time.deltaTime * 100f);
        agentRb.AddForce(dirToGo * m_SoccerSettings.agentRunSpeed, ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)

    {

        if (position == Position.Goalie)
        {
            // Existential bonus for Goalies.
            AddReward(m_Existential);
        }
        else if (position == Position.Striker)
        {
            // Existential penalty for Strikers
            AddReward(-m_Existential);
        }
        MoveAgent(actionBuffers.DiscreteActions);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        //forward
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
        //rotate
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[2] = 2;
        }
        //right
        if (Input.GetKey(KeyCode.E))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            discreteActionsOut[1] = 2;
        }

        // Vision control
        if (decoupledVision)
        {
            if (Input.GetKey(KeyCode.Z))
            {
                discreteActionsOut[3] = 1; // Rotate vision left
            }
            if (Input.GetKey(KeyCode.C))
            {
                discreteActionsOut[3] = 2; // Rotate vision right
            }
        }
    }
    /// <summary>
    /// Used to provide a "kick" to the ball.
    /// </summary>
    public void OnCollisionEnter(Collision c)
    {
        var force = k_Power * m_KickPower;
        if (position == Position.Goalie)
        {
            force = k_Power;
        }
        if (c.gameObject.CompareTag("ball"))
        {
            AddReward(.2f * m_BallTouch);
            var dir = c.contacts[0].point - transform.position;
            dir = dir.normalized;
            c.gameObject.GetComponent<Rigidbody>().AddForce(dir * force);
        }

        // Collision avoidance logic for fouls
        if (avoidFouls)
        {
            // Penalize for colliding with opposing players
            if (team == Team.Blue && c.gameObject.CompareTag("purpleAgent"))
            {
                AddReward(-0.01f); // Penalize blue agent for colliding with a purple agent
            }
            else if (team == Team.Purple && c.gameObject.CompareTag("blueAgent"))
            {
                AddReward(-0.01f); // Penalize purple agent for colliding with a blue agent
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        m_BallTouch = m_ResetParams.GetWithDefault("ball_touch", 0);
    }

}
