using System.Collections;
using UnityEngine;
using Unity.MLAgents.Sensors;

public enum SoundType
{
    BallWithObjectCollision = 2,   // Highest priority
    BallWithAgentCollision = 3,   // Medium priority
    AgentWithObjectCollision = 1  // Lowest priority
}

public class SoundSensor : SensorComponent
{
    public Transform agentTransform; // Reference to the agent for calculations
    public float maxSoundRange = 10f; // Maximum range to hear sound, affects intensity calculation

    private Vector3 soundDirection;
    private float soundIntensity;
    private SoundType soundType;
    private bool isSoundDetected;
    private int currentPriority = 0; // Tracks the current sound's priority

    public void Initialize(Transform agent)
    {
        agentTransform = agent;
    }

    /// <summary>
    /// Sets the sound data, combining initial intensity and distance attenuation.
    /// </summary>
    /// <param name="soundPosition">Position of the sound source.</param>
    /// <param name="type">Type of the sound.</param>
    /// <param name="initialIntensity">Initial intensity of the sound (e.g., based on collision force).</param>
    public void SetSoundData(Vector3 soundPosition, SoundType type, float initialIntensity)
    {
        if (agentTransform == null)
        {
            Debug.LogError("SS agenttransform not set");
            return;
        }
        int priority = (int)type;

        // Only update if the new sound has a higher priority
        if (priority >= currentPriority)
        {
            soundDirection = (soundPosition - agentTransform.position).normalized;
            float distance = Vector3.Distance(agentTransform.position, soundPosition);

            // Combine initial intensity with distance attenuation
            float distanceAttenuation = Mathf.Clamp01(1 - (distance / maxSoundRange));
            soundIntensity = Mathf.Clamp01(initialIntensity * distanceAttenuation);

            soundType = type;
            isSoundDetected = soundIntensity > 0; // Detect sound only if within range
            currentPriority = priority;
        }
    }

    // Trigger sound events with initial intensity
    public void TriggerBallWithObjectCollision(Vector3 soundPosition, float initialIntensity)
    {
        SetSoundData(soundPosition, SoundType.BallWithObjectCollision, initialIntensity);
    }

    public void TriggerBallWithAgentCollision(Vector3 soundPosition, float initialIntensity)
    {
        SetSoundData(soundPosition, SoundType.BallWithAgentCollision, initialIntensity);
    }

    public void TriggerAgentWithObjectCollision(Vector3 soundPosition, float initialIntensity)
    {
        SetSoundData(soundPosition, SoundType.AgentWithObjectCollision, initialIntensity);
    }

    /// <summary>
    /// Resets the sound detection at the start of a new frame or episode.
    /// </summary>
    public void ResetSound()
    {
        isSoundDetected = false;
        currentPriority = 0; // Reset priority for new sounds
    }

    /// <summary>
    /// Initializes the sensor for ML-Agents.
    /// </summary>
    public override ISensor[] CreateSensors()
    {
        return new ISensor[] { new VectorSensor(5, "SoundSensor") };
    }

    /// <summary>
    /// Updates the sensor with current sound data.
    /// </summary>
    public void UpdateSensor(VectorSensor sensor)
    {
        if (agentTransform == null)
        {
            Debug.LogWarning("Agent Transform not set for SoundSensor");
            return;
        }
        if (isSoundDetected)
        {
            // Add sound direction (x, y, z)
            sensor.AddObservation(soundDirection);

            // Add sound intensity
            sensor.AddObservation(soundIntensity);

            // Add sound type as integer
            sensor.AddObservation((int)soundType);
        }
        else
        {
            // Add zero values when no sound is detected
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0); // Intensity
            sensor.AddObservation(0); // Sound type
        }
    }
}