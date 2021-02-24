#if !BACKOFFICE
using UnityEngine;

namespace MetaLoop.Common.PlatformCommon.Unity.Sounds
{
    [System.Serializable]
    public class DGSoundClip
    {
        public string Name;
        public AudioClip Audio;
        public DGSoundType Type = DGSoundType.Sound_Fx;
        public float Volume = 1f;
        public bool Loop = false;

        [HideInInspector]
        public float currentPosition = 0f;

        [HideInInspector]
        public AudioSource CurrentSource;

    }
}
#endif