using System.Collections.Generic;
using UnityEngine;

public class InputRecorder : MonoBehaviour
{
    private List<PlayerInputFrame> inputHistory = new();
    private float timeElapsed = 0f;
    private bool isRecording = true;
    
    public bool IsRecording => isRecording;
    public List<PlayerInputFrame> InputHistory => new List<PlayerInputFrame>(inputHistory);
    public int FrameCount => inputHistory.Count;
    
    void Update()
    {
        timeElapsed += Time.deltaTime;
    }
    
    public void StartRecording()
    {
        isRecording = true;
        timeElapsed = 0f;
        inputHistory.Clear();
    }
    
    public void StopRecording()
    {
        isRecording = false;
    }
    
    public void RecordInput(float horizontal, bool jump)
    {
        if (isRecording)
        {
            inputHistory.Add(new PlayerInputFrame(timeElapsed, horizontal, jump));
        }
    }
    
    public void ClearHistory()
    {
        inputHistory.Clear();
        timeElapsed = 0f;
    }
}
