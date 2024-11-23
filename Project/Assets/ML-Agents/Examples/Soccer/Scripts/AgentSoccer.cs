using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

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

    public Vector3 opponentGoalPosition;
    public Vector3 ownGoalPosition;

    float m_KickPower;
    // The coefficient for the reward for colliding with a ball. Set using curriculum.
    float m_BallTouch;
    public Position position;

    const float k_Power = 2000f;
    float m_Existential;
    float m_LateralSpeed;
    float m_ForwardSpeed;


    [HideInInspector]
    public Rigidbody agentRb;
    SoccerSettings m_SoccerSettings;
    BehaviorParameters m_BehaviorParameters;
    public Vector3 initialPos;
    public float rotSign;
    public GameObject ball;

    EnvironmentParameters m_ResetParams;

    public override void Initialize()
    {
        SoccerEnvController envController = GetComponentInParent<SoccerEnvController>();
        if (envController != null)
        {
            ball = envController.ball;
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

        if (team == Team.Blue)
        {
            ownGoalPosition = new Vector3(-1650, -25, -1.5258f); // Blue goal position
            opponentGoalPosition = new Vector3(1650, -25, 1.5258f); // Purple goal position
        }
        else
        {
            ownGoalPosition = new Vector3(1650, -25, 1.5258f); // Purple goal position
            opponentGoalPosition = new Vector3(-1650, -25, -1.5258f); // Blue goal position
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

        transform.Rotate(rotateDir, Time.deltaTime * 100f);
        agentRb.AddForce(dirToGo * m_SoccerSettings.agentRunSpeed,
            ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var soundSensor = GetComponent<SoundSensor>();
        if (soundSensor != null)
        {
            soundSensor.ResetSound();
        }

        if (position == Position.Goalie)
        {
            AddReward(m_Existential);
        }
        else if (position == Position.Striker)
        {
            AddReward(-m_Existential);
        }

        MoveAgent(actionBuffers.DiscreteActions);

        float distanceToOpponentGoal = Vector3.Distance(transform.position, opponentGoalPosition);
        float distanceBallToOpponentGoal = Vector3.Distance(ball.transform.position, opponentGoalPosition);

        if (distanceToOpponentGoal < 5f && distanceBallToOpponentGoal < 5f)
        {
            AddReward(0.1f);
        }
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
    }
    /// <summary>
    /// Used to provide a "kick" to the ball.
    /// </summary>
    void OnCollisionEnter(Collision c)
    {
        var soundSensor = GetComponent<SoundSensor>();
        if (soundSensor != null)
        {
            // Calculate initial intensity based on collision speed
            float collisionSpeed = c.relativeVelocity.magnitude;
            float initialIntensity = Mathf.Clamp01(collisionSpeed / 10f); // Scale to a 0-1 range

            if (c.gameObject.CompareTag("ball"))
            {
                // Trigger sound for ball collision
                soundSensor.TriggerBallWithAgentCollision(transform.position, initialIntensity);

                // Existing logic for rewards and physics
                Vector3 directionToOpponentGoal = (opponentGoalPosition - transform.position).normalized;
                Vector3 directionToOwnGoal = (ownGoalPosition - transform.position).normalized;
                float alignmentWithOpponentGoal = Vector3.Dot(directionToOpponentGoal, transform.forward);
                float alignmentWithOwnGoal = Vector3.Dot(directionToOwnGoal, transform.forward);

                if (alignmentWithOpponentGoal > 0.8f)
                {
                    AddReward(0.5f); // Positive reward
                }

                if (alignmentWithOwnGoal > 0.8f)
                {
                    AddReward(-0.5f); // Negative reward
                }

                var force = k_Power * m_KickPower;
                if (position == Position.Goalie) force = k_Power;
                var dir = c.contacts[0].point - transform.position;
                dir = dir.normalized;
                c.gameObject.GetComponent<Rigidbody>().AddForce(dir * force);

                AddReward(.2f * m_BallTouch);
            }
            else if (c.gameObject.CompareTag("wall"))
            {
                // Trigger sound for wall collision
                soundSensor.TriggerAgentWithObjectCollision(transform.position, initialIntensity);
            }
            else if (c.gameObject.CompareTag("object"))
            {
                // Trigger sound for ball with object collision
                soundSensor.TriggerBallWithObjectCollision(transform.position, initialIntensity);
            }
        }
    }



    public override void OnEpisodeBegin()
    {
        m_BallTouch = m_ResetParams.GetWithDefault("ball_touch", 0);

        // Reset the sound sensor
        var soundSensor = GetComponent<SoundSensor>();
        if (soundSensor != null)
        {
            soundSensor.ResetSound();
        }
    }


}
