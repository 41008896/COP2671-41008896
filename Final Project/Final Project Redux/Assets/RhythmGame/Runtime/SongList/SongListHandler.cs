using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RhythmGameStarter
{
    public class SongListHandler : MonoBehaviour
    {
        // [Comment("Handling note recording target switching")]
        public Transform container;

        [Tooltip("Must contain Button & TextMeshProUGUI component")]
        public GameObject itemPrefab;

        [ReorderableDisplay("Item")]
        public SongItemList songItems;

        [Comment("Events")]
        [CollapsedEvent]
        public SongItemEvent onItemSelect;

        [Serializable] public class SongItemList : ReorderableList<SongItem> { }

#if UNITY_EDITOR
        private readonly string savePath = Path.Combine(Application.dataPath, "Songs", "songlist.json");
#else
        private readonly string savePath = Path.Combine(Application.persistentDataPath, "songlist.json");
#endif

        private void Start()
        {
            LoadSongListFromFile();   // Load the file after ensuring it exists
            RefreshUI();              // Refresh the UI with loaded data
        }

        public void RefreshUI()
        {
            foreach (Transform child in container)
                GameObject.Destroy(child.gameObject);

            int i = 0;
            foreach (var target in songItems)
            {
                if (!target) continue;

                i++;

                var itemName = target.name;

                var item = Instantiate(itemPrefab, container);
                item.transform.localScale = Vector3.one;
                item.name = itemName;

                var songListItem = item.GetComponent<SongListItem>();

                songListItem.targetSongItem = target;
                songListItem.label.text = target.name;
                songListItem.authorLabel.text = target.author;
                songListItem.indexLabel.text = i.ToString();
                if (songListItem.coverArtImage)
                    songListItem.coverArtImage.sprite = target.coverArt;
                songListItem.difficultiesFill.fillAmount = target.difficulty / 4f; // 5 levels (0-4)
                if (target.difficulty == 0)
                {
                    songListItem.difficultiesFill.gameObject.SetActive(false);
                }
                songListItem.button.onClick.AddListener(() =>
                {
                    onItemSelect.Invoke(target);
                });
                // Subscribe to the delete event
                songListItem.onItemDelete.AddListener(RemoveSong);

                songListItem.onItemSetup.Invoke();
            }
        }

        public void AddSong(SongItem newSong)
        {
            songItems.values.Add(newSong);
            // Refresh UI to reflect changes
            RefreshUI();
            // Save the updated list to the JSON file
            SaveSongListToFile();
        }

        public void RemoveSong(SongItem songToRemove)
        {
            songItems.values.Remove(songToRemove);
            // Refresh UI to reflect changes
            RefreshUI();
            // Save the updated list to the JSON file
            SaveSongListToFile();
        }


        private void LoadSongListFromFile()
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(savePath);

                // Load all song files matching song_*.json
                var songFiles = Directory.GetFiles(directoryPath, "song_*.json");
                var loadedSongs = new List<SongItem>();

                foreach (var filePath in songFiles)
                {
                    try
                    {
                        string json = File.ReadAllText(filePath);
                        var serializedSong = JsonUtility.FromJson<SerializableSongItem>(json);

                        if (serializedSong != null)
                        {
                            loadedSongs.Add(serializedSong.ToSongItem());
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to load song file {filePath}: {ex}");
                    }
                }

                songItems.values = loadedSongs;
                Debug.Log($"Loaded {loadedSongs.Count} songs from {directoryPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading song list: {ex}");
                songItems.values = new List<SongItem>();
            }
        }

        private void SaveSongListToFile()
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(savePath);

                // Clear existing song files to avoid stale data
                var existingFiles = Directory.GetFiles(directoryPath, "song_*.json");
                foreach (var file in existingFiles)
                {
                    File.Delete(file);
                }

                // Save each song individually
                for (int i = 0; i < songItems.values.Count; i++)
                {
                    var song = songItems.values[i];
                    if (song == null) continue;

                    var serializedSong = new SerializableSongItem(song);
                    string json = JsonUtility.ToJson(serializedSong, true);

                    string filePath = Path.Combine(directoryPath, $"song_{i}.json");
                    File.WriteAllText(filePath, json);
                    Debug.Log($"Saved song: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving songs: {ex}");
            }
        }
    }
}