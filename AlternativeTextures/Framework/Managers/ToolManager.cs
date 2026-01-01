using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.GameData.Tools;
using System.Collections.Generic;
using System.IO;

namespace AlternativeTextures.Framework.Managers
{
    internal class ToolManager
    {
        internal string assetFolderPath;
        internal Dictionary<string, string> toolKeyToData = [];

        private IModHelper _helper;

        internal const string TOOL_ID_PAINT_BUCKET = $"(T){AlternativeTextures.MOD_ID}_PaintBucket";
        internal const string TOOL_ID_SCISSORS = $"(T){AlternativeTextures.MOD_ID}_Scissors";
        internal const string TOOL_ID_PAINT_BRUSH = $"(T){AlternativeTextures.MOD_ID}_PaintBrush";
        internal const string TOOL_ID_SPRAY_CAN = $"(T){AlternativeTextures.MOD_ID}_SprayCan";
        internal const string TOOL_ID_SPRAY_CAN_RARE = $"(T){AlternativeTextures.MOD_ID}_SprayCanRare";
        internal const string TOOL_ID_CATALOGUE = $"(T){AlternativeTextures.MOD_ID}_Catalogue";

        public ToolManager(IModHelper helper)
        {
            _helper = helper;

            // Get the asset folder path
            assetFolderPath = helper.ModContent.GetInternalAssetName(Path.Combine("Framework", "Assets")).Name;

            // Setup toolNames
            toolKeyToData.Add($"{AlternativeTextures.TOOL_TOKEN_HEADER}PaintBucket", Path.Combine(assetFolderPath, "PaintBucket.png"));
            toolKeyToData.Add($"{AlternativeTextures.TOOL_TOKEN_HEADER}Scissors", Path.Combine(assetFolderPath, "Scissors.png"));
            toolKeyToData.Add($"{AlternativeTextures.TOOL_TOKEN_HEADER}SprayCan", Path.Combine(assetFolderPath, "SprayCan.png"));
            toolKeyToData.Add($"{AlternativeTextures.TOOL_TOKEN_HEADER}SprayCanRare", Path.Combine(assetFolderPath, "SprayCanRare.png"));
            toolKeyToData.Add($"{AlternativeTextures.TOOL_TOKEN_HEADER}PaintBrush_Empty", Path.Combine(assetFolderPath, "PaintBrushEmpty.png"));
            toolKeyToData.Add($"{AlternativeTextures.TOOL_TOKEN_HEADER}PaintBrush_Filled", Path.Combine(assetFolderPath, "PaintBrushFilled.png"));
            toolKeyToData.Add($"{AlternativeTextures.TOOL_TOKEN_HEADER}Catalogue", Path.Combine(assetFolderPath, "Catalogue.png"));
        }

        internal void Edit_DataTools(IAssetData data)
        {
            IDictionary<string, ToolData> toolData = data.AsDictionary<string, ToolData>().Data;
            AddToolDataEntry(toolData, "PaintBucket", "paint_bucket", AlternativeTextures.PAINT_BUCKET_FLAG, true.ToString());
            AddToolDataEntry(toolData, "Scissors", "scissors", AlternativeTextures.SCISSORS_FLAG, true.ToString());
            AddToolDataEntry(toolData, "SprayCan", "spray_can", AlternativeTextures.SPRAY_CAN_FLAG, null);
            AddToolDataEntry(toolData, "SprayCanRare", "spray_can", AlternativeTextures.SPRAY_CAN_FLAG, null).ModData[AlternativeTextures.SPRAY_CAN_RARE] = null;
            AddToolDataEntry(toolData, "Catalogue", "catalogue", AlternativeTextures.CATALOGUE_FLAG, null);

            AddToolDataEntry(toolData, "PaintBrush", "paint_brush", AlternativeTextures.PAINT_BRUSH_FLAG, null).Texture = $"{AlternativeTextures.TOOL_TOKEN_HEADER}PaintBrush_Empty";
        }

        private ToolData AddToolDataEntry(IDictionary<string, ToolData> toolData, string toolKey, string translationKey, string modDataKey, string modDataValue)
        {
            string fullId = $"{AlternativeTextures.MOD_ID}_{toolKey}";
            ToolData newToolData = new()
            {
                ClassName = "GenericTool",
                Name = fullId,
                SalePrice = 500,
                DisplayName = _helper.Translation.Get($"tools.name.{translationKey}"),
                Description = _helper.Translation.Get($"tools.description.{translationKey}"),
                Texture = $"{AlternativeTextures.TOOL_TOKEN_HEADER}{toolKey}",
                ModData = new()
                {
                    [modDataKey] = modDataValue
                }
            };
            toolData[fullId] = newToolData;
            return newToolData;
        }

        internal void Edit_DataShops(IAssetData data)
        {
            data.AsDictionary<string, ShopData>().Data["Carpenter"].Items.AddRange([
                new(){
                    Id = TOOL_ID_PAINT_BUCKET,
                    ItemId = TOOL_ID_PAINT_BUCKET,
                },
                new(){
                    Id = TOOL_ID_SCISSORS,
                    ItemId = TOOL_ID_SCISSORS,
                },
                new(){
                    Id = TOOL_ID_PAINT_BRUSH,
                    ItemId = TOOL_ID_PAINT_BRUSH,
                },
                // normal and rare spray cans: use SYNCED_RANDOM to achieve 1% chance to get rare version, mutually exclusive condition
                new(){
                    Id = TOOL_ID_SPRAY_CAN,
                    ItemId = TOOL_ID_SPRAY_CAN,
                    Condition = $"!SYNCED_RANDOM day {TOOL_ID_SPRAY_CAN} 0.01"
                },
                new(){
                    Id = TOOL_ID_SPRAY_CAN_RARE,
                    ItemId = TOOL_ID_SPRAY_CAN_RARE,
                    Condition = $"SYNCED_RANDOM day {TOOL_ID_SPRAY_CAN} 0.01"
                },
                new(){
                    Id = TOOL_ID_CATALOGUE,
                    ItemId = TOOL_ID_CATALOGUE,
                },
            ]);
        }

        internal static Texture2D GetPaintBrushEmptyTexture()
        {
            return Game1.content.Load<Texture2D>($"{AlternativeTextures.TOOL_TOKEN_HEADER}PaintBrush_Empty");
        }

        internal static Texture2D GetPaintBrushFilledTexture()
        {
            return Game1.content.Load<Texture2D>($"{AlternativeTextures.TOOL_TOKEN_HEADER}PaintBrush_Filled");
        }
    }
}
