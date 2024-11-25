using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MidiPlayerTK;
using System.IO;
using static RhythmGameStarter.SongItem;

namespace RhythmGameStarter
{
    public class MidiPlaybackManager : MonoBehaviour
    {
        public MidiLoad midiLoad;
        public SongManager songManager;

        private int[] pentatonicScale = { 0, 2, 4, 7, 9 }; // Pentatonic scale in chromatic offsets
        private const int RootOctave = 3; // Fixed octave

        // Create a SongItem from a MIDI file with selected track and pentatonic mapping
        public SongItem CreateSongItemFromMidi(string midiFilePath, int trackIndex)
        {
            // Read the MIDI file as a byte array
            byte[] midiBytes;
            try
            {
                midiBytes = File.ReadAllBytes(midiFilePath);
            }
            catch (IOException e)
            {
                Debug.LogError($"Failed to load MIDI file: {e.Message}");
                return null;
            }

            // Load the MIDI data using MidiLoad
            if (!midiLoad.MPTK_Load(midiBytes))
            {
                Debug.LogError($"Failed to load MIDI events from file: {midiFilePath}");
                return null;
            }

            // Create SongItem and populate its notes based on MIDI events
            SongItem songItem = ScriptableObject.CreateInstance<SongItem>();
            songItem.bpm = (int)midiLoad.MPTK_InitialTempo;
            songItem.notes = new List<SongItem.MidiNote>();

            foreach (MPTKEvent midiEvent in midiLoad.MPTK_MidiEvents)
            {
                if (midiEvent.Track == trackIndex && midiEvent.Command == MPTKCommand.NoteOn)
                {
                    int pentatonicNoteValue = MapToPentatonicScale(midiEvent.Value);
                    float beatIndex = CalculateBeatIndex(midiEvent.RealTime, songItem.bpm);
                    float beatLengthIndex = CalculateBeatLengthIndex(midiEvent.Duration, songItem.bpm);

                    var note = new SongItem.MidiNote
                    {
                        noteName = (NoteName)pentatonicNoteValue,
                        noteOctave = RootOctave,
                        time = midiEvent.RealTime,
                        noteLength = midiEvent.Duration,
                        beatIndex = beatIndex,
                        beatLengthIndex = beatLengthIndex
                    };

                    songItem.notes.Add(note);
                }
            }

#if UNITY_EDITOR
            string assetPath = $"Assets/Songs/{Path.GetFileNameWithoutExtension(midiFilePath)}_Track{trackIndex}_SongItem.asset";
            AssetDatabase.CreateAsset(songItem, assetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"SongItem saved at: {assetPath}");
#endif

            return songItem;
        }

        private int MapToPentatonicScale(int midiNote)
        {
            int baseNote = midiNote % 12;
            int closestNote = pentatonicScale[0];
            int minDistance = Mathf.Abs(baseNote - pentatonicScale[0]);

            foreach (int scaleNote in pentatonicScale)
            {
                int distance = Mathf.Abs(baseNote - scaleNote);
                if (distance < minDistance)
                {
                    closestNote = scaleNote;
                    minDistance = distance;
                }
            }

            return closestNote;
        }

        private float CalculateBeatIndex(float time, float bpm)
        {
            float secondsPerBeat = 60f / bpm;
            return time / secondsPerBeat;
        }

        private float CalculateBeatLengthIndex(float duration, float bpm)
        {
            float secondsPerBeat = 60f / bpm;
            return duration / secondsPerBeat;
        }
    }
}
