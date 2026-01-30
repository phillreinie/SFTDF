using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Audio/Audio Library")]
public class AudioLibrarySO : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public SfxEvent id;
        public List<AudioClip> clips = new();
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.5f, 2f)] public float pitchMin = 0.95f;
        [Range(0.5f, 2f)] public float pitchMax = 1.05f;
    }

    public List<Entry> entries = new();

    public bool TryGet(SfxEvent id, out Entry entry)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].id == id) { entry = entries[i]; return true; }
        }
        entry = null;
        return false;
    }
}