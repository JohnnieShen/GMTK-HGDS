using UnityEngine;

[System.Serializable]
public class PropStatusFrame
{
    public int propId;
    public float time;
    public bool active;
    public Vector3 position;
    // public int spriteIndex;

    public PropStatusFrame(int id, float t, bool active,
                           Vector3 pos)
    {
        propId = id;
        time = t;
        this.active = active;
        position = pos;
        // this.spriteIndex = spriteIndex;
    }
}
