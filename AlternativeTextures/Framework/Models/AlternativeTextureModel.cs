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
        public bool EnableContentPatcherCheck { get; set; }
        public List<string> Keywords { get; set; } = new List<string>();
        public List<string> Seasons { get; set; } = new List<string>(); // For use by mod user to determine which seasons the texture is valid for
        internal string Season { get; set; } // Used by framework to split the Seasons property into individual AlternativeTextureModel models
        internal string TextureId { get; set; }
        internal string ModelName { get; set; }
        public int TextureWidth { get; set; }
        public int TextureHeight { get; set; }
        public int Variations { get; set; } = 1;
        internal Texture2D Texture { get; set; }
        internal string TileSheetPath { get; set; }
        public List<VariationModel> ManualVariations { get; set; } = new List<VariationModel>();
        public List<AnimationModel> Animation { get; set; } = new List<AnimationModel>();

        public int MAX_TEXTURE_HEIGHT { get { return 16384; } }
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
            Bush,
            Flooring,
            Furniture,
            Character,
            Building
        }

        public AlternativeTextureModel ShallowCopy()
        {
            return (AlternativeTextureModel)this.MemberwiseClone();
        }

        public string GetTextureType()
        {
            if (!Enum.TryParse<TextureType>(Type.Trim(), true, out var textureType))
            {
                return TextureType.Unknown.ToString();
            }

            return textureType.ToString();
        }

        public string GetId()
        {
            return TextureId;
        }

        public string GetNameWithSeason()
        {
            return ModelName;
        }

        public int GetVariations()
        {
            return ManualVariations.Where(v => v.Id >= 0).Count() > 0 ? ManualVariations.Where(v => v.Id >= 0).Count() : Variations;
        }

        public List<AnimationModel> GetAnimationData(int variation)
        {
            var manualVariation = ManualVariations.FirstOrDefault(v => v.Id == variation && v.HasAnimation());
            if (manualVariation != null)
            {
                return manualVariation.Animation;
            }

            return Animation;
        }

        public AnimationModel GetAnimationDataAtIndex(int variation, int index)
        {
            return GetAnimationData(variation).ElementAt(index);
        }

        public bool HasKeyword(string variationString, string keyword)
        {
            if (!Int32.TryParse(variationString, out var variation))
            {
                return false;
            }

            return HasKeyword(variation, keyword);
        }

        public bool HasKeyword(int variation, string keyword)
        {
            if (ManualVariations.Any(v => v.Id == variation))
            {
                return ManualVariations.First(v => v.Id == variation).Keywords.Any(k => k.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            return Keywords.Any(k => k.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public bool HasAnimation(int variation)
        {
            return Animation.Count() > 0 || ManualVariations.Any(v => v.Id == variation && v.HasAnimation());
        }

        public override string ToString()
        {
            return $"\n[\n" +
                $"\tOwner: {Owner} | ItemName: {ItemName} | ItemId: {ItemId} | Type: {Type} | Season: {Season}\n" +
                $"\tTextureWidth x TextureHeight: [{TextureWidth}x{TextureHeight}] | Variations: {Variations}\n";
        }
    }
}
