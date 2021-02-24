#if !BACKOFFICE
using System;

namespace MetaLoop.Common.PlatformCommon.Unity.Sounds
{
    [Serializable]
    public class DGSoundInfo
    {
        public string SoundName;
        public float Volume = 1f;
        public float Delay;
    }
}
#endif