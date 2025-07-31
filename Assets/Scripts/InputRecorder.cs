using System.Collections.Generic;
using UnityEngine;

public class InputRecorder : MonoBehaviour
{
    private List<PlayerInputFrame> inputHistory = new();
    private bool isRecording = true;

    private Vector3 spawnPosition;
    
    public bool IsRecording => isRecording;
    public List<PlayerInputFrame> InputHistory => new List<PlayerInputFrame>(inputHistory);
    public int FrameCount => inputHistory.Count;

    void Awake()
    {
        spawnPosition = transform.position;
    }

    public Vector3 GetSpawnPosition()
    {
        return spawnPosition;
    }

    public void StartRecording()
    {
        isRecording = true;
        inputHistory.Clear();
    }
    
    public void StopRecording()
    {
        isRecording = false;
    }
    public void RecordInput(float horizontal, bool jumpHeld)
    {
        if (isRecording)
        {
            float time = TimelineManager.Instance.GetCurrentTime();
            inputHistory.Add(new PlayerInputFrame(horizontal, jumpHeld, time));
        }
    }

        
    public void ClearHistory()
    {
        inputHistory.Clear();
    }
}