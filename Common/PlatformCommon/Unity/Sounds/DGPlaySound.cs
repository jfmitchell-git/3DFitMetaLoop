#if !BACKOFFICE

using System;
using UnityEngine;

namespace MetaLoop.Common.PlatformCommon.Unity.Sounds
{
    public class DGPlaySound : MonoBehaviour
    {

        public String SoundName;
        public AudioClip Sound;

        public void Awake()
        {
            if(Sound)
            {
                SoundManager.getInstance().PlaySoundFx(Sound);
            } else
            {
                SoundManager.getInstance().PlaySoundByName(SoundName);
            }
           
        }

    }
}
#endif