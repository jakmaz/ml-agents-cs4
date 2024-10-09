using UnityEngine;

public class AgentFairSoccer : AgentSoccer
{
    /// <summary>
    /// Override the OnCollisionEnter method to penalize agents for colliding with opposing players.
    /// </summary>
    void OnCollisionEnter(Collision c)
    {
        base.OnCollisionEnter(c);  // Call the base method for handling other collisions (e.g., ball)

        // Check for collisions with opposing players and apply penalties
        if (team == Team.Blue && c.gameObject.CompareTag("purpleAgent"))
        {
            AddReward(-0.5f); // Penalize blue agent for colliding with a purple agent
        }
        else if (team == Team.Purple && c.gameObject.CompareTag("blueAgent"))
        {
            AddReward(-0.5f); // Penalize purple agent for colliding with a blue agent
        }
    }
}
