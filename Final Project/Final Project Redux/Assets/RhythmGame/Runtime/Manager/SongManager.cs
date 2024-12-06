using UnityEngine;
using MidiPlayerTK;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using PlasticGui.WorkspaceWindow.Locks;

namespace RhythmGameStarter
{
    [HelpURL("https://bennykok.gitbook.io/rhythm-game-starter/hierarchy-overview/song-manager")]
    [RequireComponent(typeof(TrackManager))]
    public class SongManager : MonoBehaviour
    {
        [Comment("Responsible for song control and MIDI playback events")]
        private MidiFilePlayer midiFilePlayer;

        [Title("Properties", 0)]
        [Space]
        [Tooltip("Start playing the default song when scene loads")]
        public bool playOnAwake = true;

        [Tooltip("The default song to play if none is specified")]
        public SongItem defaultSong;

        [Tooltip("If true, song will restart when finished")]
        public bool looping;

        [Title("Display", 0)]
        [Tooltip("Show progress as percentage instead of time")]
        public bool progressAsPercentage = true;

        [Tooltip("Invert the fill amount (1 - progress)")]
        public bool inverseProgressFill = false;

        // Hidden properties used by track system
        [HideInInspector] public float secPerBeat;    // Duration of one beat in seconds
        [HideInInspector] public float songPosition;   // Current playback position in seconds
        [HideInInspector] public IEnumerable<SongItem.MidiNote> currnetNotes; // All notes for current song

        [Title("Events", 0)]
        [CollapsedEvent("Triggered every frame when song is playing, passes current time in seconds")]
        public FloatEvent onSongProgress;

        [CollapsedEvent("Triggered every frame, passes normalized progress value [0,1] for UI fill")]
        public FloatEvent onSongProgressFill;

        [CollapsedEvent("Triggered every frame, provides formatted time/percentage string")]
        public StringEvent onSongProgressDisplay;

        [CollapsedEvent("Triggered when MIDI playback begins")]
        public UnityEvent onSongStart;

        [CollapsedEvent("Triggered immediately when Play is called, before playback")]
        public UnityEvent onSongStartPlay;

        [CollapsedEvent("Triggered when song finishes or is stopped")]
        public UnityEvent onSongFinished;

        // Runtime state tracking
        [NonSerialized] public bool songPaused;        // Current pause state
        [NonSerialized] public bool songHasStarted;    // Indicates if a song is currently active
        [NonSerialized] public SongItem currentSongItem; // Currently playing song
        [NonSerialized] public ComboSystem comboSystem; // Reference to the combo system
        [NonSerialized] public TrackManager trackManager; // Reference to track management

        public AudioClip metronomeBeat;
        private AudioSource metronomeSource;
        [NonSerialized] public float delay;
        public int delayInMeasures = 2;
        [NonSerialized] public bool isDelaying = false;
        private int timeSignatureNumerator = 4;
        private float delayStartTime;

        private void Awake()
        {
            Debug.Log("SongManager: Initializing");
            trackManager = GetComponent<TrackManager>();
            comboSystem = GetComponent<ComboSystem>();

            midiFilePlayer = GameObject.FindWithTag("MidiFilePlayer")?.GetComponent<MidiFilePlayer>();
            if (midiFilePlayer == null)
            {
                Debug.LogError("SongManager: MidiFilePlayer not found in the scene!");
                return;
            }

            metronomeSource = gameObject.AddComponent<AudioSource>();
            metronomeSource.clip = metronomeBeat;
            metronomeSource.playOnAwake = false;
            metronomeSource.volume = 1.0f; // Set volume (0.0 to 1.0)
            metronomeSource.loop = false; // Set looping if needed

            songPaused = false;
            isDelaying = false;  // Ensure delay logic doesn't trigger on load
            songHasStarted = false;
            midiFilePlayer.MPTK_PlayOnStart = false;
            trackManager.Init(this);
        }

        private void Start()
        {
            if (playOnAwake && defaultSong)
            {
                PlaySong(defaultSong);
            }
        }

        void Update()
        {
            if (!songPaused && (midiFilePlayer.MPTK_IsPlaying || isDelaying))
            {
                // Get current position in seconds from MIDI player
                songPosition = (float)midiFilePlayer.MPTK_Position / 1000f;

                // Update track positions and trigger progress events
                trackManager.UpdateTrack(songPosition, secPerBeat);
                onSongProgress.Invoke(songPosition);

                // Calculate and send normalized progress for UI
                float normalizedProgress = songPosition / (midiFilePlayer.MPTK_DurationMS / 1000f);
                onSongProgressFill.Invoke(inverseProgressFill ? 1 - normalizedProgress : normalizedProgress);

                // Update progress display
                if (songPosition >= 0)
                {
                    if (progressAsPercentage)
                        onSongProgressDisplay.Invoke(Math.Truncate(normalizedProgress * 100) + "%");
                    else
                    {
                        var now = new DateTime((long)songPosition * TimeSpan.TicksPerSecond);
                        onSongProgressDisplay.Invoke(now.ToString("mm:ss"));
                    }
                }
            }

            // Only check for completion after delay is done
            if (!songPaused && !isDelaying && songHasStarted && !midiFilePlayer.MPTK_IsPlaying)
            {
                onSongFinished.Invoke();
                trackManager.ClearAllTracks();

                if (looping && currentSongItem != null)
                    PlaySong(currentSongItem);
            }

        //    // Handle metronome playback during delay
        //    if (isDelaying)
        //    {
        //        if (Time.realtimeSinceStartup >= nextBeatTime)
        //        {
        //            metronomeSource.Play();
        //            nextBeatTime += secPerBeat;

        //            // End delay when time exceeds the delay threshold
        //            if (nextBeatTime >= delay)
        //                isDelaying = false;
        //        }
        //    }
        }

        public void PlaySong()
        {
            if (defaultSong)
                PlaySong(defaultSong);
            else
                Debug.LogWarning("Default song is not set!");
        }

        public void PlaySongSelected(SongItem songItem)
        {
            PlaySong(songItem);
        }

        public void SetDefaultSong(SongItem songItem)
        {
            defaultSong = songItem;
        }

        public void PlaySong(SongItem songItem)
        {
            songPaused = false;
            isDelaying = true;
            songHasStarted = false;  // Will be set true after delay
            songPosition = 0;
            Debug.Log($"SongManager: Playing song {songItem.name}");
            currentSongItem = songItem;
            secPerBeat = 60.0f / songItem.bpm;
            if (songItem.timeSignatureNumerator != 0)
                timeSignatureNumerator = songItem.timeSignatureNumerator;
            delay = delayInMeasures * timeSignatureNumerator * secPerBeat;
            Debug.Log($"Delay time in seconds: {delay} {secPerBeat} {songItem.timeSignatureNumerator}");

            midiFilePlayer.MPTK_Stop();
            midiFilePlayer.MPTK_MidiName = songItem.midiReference;
            midiFilePlayer.MPTK_Speed = songItem.speedModifier;
            
            // Preload the MIDI File
            midiFilePlayer.MPTK_Load();

            currnetNotes = songItem.GetNotes();
            if (currnetNotes == null || !currnetNotes.Any())
            {
                Debug.LogError($"SongManager: No notes found for song {songItem.name}.");
                return;
            }

            trackManager.SetupForNewSong();  // Only this setup call needed

            songPaused = false;
            isDelaying = true;
            onSongStartPlay.Invoke();

            StartCoroutine(PlayWithDelay());
        }

        private IEnumerator PlayWithDelay()
        {
            delayStartTime = Time.time;
            for (int i = 0; i < delayInMeasures * timeSignatureNumerator; i++)
            {
                metronomeSource.Play();
                yield return new WaitForSeconds(secPerBeat);
            }
            songHasStarted = true;
            midiFilePlayer.MPTK_Play();
            isDelaying = false;
        }


        public void PauseSong()
        {
            if (!songPaused)
            {
                songPaused = true;
                midiFilePlayer.MPTK_Pause();
            }
        }

        public void ResumeSong()
        {
            if (isDelaying)
            {
                isDelaying = false; // Ensure delay is canceled if resuming prematurely
            }
            if (!midiFilePlayer.MPTK_IsPlaying)
            {
                PlaySong();
                return;
            }
            if (!songPaused) return;

            songPaused = false;
            midiFilePlayer.MPTK_UnPause();
        }

        public void StopSong(bool dontInvokeEvent = false)
        {
            midiFilePlayer.MPTK_Stop();
            isDelaying = false;
            songPaused = false;
            songHasStarted = false;
            songPosition = 0;  // Reset position
            currentSongItem = null;  // Clear current song reference

            if (!dontInvokeEvent)
                onSongFinished.Invoke();

            trackManager.ClearAllTracks();
        }
    
        
    }
}