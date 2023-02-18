using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework.Graphics;

namespace DanceDanceRotationModule.Util
{
    public class Resources
    {
        public static Resources Instance = new Resources();

        public Resources()
        {
        }

        public void LoadResources(ContentsManager contentsManager)
        {
            MugTexture = contentsManager.GetTexture("603447.png");
            WindowBackgroundTexture = contentsManager.GetTexture("155985.png");
        }

        public void Unload()
        {
            WindowBackgroundTexture?.Dispose();
            MugTexture?.Dispose();
        }

        public Texture2D MugTexture { get; private set; }
        public Texture2D WindowBackgroundTexture { get; private set; }
    }
}