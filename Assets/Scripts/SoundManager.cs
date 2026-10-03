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

    void Awake()
    {
        Instance = this;

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
            case Sfx.Arrive:    return Arrive;
            case Sfx.Seat:      return Seat;
            case Sfx.Order:     return Order;
            case Sfx.FoodReady: return FoodReady;
            case Sfx.PickUp:    return PickUp;
            case Sfx.Serve:     return Serve;
            case Sfx.Coin:      return Coin;
            case Sfx.Tip:       return Tip;
            case Sfx.Angry:     return Angry;
            case Sfx.Click:     return Click;
            case Sfx.DayWon:    return DayWon;
            case Sfx.DayLost:   return DayLost;
            default:            return null;
        }
    }

    // For UI buttons: in a button's On Click (), pick SoundManager > PlayClick.
    public void PlayClick()
    {
        Play(Sfx.Click);
    }
}
