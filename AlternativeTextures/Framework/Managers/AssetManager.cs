using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.GameData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlternativeTextures.Framework.Managers
{
    internal class AssetManager
    {
        // Tilesheet related for decorations (wallpaper / floor)
        private TextureManager _textureManager;

        internal string assetFolderPath;
        internal Dictionary<string, Texture2D> toolNames = new Dictionary<string, Texture2D>();

        private Texture2D _paintBucketTexture;
        private Texture2D _scissorsTexture;
        private Texture2D _paintBrushEmptyTexture;
        private Texture2D _paintBrushFilledTexture;

        public AssetManager(IModHelper helper, TextureManager textureManager)
        {
            // Get the asset folder path
            assetFolderPath = helper.ModContent.GetInternalAssetName(Path.Combine("Framework", "Assets")).Name;

            // Load in the assets
            _paintBucketTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "PaintBucket.png"));
            _scissorsTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "Scissors.png"));
            _paintBrushEmptyTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "PaintBrushEmpty.png"));
            _paintBrushFilledTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "PaintBrushFilled.png"));

            // Setup toolNames
            toolNames.Add("PaintBucket", _paintBucketTexture);
            toolNames.Add("Scissors", _scissorsTexture);
            toolNames.Add("PaintBrush_Empty", _paintBrushEmptyTexture);
            toolNames.Add("PaintBrush_Filled", _paintBrushFilledTexture);

            // Get the TextureMananger
            _textureManager = textureManager;
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (asset.Name.IsEquivalentTo("Data/AdditionalWallpaperFlooring") && _textureManager.GetValidTextureNamesWithSeason().Count > 0)
            {
                return true;
            }

            return false;
        }

        public void Edit<T>(IAssetData asset)
        {
            if (asset.Name.IsEquivalentTo("Data/AdditionalWallpaperFlooring") && _textureManager.GetValidTextureNamesWithSeason().Count > 0)
            {
                if (asset.Name.IsEquivalentTo("Data/AdditionalWallpaperFlooring"))
                {
                    List<ModWallpaperOrFlooring> moddedDecorations = asset.GetData<List<ModWallpaperOrFlooring>>();
                    foreach (var textureModel in _textureManager.GetAllTextures().Where(t => t.IsDecoration() && !moddedDecorations.Any(d => d.ID == t.GetId())))
                    {
                        var decoration = new ModWallpaperOrFlooring()
                        {
                            ID = textureModel.GetId(),
                            Texture = $"{AlternativeTextures.TEXTURE_TOKEN_HEADER}{textureModel.GetTokenId()}",
                            IsFlooring = String.Equals(textureModel.ItemName, "Floor", StringComparison.OrdinalIgnoreCase),
                            Count = textureModel.GetVariations()
                        };

                        moddedDecorations.Add(decoration);
                    }
                }
            }
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (toolNames.Any(n => asset.Name.IsEquivalentTo($"{AlternativeTextures.TOOL_TOKEN_HEADER}{n.Key}")))
            {
                return true;
            }
            return AlternativeTextures.textureManager.GetAllTextures().Any(t => asset.Name.IsEquivalentTo($"{AlternativeTextures.TEXTURE_TOKEN_HEADER}{t.GetTokenId()}"));
        }

        public T Load<T>(IAssetInfo asset)
        {
            if (toolNames.Any(n => asset.Name.IsEquivalentTo($"{AlternativeTextures.TOOL_TOKEN_HEADER}{n.Key}")) || AlternativeTextures.textureManager.GetAllTextures().Any(t => asset.Name.IsEquivalentTo($"{AlternativeTextures.TEXTURE_TOKEN_HEADER}{t.GetTokenId()}")))
            {
                if (toolNames.Any(n => asset.Name.IsEquivalentTo($"{AlternativeTextures.TOOL_TOKEN_HEADER}{n.Key}")))
                {
                    return (T)(object)toolNames.First(n => asset.Name.IsEquivalentTo($"{AlternativeTextures.TOOL_TOKEN_HEADER}{n.Key}")).Value;
                }

                var textureModel = AlternativeTextures.textureManager.GetAllTextures().First(t => asset.Name.IsEquivalentTo($"{AlternativeTextures.TEXTURE_TOKEN_HEADER}{t.GetTokenId()}"));
                return (T)(object)textureModel.Textures.First();
            }

            return default(T);
        }

        internal Texture2D GetPaintBucketTexture()
        {
            return _paintBucketTexture;
        }

        internal Texture2D GetScissorsTexture()
        {
            return _scissorsTexture;
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
