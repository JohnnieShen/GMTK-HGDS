using UnityEngine;

public static class WwiseAudioGate
{
    // Keep disabled while Spectral Imprints replaces character/lifetime interaction audio.
    public static bool CharacterLifetimeSfxEnabled = false;

    public static void PostCharacterLifetimeEvent(string eventName, GameObject source)
    {
        if (!CharacterLifetimeSfxEnabled) return;

        AkSoundEngine.PostEvent(eventName, source);
    }
}
