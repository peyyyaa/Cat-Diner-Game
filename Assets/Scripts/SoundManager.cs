using UnityEngine;

// enum: every sound the game can play.
public enum Sfx
{
    Arrive,     // a customer comes in
    Seat,       // a party sits down
    Order,      // the waiter takes an order
    FoodReady,  // food appears on the counter
    PickUp,     // the waiter picks up plates
    Serve,      // food is put on the table
    Coin,       // money collected
    Tip,        // there was a tip too
    Angry,      // a customer leaves unhappy
    Click,      // a UI button
    DayWon,     // goal reached at closing time
    DayLost     // goal not reached
}

// Put this on an empty object named SoundManager (one in each scene that needs sounds).
// Any script can play a sound with: SoundManager.Play(Sfx.Coin);
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Sound effects (drag the audio files in)")]
    public AudioClip Arrive;
    public AudioClip Seat;
    public AudioClip Order;
    public AudioClip FoodReady;
    public AudioClip PickUp;
    public AudioClip Serve;
    public AudioClip Coin;
    public AudioClip Tip;
    public AudioClip Angry;
    public AudioClip Click;
    public AudioClip DayWon;
    public AudioClip DayLost;

    [Header("Background music (optional, loops)")]
    public AudioClip Music;

    [Range(0f, 1f)] public float SfxVolume = 0.7f;
    [Range(0f, 1f)] public float MusicVolume = 0.35f;

    AudioSource sfxSource;
    AudioSource musicSource;

    // Keys used to save the player's volume settings on this computer.
    const string MusicKey = "CatDiner_MusicVolume";
    const string SfxKey = "CatDiner_SfxVolume";

    void Awake()
    {
        Instance = this;

        // Use the saved volumes if the player changed them in Settings before.
        MusicVolume = PlayerPrefs.GetFloat(MusicKey, MusicVolume);
        SfxVolume = PlayerPrefs.GetFloat(SfxKey, SfxVolume);

        // AudioSource = the "speaker" that plays sounds. We make two: effects and music.
        sfxSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = MusicVolume;

        if (Music != null)
        {
            musicSource.clip = Music;
            musicSource.Play();
        }
    }

    // static: call it from anywhere without needing a reference.
    public static void Play(Sfx sound)
    {
        if (Instance == null)
        {
            return;  // no SoundManager in this scene: stay silent
        }

        AudioClip clip = Instance.ClipFor(sound);
        if (clip != null)
        {
            // PlayOneShot lets several sounds overlap.
            Instance.sfxSource.PlayOneShot(clip, Instance.SfxVolume);
        }
    }

    // switch: find the audio file for each sound.
    AudioClip ClipFor(Sfx sound)
    {
        switch (sound)
        {
            case Sfx.Arrive: return Arrive;
            case Sfx.Seat: return Seat;
            case Sfx.Order: return Order;
            case Sfx.FoodReady: return FoodReady;
            case Sfx.PickUp: return PickUp;
            case Sfx.Serve: return Serve;
            case Sfx.Coin: return Coin;
            case Sfx.Tip: return Tip;
            case Sfx.Angry: return Angry;
            case Sfx.Click: return Click;
            case Sfx.DayWon: return DayWon;
            case Sfx.DayLost: return DayLost;
            default: return null;
        }
    }

    // Called by the Settings sliders. Changes the volume right away and saves it.
    public static void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat(MusicKey, volume);
        if (Instance != null)
        {
            Instance.MusicVolume = volume;
            Instance.musicSource.volume = volume;
        }
    }

    public static void SetSfxVolume(float volume)
    {
        PlayerPrefs.SetFloat(SfxKey, volume);
        if (Instance != null)
        {
            Instance.SfxVolume = volume;
        }
    }

    // The saved volumes (used to set the sliders when Settings opens).
    public static float SavedMusicVolume()
    {
        return Instance != null ? Instance.MusicVolume : PlayerPrefs.GetFloat(MusicKey, 0.35f);
    }

    public static float SavedSfxVolume()
    {
        return Instance != null ? Instance.SfxVolume : PlayerPrefs.GetFloat(SfxKey, 0.7f);
    }

    // For UI buttons: in a button's On Click (), pick SoundManager > PlayClick.
    public void PlayClick()
    {
        Play(Sfx.Click);
    }
}