#if !BACKOFFICE
using UnityEngine;

namespace MetaLoop.Common.PlatformCommon.GameServices
{

    public enum Status
    {
        Available,
        Busy,
        Offline
    }

    public class GameServiceUserInfo
    {

        public delegate void AvatarReadyEventHandler();
        public event AvatarReadyEventHandler OnAvatarReady;

        public string PlayerId { get; set; }
        public string NickName { get; set; }
        public Status Status { get; set; }
        public Sprite Avatar { get; set; }

        public void SetAvatarFromTexture2D(Texture2D texture)
        {
            if (texture != null)
            {
                Avatar = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                if (OnAvatarReady != null) OnAvatarReady();
            }
        }


        public void SetAvatarFromSprite(Sprite sprite)
        {
            this.Avatar = sprite;
            if (OnAvatarReady != null) OnAvatarReady();
        }


    }
}
#endif