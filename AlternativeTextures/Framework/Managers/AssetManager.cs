using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlternativeTextures.Framework.Managers
{
    internal class AssetManager : IAssetLoader
    {
        public bool CanLoad<T>(IAssetInfo asset)
        {
            return AlternativeTextures.textureManager.GetValidTextureNames().Any(id => asset.AssetNameEquals($"{AlternativeTextures.TOKEN_HEADER}{id}"));
        }

        public T Load<T>(IAssetInfo asset)
        {
            var textureModel = AlternativeTextures.textureManager.GetAllTextures().First(t => asset.AssetNameEquals($"{AlternativeTextures.TOKEN_HEADER}{t.GetId()}"));
            return (T)(object)textureModel.Texture;
        }
    }
}
