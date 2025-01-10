using Microsoft.Xna.Framework.Graphics;

namespace AlternativeTextures.Framework.Interfaces
{
    public interface IMoreGiantCropsApi
    {
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        Texture2D? GetTexture(int productIndex);
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    }
}
