using UnityEngine;
using MidiPlayerTK;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RhythmGameStarter
{
    public class MidiToSongConverter
    {
        private const string SONG_ASSET_PATH = "Assets/Songs/";
        public static readonly int[] PentatonicDegrees = { 0, 2, 4, 7, 9 };
        private static MidiFileLoader midiLoader;
        private static bool verboseDebug = false;

        public static SongItem CreateSongFromMidi(string midiName, SongItem.NoteName rootNote, int selectedChannel, string instrumentName)
        {
            Debug.Log($"=== Starting MIDI Conversion Process ===");

            if (midiLoader == null)
                midiLoader = new MidiFileLoader();

            midiLoader.MPTK_MidiName = midiName;

            if (!midiLoader.MPTK_Load())
            {
                Debug.LogError($"Failed to load MIDI file: {midiName}");
                return null;
            }

            Debug.Log($"PPQ: {midiLoader.MPTK_DeltaTicksPerQuarterNote}");
            Debug.Log($"Initial Tempo: {midiLoader.MPTK_InitialTempo} BPM");

            var songItem = ScriptableObject.CreateInstance<SongItem>();
            // Set display name
            songItem.name = $"{midiName} ({songItem.instrumentName}) [{rootNote}]";

            songItem.bpm = (int)midiLoader.MPTK_InitialTempo;
            songItem.notes = new List<SongItem.MidiNote>();

            // Store MIDI reference
            songItem.midiReference = midiName;
            songItem.rootKey = rootNote;
            songItem.instrumentName = instrumentName;

            var midiEvents = midiLoader.MPTK_ReadMidiEvents();
            var channelEvents = midiEvents
                .Where(e => e.Channel == selectedChannel && e.Command == MPTKCommand.NoteOn && e.Velocity > 0)
                .OrderBy(e => e.Tick)
                .ToList();

            foreach (var midiEvent in channelEvents)
            {
                var note = new SongItem.MidiNote
                {
                    noteName = (SongItem.NoteName)(((int)(FindNearestPentatonicDegree((midiEvent.Value - (int)rootNote + 12) % 12) + rootNote)) % 12),
                    noteOctave = 3,
                    //Seconds
                    //time = midiEvent.RealTime / 1000f,
                    //noteLength = midiEvent.Duration / 1000f,
                    //Beats
                    time = midiEvent.Tick,
                    noteLength = midiEvent.Length,
                    beatIndex = midiEvent.Tick / (float)midiLoader.MPTK_DeltaTicksPerQuarterNote,
                    beatLengthIndex = midiEvent.Length / (float)midiLoader.MPTK_DeltaTicksPerQuarterNote
                };

                if (verboseDebug)
                {
                    Debug.Log($"Event: Tick={midiEvent.Tick} RealTime={midiEvent.RealTime} Length={midiEvent.Length}");
                    Debug.Log($"Created Note: time={note.time:F3} length={note.noteLength:F3} " +
                              $"beatIndex={note.beatIndex:F3} beatLengthIndex={note.beatLengthIndex:F3}");
                }

                songItem.notes.Add(note);
            }

            // Add metadata
            songItem.metadata = new SongItem.MetadataList();
            songItem.metadata.values = new List<SongItem.Metadata>
            {
                new SongItem.Metadata
                {
                    id = "difficulties",
                    intValue = 2,
                    stringValue = ""
                }
            };

//#if UNITY_EDITOR
//            if (!Application.isPlaying)
//            {
//                string assetName = $"{midiName}.{songItem.instrumentName}.{rootNote}";
//                string assetPath = $"{SONG_ASSET_PATH}{assetName}.asset";
//                try
//                {
//                    System.IO.Directory.CreateDirectory(SONG_ASSET_PATH);
//                    AssetDatabase.CreateAsset(songItem, assetPath);
//                    AssetDatabase.SaveAssets();
//                    AssetDatabase.Refresh();
//                }
//                catch (System.Exception ex)
//                {
//                    Debug.LogError($"Failed to save asset: {ex.Message}");
//                    return null;
//                }
//            }
//#endif

            return songItem;
        }

        private static int FindNearestPentatonicDegree(int semitonesFromRoot)
        {
            int nearestDegree = PentatonicDegrees[0];
            int minDistance = int.MaxValue;

            if (verboseDebug)
                Debug.Log($"Finding nearest pentatonic degree for {semitonesFromRoot} semitones:");

            foreach (int degree in PentatonicDegrees)
            {
                int distance = Mathf.Min(
                    Mathf.Abs(semitonesFromRoot - degree),
                    Mathf.Abs(semitonesFromRoot - (degree + 12))
                );

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestDegree = degree;
                }
            }

            return nearestDegree;
        }

        private static SongItem Difficulity(SongItem songItem)
        {
            //TODO: Impliment difficulty 1: downbeat only 3 lanes, 2: downbeat 5 lanes, 3: 5 lanes
            //4 beats per measure whole notes, 4: 5 lames 8 beats per measure whole quarter, 5: all notes

            return songItem;
        }
    }
}