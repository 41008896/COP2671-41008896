using UnityEngine;
using UnityEditor;
using MidiPlayerTK;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace RhythmGameStarter
{
    public class MidiTestCreatorWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private string[] midiFiles;
        private int selectedMidiIndex = -1;
        private int selectedChannel = -1; // This will be our channel selection
        private int selectedKeyIndex = 0;
        private SongItem.NoteName selectedRootNote = SongItem.NoteName.C; // Added
        private string authorName = "Test Author";
        private int difficulty = 2;
        private MidiLoad previewMidiLoad;
        private Dictionary<int, int> channelInstruments = new Dictionary<int, int>();

        // Soundfont standard instruments
        private readonly string[] instrumentNames = new string[]
        {
            "Acoustic Grand Piano", "Bright Acoustic Piano", "Electric Grand Piano", "Honky-tonk Piano",
            "Electric Piano 1", "Electric Piano 2", "Harpsichord", "Clavinet",
            "Celesta", "Glockenspiel", "Music Box", "Vibraphone",
            "Marimba", "Xylophone", "Tubular Bells", "Dulcimer",
            "Drawbar Organ", "Percussive Organ", "Rock Organ", "Church Organ",
            "Reed Organ", "Accordion", "Harmonica", "Tango Accordion",
            "Acoustic Guitar (nylon)", "Acoustic Guitar (steel)", "Electric Guitar (jazz)", "Electric Guitar (clean)",
            "Electric Guitar (muted)", "Overdriven Guitar", "Distortion Guitar", "Guitar Harmonics",
            "Acoustic Bass", "Electric Bass (finger)", "Electric Bass (pick)", "Fretless Bass",
            "Slap Bass 1", "Slap Bass 2", "Synth Bass 1", "Synth Bass 2",
            "Violin", "Viola", "Cello", "Contrabass",
            "Tremolo Strings", "Pizzicato Strings", "Orchestral Harp", "Timpani",
            "String Ensemble 1", "String Ensemble 2", "Synth Strings 1", "Synth Strings 2",
            "Choir Aahs", "Voice Oohs", "Synth Choir", "Orchestra Hit",
            "Trumpet", "Trombone", "Tuba", "Muted Trumpet",
            "French Horn", "Brass Section", "Synth Brass 1", "Synth Brass 2",
            "Soprano Sax", "Alto Sax", "Tenor Sax", "Baritone Sax",
            "Oboe", "English Horn", "Bassoon", "Clarinet",
            "Piccolo", "Flute", "Recorder", "Pan Flute",
            "Blown Bottle", "Shakuhachi", "Whistle", "Ocarina",
            "Lead 1 (square)", "Lead 2 (sawtooth)", "Lead 3 (calliope)", "Lead 4 (chiff)",
            "Lead 5 (charang)", "Lead 6 (voice)", "Lead 7 (fifths)", "Lead 8 (bass + lead)",
            "Pad 1 (new age)", "Pad 2 (warm)", "Pad 3 (polysynth)", "Pad 4 (choir)",
            "Pad 5 (bowed)", "Pad 6 (metallic)", "Pad 7 (halo)", "Pad 8 (sweep)",
            "FX 1 (rain)", "FX 2 (soundtrack)", "FX 3 (crystal)", "FX 4 (atmosphere)",
            "FX 5 (brightness)", "FX 6 (goblins)", "FX 7 (echoes)", "FX 8 (sci-fi)",
            "Sitar", "Banjo", "Shamisen", "Koto",
            "Kalimba", "Bagpipe", "Fiddle", "Shanai",
            "Tinkle Bell", "Agogo", "Steel Drums", "Woodblock",
            "Taiko Drum", "Melodic Tom", "Synth Drum", "Reverse Cymbal",
            "Guitar Fret Noise", "Breath Noise", "Seashore", "Bird Tweet",
            "Telephone Ring", "Helicopter", "Applause", "Gunshot"
        };

        // Key names array
        private readonly string[] keyNames = new string[]
        {
        "C Major", "G Major", "D Major", "A Major", "E Major", "B Major", "F# Major",
        "C# Major", "F Major", "Bb Major", "Eb Major", "Ab Major", "Db Major", "Gb Major", "Cb Major",
        "A Minor", "E Minor", "B Minor", "F# Minor", "C# Minor", "G# Minor", "D# Minor",
        "A# Minor", "D Minor", "G Minor", "C Minor", "F Minor", "Bb Minor", "Eb Minor", "Ab Minor"
        };

        private int GetKeyIndex(int sharpsFlats, bool isMajor)
        {
            // Convert MIDI key signature (-7 to +7 sharps/flats) to array index
            int baseIndex = sharpsFlats + 7; // 7 is the index for C major/A minor
            if (isMajor)
                return baseIndex;
            else
                return baseIndex + 15; // Offset for minor keys
        }

        [MenuItem("Tools/MPTK/MIDI Test Creator")]
        public static void ShowWindow()
        {
            var window = GetWindow<MidiTestCreatorWindow>("MIDI Test Creator");
            window.minSize = new Vector2(400, 600);
            // Create singleton if needed
            if (MidiPlayerGlobal.Instance == null)
            {
                var go = new GameObject("MidiPlayerGlobal");
                var mpg = go.AddComponent<MidiPlayerGlobal>();
                mpg.InitInstance();
            }
            window.RefreshMidiList();
        }

        private void RefreshMidiList()
        {
            // Get direct reference to MPTK MIDI files
            if (MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null)
            {
                midiFiles = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.ToArray();
                Debug.Log($"MPTK Database Loaded: {midiFiles.Length} MIDI files found.");
            }
            else
            {
                Debug.LogWarning("MPTK Database could not be loaded or contains no MIDI files.");
                midiFiles = new string[0];
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("MIDI Test Song Creator", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if (GUILayout.Button("Refresh MIDI List"))
            {
                RefreshMidiList();
            }

            EditorGUILayout.Space(10);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // MIDI File Selection
            EditorGUILayout.LabelField("1. Select MIDI File", EditorStyles.boldLabel);
            int newSelectedIndex = EditorGUILayout.Popup("MIDI File", selectedMidiIndex, midiFiles);
            if (newSelectedIndex != selectedMidiIndex)
            {
                selectedMidiIndex = newSelectedIndex;
                if (selectedMidiIndex >= 0)
                {
                    LoadPreviewMidi(midiFiles[selectedMidiIndex]);
                }
            }

            EditorGUILayout.Space(10);

            // MIDI Information
            EditorGUILayout.LabelField("2. MIDI Information", EditorStyles.boldLabel);
            if (previewMidiLoad != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"Duration: {previewMidiLoad.MPTK_Duration.ToString(@"mm\:ss")}");
                EditorGUILayout.LabelField($"Tempo: {previewMidiLoad.MPTK_InitialTempo} BPM");
                EditorGUILayout.LabelField($"Time Signature: {previewMidiLoad.MPTK_TimeSigNumerator}/{previewMidiLoad.MPTK_TimeSigDenominator}");

                // Key Signature display
                string keyName = GetKeySignatureName(
                    previewMidiLoad.MPTK_KeySigSharpsFlats,
                    previewMidiLoad.MPTK_KeySigMajorMinor == 0 ? "Major" : "Minor"
                );
                EditorGUILayout.LabelField($"Key Signature: {keyName}");
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a MIDI file to view information", MessageType.Info);
            }

            EditorGUILayout.Space(10);

            // Channel Selection with Instruments
            EditorGUILayout.LabelField("3. Select Channel", EditorStyles.boldLabel);
            if (channelInstruments != null && channelInstruments.Count > 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                foreach (var channel in channelInstruments)
                {
                    string instrumentName = GetInstrumentName(channel.Value);
                    int noteCount = GetChannelNoteCount(channel.Key);
                    if (noteCount > 0) // Only show channels with notes
                    {
                        bool isSelected = selectedChannel == channel.Key;
                        bool newIsSelected = EditorGUILayout.ToggleLeft(
                            $"Channel {channel.Key}: {instrumentName} ({noteCount} notes)",
                            isSelected
                        );
                        if (newIsSelected != isSelected)
                        {
                            selectedChannel = newIsSelected ? channel.Key : -1;
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(10);

            // Song Properties
            EditorGUILayout.LabelField("4. Song Properties", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Key selection (defaulting to actual key but not affecting conversion yet)
            if (previewMidiLoad != null)
            {
                int defaultKeyIndex = GetKeyIndex(
                    previewMidiLoad.MPTK_KeySigSharpsFlats,
                    previewMidiLoad.MPTK_KeySigMajorMinor == 0
                );
                selectedKeyIndex = EditorGUILayout.Popup("Key", defaultKeyIndex, keyNames);
            }

            authorName = EditorGUILayout.TextField("Author", authorName);
            difficulty = EditorGUILayout.IntSlider("Difficulty", difficulty, 0, 3);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(20);

            // Create button
            GUI.enabled = selectedMidiIndex >= 0 && selectedChannel >= 0;
            if (GUILayout.Button("Create Test Song"))
            {
                CreateTestSong();
            }
            GUI.enabled = true;

            EditorGUILayout.EndScrollView();
        }

        private void LoadPreviewMidi(string midiName)
        {
            var loader = new MidiFileLoader();
            loader.MPTK_MidiName = midiName;

            if (loader.MPTK_Load())
            {
                previewMidiLoad = loader.MPTK_MidiLoaded;
                channelInstruments.Clear();

                // Get all program changes to map channels to instruments
                foreach (var midiEvent in loader.MPTK_ReadMidiEvents())
                {
                    if (midiEvent.Command == MPTKCommand.PatchChange)
                    {
                        channelInstruments[midiEvent.Channel] = midiEvent.Value;
                    }
                }

                Debug.Log($"Loaded MIDI with {channelInstruments.Count} active channels");
            }
        }

        private int GetChannelNoteCount(int channel)
        {
            if (previewMidiLoad == null) return 0;
            return previewMidiLoad.MPTK_MidiEvents.Count(evt =>
                evt.Channel == channel &&
                evt.Command == MPTKCommand.NoteOn &&
                evt.Velocity > 0);
        }

        private string GetInstrumentName(int programNumber)
        {
            // Special case for channel 9 (drums)
            if (programNumber == 9)
                return "Drums";

            // Standard instruments (0-127)
            if (programNumber >= 0 && programNumber < instrumentNames.Length)
                return instrumentNames[programNumber];

            return $"Unknown Instrument ({programNumber})";
        }

        private string GetKeySignatureName(int sharpsFlats, string majorMinor)
        {
            string[] keyNames = {
        "Cb", "Gb", "Db", "Ab", "Eb", "Bb", "F", "C", "G", "D", "A", "E", "B", "F#", "C#"};
            int index = sharpsFlats + 7; // 7 is the index for C
            if (index >= 0 && index < keyNames.Length)
            {
                return $"{keyNames[index]} {majorMinor}";
            }
            return "Unknown Key";
        }

        private void CreateTestSong()
        {
            if (selectedMidiIndex >= 0)
            {
                // Pass exact MIDI name from MPTK database
                string midiName = midiFiles[selectedMidiIndex];
                string instrumentName = GetInstrumentName(channelInstruments[selectedChannel]);
                var songItem = MidiToSongConverter.CreateSongFromMidi(
                            midiName,
                            selectedRootNote,
                            selectedChannel,
                            instrumentName
                        );

                if (songItem != null)
                {
                    // Update author and difficulty metadata using the format expected
                    songItem.author = authorName;
                    if (songItem.metadata == null)
                        songItem.metadata = new SongItem.MetadataList();

                    var difficultyMetadata = songItem.metadata.values.Find(x => x.id == "difficulties");
                    if (difficultyMetadata == null)
                    {
                        difficultyMetadata = new SongItem.Metadata
                        {
                            id = "difficulties",
                            stringValue = ""
                        };
                        songItem.metadata.values.Add(difficultyMetadata);
                    }
                    difficultyMetadata.intValue = difficulty;

                    EditorUtility.SetDirty(songItem);
                    AssetDatabase.SaveAssets();
                    Debug.Log($"Successfully created test song: {songItem.name}");
                }
            }
        }

        private void OnDestroy()
        {
            if (MidiPlayerGlobal.Instance != null)
            {
                DestroyImmediate(MidiPlayerGlobal.Instance.gameObject);
                Debug.Log("MidiPlayerGlobal object destroyed.");
            }
        }
    }
}