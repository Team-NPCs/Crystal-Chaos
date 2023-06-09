using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxScript : MonoBehaviour {
    [SerializeField] private AudioSource earthAudio, airAudio, voidAudio, waterAudio, fireAudio;
    [SerializeField] private AudioSource healthPotionAudio, movementPotionAudio;
    public AudioSource deathAudio;
    public AudioSource crystalBallPickUpAudio;
    public readonly Dictionary<CrystalType, AudioSource> spellAudio = new();
    public readonly Dictionary<PotionType, AudioSource> potionAudio = new();

    public readonly float soundVolume = 0.15f;

    void Awake() {
        spellAudio.Add(CrystalType.Fire, fireAudio);
        spellAudio.Add(CrystalType.Water, waterAudio);
        spellAudio.Add(CrystalType.Earth, earthAudio);
        spellAudio.Add(CrystalType.Air, airAudio);
        spellAudio.Add(CrystalType.Void, voidAudio);
        potionAudio.Add(PotionType.Health, healthPotionAudio);
        potionAudio.Add(PotionType.Movement, movementPotionAudio);
    }
}
