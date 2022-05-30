using AlternativeTextures.Framework.Models;
using AlternativeTextures.Framework.Patches;
using AlternativeTextures.Framework.Patches.Buildings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static AlternativeTextures.Framework.Models.AlternativeTextureModel;
using Object = StardewValley.Object;

namespace AlternativeTextures.Framework.UI
{
    internal class SprayCanMenu : PaintBucketMenu
    {
        public SprayCanMenu(Object target, Vector2 position, TextureType textureType, string modelName, string uiTitle = "Spray Can", int textureTileWidth = -1) : base(target, position, textureType, modelName, uiTitle, textureTileWidth, isSprayCan: true)
        {
            if (!target.modData.ContainsKey("AlternativeTextureOwner") || !target.modData.ContainsKey("AlternativeTextureName"))
            {
                this.exitThisMenu();
                return;
            }
            _title = uiTitle;

        }

        private void SetEnabledTexture(ModDataDictionary modData)
        {
            if (modData.ContainsKey("AlternativeTextureName") && modData.ContainsKey("AlternativeTextureOwner") && modData.ContainsKey("AlternativeTextureVariation") && Int32.TryParse(modData["AlternativeTextureVariation"], out int variation))
            {
                SetEnabledTexture(modData["AlternativeTextureName"], modData["AlternativeTextureOwner"], variation);
            }
        }

        private void SetEnabledTexture(string textureName, string owner, int variation)
        {
            if (_selectedIdsToModels.ContainsKey(textureName))
            {
                if (_selectedIdsToModels[textureName].Variations.Contains(variation))
                {
                    _selectedIdsToModels[textureName].Variations.Remove(variation);

                    if (_selectedIdsToModels[textureName].Variations.Count == 0)
                    {
                        _selectedIdsToModels.Remove(textureName);
                    }
                }
                else
                {
                    _selectedIdsToModels[textureName].Variations.Add(variation);
                }
            }
            else
            {
                _selectedIdsToModels[textureName] = new SelectedTextureModel()
                {
                    Owner = owner,
                    TextureName = textureName,
                    Variations = new List<int>() { variation }
                };
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Escape && base.readyToClose())
            {
                Game1.player.modData[AlternativeTextures.ENABLED_SPRAY_CAN_TEXTURES] = JsonConvert.SerializeObject(_selectedIdsToModels);
                base.exitThisMenu();
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = false)
        {
            if (Game1.activeClickableMenu == null)
            {
                return;
            }

            foreach (ClickableTextureComponent c in this.availableTextures)
            {
                if (c.containsPoint(x, y) && c.item != null)
                {
                    SetEnabledTexture(c.item.modData);
                }
            }
        }
    }
}