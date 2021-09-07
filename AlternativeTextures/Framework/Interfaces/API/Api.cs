using AlternativeTextures.Framework.Models;
using StardewModdingAPI;
using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using StardewValley;

namespace AlternativeTextures.Framework.Interfaces.API
{
    public interface IApi
    {
        void AddAlternativeTexture(AlternativeTextureModel model, string owner, string tileSheetPath);
    }

    public class Api : IApi
    {
        private readonly AlternativeTextures _framework;

        public Api(AlternativeTextures alternativeTexturesMod)
        {
            _framework = alternativeTexturesMod;
        }

        public void AddAlternativeTexture(AlternativeTextureModel model, string owner, string tileSheetPath)
        {
            if (String.IsNullOrEmpty(owner))
            {
                _framework.Monitor.Log($"Unable to add AlternativeTextureModel {model.GetNameWithSeason()}: Owner property is not set.");
                return;
            }

            if (String.IsNullOrEmpty(tileSheetPath))
            {
                _framework.Monitor.Log($"Unable to add AlternativeTextureModel {model.GetNameWithSeason()}: TileSheetPath property is not set.");
                return;
            }
            else if (!tileSheetPath.ToLower().Contains("texture.png"))
            {
                _framework.Monitor.Log($"Unable to add AlternativeTextureModel {model.GetNameWithSeason()}: TileSheetPath property does not contain a texture.png file.");
                return;
            }

            model.Owner = owner;
            model.Type = model.GetTextureType();

            var seasons = model.Seasons;
            for (int s = 0; s < 4; s++)
            {
                if ((seasons.Count() == 0 && s > 0) || (seasons.Count() > 0 && s >= seasons.Count()))
                {
                    continue;
                }

                // Parse the model and assign it the content pack's owner
                AlternativeTextureModel textureModel = model.ShallowCopy();

                // Override Grass Alternative Texture pack ItemNames to always be Grass, in order to be compatible with translations 
                textureModel.ItemName = textureModel.GetTextureType() == "Grass" ? "Grass" : textureModel.ItemName;

                // Add the UniqueId to the top-level Keywords
                textureModel.Keywords.Add(model.Owner);

                // Add the top-level Keywords to any ManualVariations.Keywords
                foreach (var variation in textureModel.ManualVariations)
                {
                    variation.Keywords.AddRange(textureModel.Keywords);
                }

                // Set the season (if any)
                textureModel.Season = seasons.Count() == 0 ? String.Empty : seasons[s];

                // Set the ModelName and TextureId
                textureModel.ModelName = String.IsNullOrEmpty(textureModel.Season) ? String.Concat(textureModel.GetTextureType(), "_", textureModel.ItemName) : String.Concat(textureModel.GetTextureType(), "_", textureModel.ItemName, "_", textureModel.Season);
                textureModel.TextureId = String.Concat(textureModel.Owner, ".", textureModel.ModelName);

                // Verify we are given a texture and if so, track it
                if (!File.Exists(Path.Combine(tileSheetPath, "texture.png")))
                {
                    // No texture.png found, may be using split texture files (texture_1.png, texture_2.png, etc.)
                    var textureFilePaths = Directory.GetFiles(tileSheetPath, "texture_*.png")
                        .Select(t => Path.GetFileName(t))
                        .Where(t => t.Any(char.IsDigit))
                        .OrderBy(t => Int32.Parse(Regex.Match(t, @"\d+").Value));

                    if (textureFilePaths.Count() == 0)
                    {
                        _framework.Monitor.Log($"Unable to add alternative texture for item {textureModel.ItemName} via API: No associated texture.png or split textures (texture_1.png, texture_2.png, etc.) given", LogLevel.Warn);
                        continue;
                    }

                    // Load in the first texture_#.png to get its dimensions for creating stitchedTexture
                    Texture2D baseTexture = _framework.Helper.Content.Load<Texture2D>(Path.Combine(tileSheetPath, textureFilePaths.First()));
                    Texture2D stitchedTexture = new Texture2D(Game1.graphics.GraphicsDevice, baseTexture.Width, baseTexture.Height * textureFilePaths.Count());

                    // Now stitch together the split textures into a single texture
                    Color[] pixels = new Color[stitchedTexture.Width * stitchedTexture.Height];
                    for (int x = 0; x < textureFilePaths.Count(); x++)
                    {
                        var fileName = textureFilePaths.ElementAt(x);
                        _framework.Monitor.Log($"Stitching together {textureModel.TextureId}: {fileName}", LogLevel.Trace);

                        var offset = x * baseTexture.Width * baseTexture.Height;
                        var subTexture = _framework.Helper.Content.Load<Texture2D>(Path.Combine(tileSheetPath, fileName));

                        Color[] subPixels = new Color[subTexture.Width * subTexture.Height];
                        subTexture.GetData(subPixels);
                        for (int i = 0; i < subPixels.Length; i++)
                        {
                            pixels[i + offset] = subPixels[i];
                        }
                    }

                    stitchedTexture.SetData(pixels);
                    textureModel.TileSheetPath = Path.Combine(tileSheetPath, textureFilePaths.First());
                    textureModel.Texture = stitchedTexture;
                }
                else
                {
                    // Load in the single vertical texture
                    textureModel.TileSheetPath = Path.Combine(tileSheetPath, "texture.png");
                    textureModel.Texture = _framework.Helper.Content.Load<Texture2D>(textureModel.TileSheetPath);
                }

                // Track the texture model
                AlternativeTextures.textureManager.AddAlternativeTexture(textureModel);

                // Log it
                _framework.Monitor.Log(textureModel.ToString(), LogLevel.Trace);
            }
        }
    }
}
