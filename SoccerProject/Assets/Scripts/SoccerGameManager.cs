using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class SoccerGameManager : MonoBehaviour
{
    public List<SoccerEnvController> soccerFields = new List<SoccerEnvController>();
    private int fieldsCompleted = 0;

    void Start()
    {
        //Debug.Log("SoccerGameManager initialized and active.");
        // Automatically find all SoccerEnvController instances in the scene
        SoccerEnvController[] fields = FindObjectsOfType<SoccerEnvController>();
        foreach (var field in fields)
        {
            soccerFields.Add(field);
        }
        //Debug.Log($"Total number of soccer fields found: {soccerFields.Count}");

    }

    public void OnFieldCompleted()
    {
        fieldsCompleted++;

        //Debug.Log($"Field completed. Total completed: {fieldsCompleted} of {soccerFields.Count}");
        
        // To check if all fields have finished their games
        if (fieldsCompleted >= soccerFields.Count)
        {
            Debug.Log("All fields completed their games. Saving metrics and closing simulation.");
            SaveAllMetrics();
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // Stop the Unity Editor
            #else
            Application.Quit(); // Stop the application in a build
            #endif        
        }
    }

    private void SaveAllMetrics()
    {
        string filePath = Application.dataPath + "/AllPerformanceMetrics.csv";
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath))
        {
            file.WriteLine("Game,Field,Winner,GameDuration,BlueRewards,BluePenalties,PurpleRewards,PurplePenalties,FrameRate,CPU(ms),Memory(MB)");

            foreach (var field in soccerFields)
            {
                var metricsList = field.GetPerformanceMetrics();
                if (metricsList == null || metricsList.Count == 0)
                {
                    Debug.LogWarning($"No metrics found for Field {field.FieldIndex}");
                    continue;
                }

                for (int i = 0; i < metricsList.Count; i++)
                {
                    var metrics = metricsList[i];
                    if (metrics.GameDuration == 0) continue; //skip invalid games
                    
                    string winnerString = metrics.Winner.HasValue ? metrics.Winner.ToString() : "NoWinner";
                    file.WriteLine($"{i + 1},{field.FieldIndex},{winnerString},{metrics.GameDuration},{metrics.BlueRewards},{metrics.BluePenalties},{metrics.PurpleRewards},{metrics.PurplePenalties},{metrics.AverageFrameRate:F2},{metrics.AverageCPUUsage:F2},{metrics.MemoryUsage}");
                }
            }
        }

        Debug.Log($"Metrics saved to {filePath}");
    }
}
