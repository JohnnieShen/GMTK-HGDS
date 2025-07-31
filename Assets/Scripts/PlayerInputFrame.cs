using UnityEngine;
[System.Serializable]
public class PlayerInputFrame
{
    public float horizontal;
    public bool jump;

    public PlayerInputFrame(float horizontal, bool jump)
    {
        this.horizontal = horizontal;
        this.jump = jump;
    }
}
