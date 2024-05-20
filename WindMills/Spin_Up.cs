using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ImportedComponent]
public class Spin_Up : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DaggerfallAudioSource source1 = gameObject.AddComponent<DaggerfallAudioSource>();
        SoundClips sound = SoundClips.ArenaFireDaemon;
        SoundClips sound2 = SoundClips.BlowingWindIntro;
        source1.SetSound(sound, AudioPresets.LoopOnAwake);
        //source1.AudioSource.maxDistance = 5000;
        DaggerfallAudioSource source2 = gameObject.AddComponent<DaggerfallAudioSource>();
        //source2.SetSound(sound2, AudioPresets.LoopOnAwake);
        //source2.AudioSource.maxDistance = 5000;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0f, 0f, -13 * Time.deltaTime, Space.Self);
    }
}
