using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {

    private float volume = 0.3f;
    private AudioSource audioSource;
    public static MusicManager Instance { get; private set; }

    private void Awake() {
        Instance = this;

        audioSource = GetComponent<AudioSource>();
    }
    public void ChangeVolume() {
        volume += .1f;
        if (volume > 1f) {
            volume = 0f;
        }
        audioSource.volume = volume;
    }

    public float GetVolume() {
        return volume;
    }
}
