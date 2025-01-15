using UnityEngine;
using UnityEngine.Profiling;

public class PerformanceMetrics{
    public Team? Winner { get; set; }
    public int GameDuration { get; set; }
    public float BlueRewards { get; set; }
    public float BluePenalties { get; set; }
    public float PurpleRewards { get; set; }
    public float PurplePenalties { get; set; }
    public float AverageFrameRate { get; set; } 
    public float AverageCPUUsage { get; set; }  
    public long MemoryUsage { get; set; }       
}