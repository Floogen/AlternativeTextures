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
        internal int ItemId { get; set; } = -1;
        public string Type { get; set; }
        public List<string> Seasons { get; set; } = new List<string>(); // For use by mod user to determine which seasons the texture is valid for
        internal string Season { get; set; } // Used by framework to split the Seasons property into individual AlternativeTextureModel models
        public int TextureWidth { get; set; }
        public int TextureHeight { get; set; }
        public int Variations { get; set; } = 1;
        internal Texture2D Texture { get; set; }
        internal string TileSheetPath { get; set; }
        public List<VariationModel> ManualVariations { get; set; } = new List<VariationModel>();

        internal enum TextureType
        {
            Unknown,
            Craftable,
            Grass,
            Tree,
            FruitTree,
            Crop,
            GiantCrop,
            ResourceClump,
            Bush
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
