using UnityEngine;

namespace RhythmGameStarter
{
    public class PlaySFX : MonoBehaviour
    {
        [Header("Sound Effect")]
        [SerializeField] private AudioClip soundEffect; // Clip assigned via Inspector

        public void PlaySound()
        {
            if (soundEffect != null)
            {
                SoundManager.Instance.PlaySFX(soundEffect);
            }
            else
            {
                Debug.LogWarning("No sound effect assigned to PlaySFX on " + gameObject.name);
            }
        }
    }
}
