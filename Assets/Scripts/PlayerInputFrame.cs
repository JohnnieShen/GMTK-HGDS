using UnityEngine;
[System.Serializable]
public class PlayerInputFrame
{
    public float time;
    public float horizontal;
    public bool jump;

    public PlayerInputFrame(float time, float horizontal, bool jump)
    {
        this.time = time;
        this.horizontal = horizontal;
        this.jump = jump;
    }
}
