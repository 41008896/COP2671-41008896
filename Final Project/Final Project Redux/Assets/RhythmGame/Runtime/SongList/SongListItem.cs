using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RhythmGameStarter
{
    public class SongListItem : MonoBehaviour
    {
        [NonSerialized]
        public SongItem targetSongItem;
        public TextMeshProUGUI label;
        public TextMeshProUGUI authorLabel;
        public TextMeshProUGUI indexLabel;
        public Button button;
        public Image difficultiesFill;
        public Image coverArtImage;
        [Tooltip("Button to trigger item removal")]
        public Button removeButton; // Assigned in the editor
        [CollapsedEvent]
        public UnityEvent onItemSetup;
        [CollapsedEvent]
        [Tooltip("Triggered when the remove button is clicked")]
        public UnityEvent<SongItem> onItemDelete; // Event triggered for removal

        public void TriggerDeleteEvent()
        {
            onItemDelete?.Invoke(targetSongItem); // Wrapper method to call the UnityEvent
        }
    }
}