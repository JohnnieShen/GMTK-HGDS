using System.Collections.Generic;
using UnityEngine;

public enum ChapterMusic
{
    MainMenu,
    Chapter1,
    Chapter2,
    Chapter3,
    Chapter4,
    Chapter5
}

[CreateAssetMenu(menuName = "Audio/Scene Music Database")]
public class SceneMusicDatabase : ScriptableObject
{
    [System.Serializable]
    public class SceneMusicPair
    {
        public string sceneName;

        [Tooltip("Must exactly match a Wwise switch name in the BGM switch group")]
        public ChapterMusic musicSwitch; 
    }

    public List<SceneMusicPair> sceneMusicPairs = new();
}