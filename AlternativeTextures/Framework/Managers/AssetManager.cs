using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System.Collections.Generic;
using System.IO;

namespace AlternativeTextures.Framework.Managers
{
    internal class AssetManager
    {
        internal string assetFolderPath;
        internal Dictionary<string, Texture2D> toolNames = new Dictionary<string, Texture2D>();

        private IModHelper _helper;

        private Texture2D _paintBucketTexture;
        private Texture2D _scissorsTexture;
        private Texture2D _sprayCanTexture;
        private Texture2D _sprayCanTextureRare;
        private Texture2D _paintBrushEmptyTexture;
        private Texture2D _paintBrushFilledTexture;
        private Texture2D _catalogueTexture;

        public AssetManager(IModHelper helper)
        {
            _helper = helper;

            // Get the asset folder path
            assetFolderPath = helper.ModContent.GetInternalAssetName(Path.Combine("Framework", "Assets")).Name;

            // Load in the assets
            _paintBucketTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "PaintBucket.png"));
            _scissorsTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "Scissors.png"));
            _sprayCanTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "SprayCan.png"));
            _sprayCanTextureRare = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "SprayCanRare.png"));
            _paintBrushEmptyTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "PaintBrushEmpty.png"));
            _paintBrushFilledTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "PaintBrushFilled.png"));
            _catalogueTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "Catalogue.png"));

            // Setup toolNames
            toolNames.Add("PaintBucket", _paintBucketTexture);
            toolNames.Add("Scissors", _scissorsTexture);
            toolNames.Add("SprayCan", _sprayCanTexture);
            toolNames.Add("Catalogue", _catalogueTexture);
            toolNames.Add("PaintBrush_Empty", _paintBrushEmptyTexture);
            toolNames.Add("PaintBrush_Filled", _paintBrushFilledTexture);
            toolNames.Add($"{AlternativeTextures.TOOL_TOKEN_HEADER}PaintBucket", _paintBucketTexture);
            toolNames.Add($"{AlternativeTextures.TOOL_TOKEN_HEADER}Scissors", _scissorsTexture);
            toolNames.Add($"{AlternativeTextures.TOOL_TOKEN_HEADER}SprayCan", _sprayCanTexture);
            toolNames.Add($"{AlternativeTextures.TOOL_TOKEN_HEADER}PaintBrush_Empty", _paintBrushEmptyTexture);
            toolNames.Add($"{AlternativeTextures.TOOL_TOKEN_HEADER}PaintBrush_Filled", _paintBrushFilledTexture);
            toolNames.Add($"{AlternativeTextures.TOOL_TOKEN_HEADER}Catalogue", _catalogueTexture);
        }

        private Texture2D GetTextureSafely(ref Texture2D texture, string textureFileName)
        {
            if (texture is null || texture.IsDisposed)
            {
                if (texture is not null && texture.IsDisposed)
                {
                    // TODO: Look into why textures are disposed when patched via Content Patcher
                    AlternativeTextures.monitor.LogOnce($"Error drawing the tool {textureFileName}: It was incorrectly disposed!", StardewModdingAPI.LogLevel.Warn);
                }
                texture = _helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, textureFileName));
            }

            return texture;
        }

        internal Texture2D GetPaintBucketTexture()
        {
            return GetTextureSafely(ref _paintBucketTexture, "PaintBucket.png");
        }

        internal Texture2D GetScissorsTexture()
        {
            return GetTextureSafely(ref _scissorsTexture, "Scissors.png");
        }

        internal Texture2D GetSprayCanTexture(bool getRareTexture = false)
        {
            return getRareTexture ? GetTextureSafely(ref _sprayCanTextureRare, "SprayCanRare.png") : GetTextureSafely(ref _sprayCanTexture, "SprayCan.png");
        }

        internal Texture2D GetPaintBrushEmptyTexture()
        {
            return GetTextureSafely(ref _paintBrushEmptyTexture, "PaintBrushEmpty.png");
        }

        internal Texture2D GetPaintBrushFilledTexture()
        {
            return GetTextureSafely(ref _paintBrushFilledTexture, "PaintBrushFilled.png");
        }

        internal Texture2D GetCatalogueTexture()
        {
            return GetTextureSafely(ref _catalogueTexture, "Catalogue.png");
        }
    }
}
