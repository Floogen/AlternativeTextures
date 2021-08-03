using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlternativeTextures.Framework.Models
{
    public class AlternativeTextureModel
    {
        public string Owner { get; set; }
        public string ItemName { get; set; }
        public int ItemId { get; set; } = -1;
        public ITranslationHelper Translations { get; set; }
        public string Type { get; set; }
        public string Season { get; set; }
        public int TextureWidth { get; set; }
        public int TextureHeight { get; set; }
        public int Variations { get; set; } = 1;
        public Texture2D Texture { get; set; }
        public string TileSheetPath { get; set; }

        internal enum TextureType
        {
            Unknown,
            Craftable,
            Grass,
            Tree,
            FruitTree,
            Crop,
            GiantCrop,
            ResourceClump
        }

        public string GetTextureType()
        {
            if (!Enum.TryParse<TextureType>(Type, true, out var textureType))
            {
                return TextureType.Unknown.ToString();
            }

            return textureType.ToString();
        }

        public string GetId()
        {
            return String.Concat(Owner, ".", GetNameWithSeason());
        }

        public string GetNameWithSeason()
        {
            return String.IsNullOrEmpty(Season) ? String.Concat(GetTextureType(), "_", ItemName) : String.Concat(GetTextureType(), "_", ItemName, "_", Season);
        }

        public override string ToString()
        {
            return $"\n[\n" +
                $"\tOwner: {Owner} | ItemName: {ItemName} | ItemId: {ItemId} | Type: {Type} | Season: {Season}\n" +
                $"\tTextureWidth x TextureHeight: [{TextureWidth}x{TextureHeight}] | Variations: {Variations}\n";
        }
    }
}
