using UnityEngine;
[System.Serializable]
public class PlayerInputFrame
{
    public float horizontal;
    public bool jump;

    public float time; 

    public PlayerInputFrame(float horizontal, bool jump, float time)
    {
        this.horizontal = horizontal;
        this.jump = jump;
        this.time = time;
    }

}
