using UnityEngine;
[System.Serializable]
public class PlayerInputFrame
{
    public float horizontal;
    public bool jump;
    public float time;
    public Vector2 velocity;
    public Vector2 position;
    public bool interact;

    public PlayerInputFrame(float horizontal, bool jump, float time, Vector2 velocity, Vector2 position, bool interact)
    {
        this.horizontal = horizontal;
        this.jump = jump;
        this.time = time;
        this.velocity = velocity;
        this.position = position;
        this.interact = interact;
    }
}
