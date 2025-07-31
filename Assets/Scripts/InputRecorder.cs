using System.Collections.Generic;
using UnityEngine;

public class InputRecorder : MonoBehaviour
{
    private List<PlayerInputFrame> inputHistory = new();
    private bool isRecording = true;
    
    public bool IsRecording => isRecording;
    public List<PlayerInputFrame> InputHistory => new List<PlayerInputFrame>(inputHistory);
    public int FrameCount => inputHistory.Count;
        
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
            inputHistory.Add(new PlayerInputFrame(horizontal, jumpHeld));
        }
    }
    
    public void ClearHistory()
    {
        inputHistory.Clear();
    }
}