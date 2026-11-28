using UnityEngine;

public class WrongSlotSound : MonoBehaviour
{
    // Attach this to an active object and call PlayWrongSlotSound on a wrong slot.
    AudioSource wrongSlotAudio;
    AudioClip wrongSlotClip;

    void Awake()
    {
        wrongSlotClip = CreateWrongSlotClip();
        wrongSlotAudio = gameObject.AddComponent<AudioSource>();
        wrongSlotAudio.playOnAwake = false;
        wrongSlotAudio.spatialBlend = 0f; // Play at the same volume wherever the player looks.
        wrongSlotAudio.volume = 0.4f;
        wrongSlotAudio.clip = wrongSlotClip;
    }

    public void PlayWrongSlotSound()
    {
        wrongSlotAudio.Play();
    }

    static AudioClip CreateWrongSlotClip()
    {
        const int samplesPerMillisecond = 24;
        const int sampleRate = samplesPerMillisecond * 1000;
        const int firstNoteLength = 110 * samplesPerMillisecond;
        const int pauseLength = 25 * samplesPerMillisecond;
        const int secondNoteLength = 150 * samplesPerMillisecond;
        const int secondNoteStart = firstNoteLength + pauseLength;

        // Play a high note, leave a short pause, then play a lower note.
        float[] samples = new float[secondNoteStart + secondNoteLength];
        AddNote(samples, 0, firstNoteLength, sampleRate, 523.25f);
        AddNote(samples, secondNoteStart, secondNoteLength, sampleRate, 392f);

        AudioClip clip = AudioClip.Create("Wrong Slot", samples.Length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    static void AddNote(float[] samples, int start, int length, int sampleRate, float frequency)
    {
        for (int i = 0; i < length; i++)
        {
            float progress = (float)i / (length - 1);
            float fade = Mathf.Sin(Mathf.PI * progress);
            float time = (float)i / sampleRate;
            float tone = Mathf.Sin(2f * Mathf.PI * frequency * time);
            samples[start + i] = tone * fade;
        }
    }

    void OnDestroy()
    {
        Destroy(wrongSlotClip);
    }
}
