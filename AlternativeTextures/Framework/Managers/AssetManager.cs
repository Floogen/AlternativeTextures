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
        internal Dictionary<string, string> idToAssetToken;

        public AssetManager()
        {
            idToAssetToken = new Dictionary<string, string>();
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            //return AlternativeTextures.textureManager.GetValidTextureNames().Any(i => asset.AssetNameEquals($"{AlternativeTextures.TOKEN_HEADER}{i}"));
            return idToAssetToken.Keys.Any(i => asset.AssetNameEquals($"{AlternativeTextures.TOKEN_HEADER}{i}"));
        }

        public T Load<T>(IAssetInfo asset)
        {
            var textureModel = AlternativeTextures.textureManager.GetAllTextures().First(t => asset.AssetNameEquals($"{AlternativeTextures.TOKEN_HEADER}{t.GetId()}"));
            return (T)(object)textureModel.Texture;
        }
    }
}
