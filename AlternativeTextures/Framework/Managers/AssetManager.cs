using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlternativeTextures.Framework.Managers
{
    internal class AssetManager : IAssetLoader
    {
        internal string assetFolderPath;
        private Texture2D _paintBucketTexture;
        private Texture2D _paintBrushEmptyTexture;
        private Texture2D _paintBrushFilledTexture;

        public AssetManager(IModHelper helper)
        {
            // Get the asset folder path
            assetFolderPath = helper.Content.GetActualAssetKey(Path.Combine("Framework", "Assets"), ContentSource.ModFolder);

            // Load in the assets
            _paintBucketTexture = helper.Content.Load<Texture2D>(Path.Combine(assetFolderPath, "PaintBucket.png"));
            _paintBrushEmptyTexture = helper.Content.Load<Texture2D>(Path.Combine(assetFolderPath, "PaintBrushEmpty.png"));
            _paintBrushFilledTexture = helper.Content.Load<Texture2D>(Path.Combine(assetFolderPath, "PaintBrushFilled.png"));
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            return AlternativeTextures.textureManager.GetValidTextureNames().Any(id => asset.AssetNameEquals($"{AlternativeTextures.TEXTURE_TOKEN_HEADER}{id}"));
        }

        public T Load<T>(IAssetInfo asset)
        {
            var textureModel = AlternativeTextures.textureManager.GetAllTextures().First(t => asset.AssetNameEquals($"{AlternativeTextures.TEXTURE_TOKEN_HEADER}{t.GetId()}"));
            return (T)(object)textureModel.Texture;
        }

        internal Texture2D GetPaintBucketTexture()
        {
            return _paintBucketTexture;
        }

        internal Texture2D GetPaintBrushEmptyTexture()
        {
            return _paintBrushEmptyTexture;
        }

        internal Texture2D GetPaintBrushFilledTexture()
        {
            return _paintBrushFilledTexture;
        }
    }
}
