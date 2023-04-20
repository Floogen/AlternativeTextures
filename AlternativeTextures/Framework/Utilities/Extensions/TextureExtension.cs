using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlternativeTextures.Framework.Utilities.Extensions
{
    public static class TextureExtension
    {
        public static Texture2D CreateSelectiveCopy(this Texture2D sourceTexture, GraphicsDevice device, Rectangle selectionRect)
        {
            Texture2D selectiveTexture = new Texture2D(device, selectionRect.Width, selectionRect.Height);
            try
            {

                int dimensions = selectionRect.Width * selectionRect.Height;
                Color[] data = new Color[dimensions];

                sourceTexture.GetData(0, selectionRect, data, 0, dimensions);
                selectiveTexture.SetData(data);
            }
            catch
            {
                AlternativeTextures.monitor.Log("Issue creating a selective texture copy, likely out of bounds for dimensions.", StardewModdingAPI.LogLevel.Trace);
            }

            return selectiveTexture;
        }
    }
}
