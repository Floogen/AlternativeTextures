using Microsoft.Xna.Framework.Graphics;
using System;

namespace AlternativeTextures.Framework.Interfaces
{
    public interface IJsonAssetsApi
    {
        int GetBigCraftableId(string name);
        int GetObjectId(string name);
        bool TryGetGiantCropSprite(int productID, out Lazy<Texture2D> texture);


        event EventHandler IdsAssigned;
    }
}
