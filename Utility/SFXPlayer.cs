using UnityEngine;

namespace Utility
{
    public class SFXPlayer
    {
        private static AudioSource _audioSource;

        private static AudioSource audioSource
        {
            get
            {
                if (_audioSource == null)
                {
                    var go = new GameObject("SFX Player");
                    _audioSource = go.AddComponent<AudioSource>();
                }

                return _audioSource;
            }
        }


        public static void PlaySound(AudioClip clip, float volumeScale = 1)
        {
            if (clip == null) return;
            audioSource.PlayOneShot(clip, volumeScale);
        }
    }
}