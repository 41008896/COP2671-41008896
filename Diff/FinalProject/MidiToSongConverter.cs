using UnityEngine;
using MidiPlayerTK;
using System.Collections.Generic;
using System.Linq;
using System;

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

        public static SongItem CreateSongFromMidi(string midiName, SongItem.NoteName rootNote, int selectedChannel, string instrumentName, int difficulty)
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
            songItem.name = $"{midiName} ({instrumentName}) [{rootNote}]";

            songItem.bpm = (int)midiLoader.MPTK_InitialTempo;
            songItem.timeSignatureNumerator = midiLoader.MPTK_TimeSigNumerator;
            songItem.timeSignatureDenominator = midiLoader.MPTK_TimeSigDenominator;
            songItem.notes = new List<SongItem.MidiNote>();
            songItem.difficulty = difficulty;

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
                    time = midiEvent.RealTime / 1000f,
                    noteLength = midiEvent.Duration / 1000f,
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

            // Shits borked for serializing
            //// Add metadata
            //songItem.metadata = new SongItem.MetadataList();
            //songItem.metadata.values = new List<SongItem.Metadata>
            //{
            //    new SongItem.Metadata
            //    {
            //        id = "difficulties",
            //        intValue = 2,
            //        stringValue = ""
            //    }
            //};

            // Do not create the serialized object, as you cant in runtime.  Has to be the JSON.
            //#if UNITY_EDITOR
            //            string assetName = $"{midiName}.{songItem.instrumentName}.{rootNote}";
            //            string assetPath = $"{SONG_ASSET_PATH}{assetName}.asset";
            //            try
            //            {
            //                System.IO.Directory.CreateDirectory(SONG_ASSET_PATH);
            //                AssetDatabase.CreateAsset(songItem, assetPath);
            //                AssetDatabase.SaveAssets();
            //                AssetDatabase.Refresh();
            //            }
            //            catch (System.Exception ex)
            //            {
            //                Debug.LogError($"Failed to save asset: {ex.Message}");
            //                return null;
            //            }
            //#endif

            // Do the diffiulty stuff
            return songItem;
            //return ProcessSongByDifficulty(songItem);
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

        // Difficulity processing
        private static SongItem ProcessSongByDifficulty(SongItem song)
        {
            List<SongItem.MidiNote> filtered = FilterNotesByDifficulty(song.notes, song.difficulty - 1);
            song.notes = MapNotesToLanes(filtered, song.difficulty);
            return song;
        }

        private static List<SongItem.MidiNote> FilterNotesByDifficulty(List<SongItem.MidiNote> notes, int difficulty)
        {
            float toleranceWindow = 0.1f; // 10% of a beat
            return notes.Where(note =>
            {
                float posInBeat = note.beatIndex % 1;
                float distanceFromBeat = Math.Min(posInBeat, 1 - posInBeat);

                return difficulty switch
                {
                    1 => note.beatIndex % 4 < toleranceWindow, // Only downbeats
                    2 or 3 => distanceFromBeat < toleranceWindow, // Quarter notes
                    _ => true // Difficulties 4,5 keep all notes
                };
            }).ToList();
        }

        private static List<SongItem.MidiNote> MapNotesToLanes(List<SongItem.MidiNote> notes, int difficulty)
        {
            if (difficulty >= 4) return notes;

            var mapped = new List<SongItem.MidiNote>();
            Dictionary<float, HashSet<int>> notesAtBeat = new Dictionary<float, HashSet<int>>();
            System.Random rnd = new System.Random();

            foreach (var note in notes)
            {
                if (!notesAtBeat.ContainsKey(note.beatIndex))
                    notesAtBeat[note.beatIndex] = new HashSet<int>();

                int sourceLane = (int)note.noteName % 6;
                int targetLane;

                if (difficulty <= 2)
                    targetLane = sourceLane <= 1 ? 0 : sourceLane <= 3 ? 1 : 2;
                else // difficulty 3
                {
                    if (sourceLane == 1 || sourceLane == 3)
                        targetLane = rnd.NextDouble() < 0.67 ? (sourceLane == 1 ? 0 : 1) : (sourceLane == 1 ? 1 : 2);
                    else
                        targetLane = sourceLane <= 1 ? 0 : sourceLane <= 3 ? 1 : 2;
                }

                if (!notesAtBeat[note.beatIndex].Contains(targetLane))
                {
                    var newNote = new SongItem.MidiNote(note) { noteName = (SongItem.NoteName)targetLane };
                    notesAtBeat[note.beatIndex].Add(targetLane);
                    mapped.Add(newNote);
                }
            } // YOU ARENT ADDINT TO LANES, YOU ARE DELETING THEM.
            return mapped;
        }
    }
}