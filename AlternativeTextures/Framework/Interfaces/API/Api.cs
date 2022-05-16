﻿using AlternativeTextures.Framework.Models;
using StardewModdingAPI;
using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;

namespace AlternativeTextures.Framework.Interfaces.API
{
    public interface IApi
    {
        void AddAlternativeTexture(AlternativeTextureModel model, string owner, List<Texture2D> textures);
    }

    public class Api : IApi
    {
        private readonly AlternativeTextures _framework;

        public Api(AlternativeTextures alternativeTexturesMod)
        {
            _framework = alternativeTexturesMod;
        }

        public void AddAlternativeTexture(AlternativeTextureModel model, string owner, Texture2D texture)
        {
            AddAlternativeTexture(model, owner, new List<Texture2D>() { texture });
        }

        public void AddAlternativeTexture(AlternativeTextureModel model, string owner, List<Texture2D> textures)
        {
            if (String.IsNullOrEmpty(owner))
            {
                _framework.Monitor.Log($"Unable to add AlternativeTextureModel {model.GetNameWithSeason()}: Owner property is not set.");
                return;
            }

            if (textures.Count() == 0)
            {
                _framework.Monitor.Log($"Unable to add AlternativeTextureModel {model.GetNameWithSeason()}: Textures property is empty.");
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

                // Verify we are given a singular texture, if not then stitch them all together
                if (textures.Count() > 1)
                {
                    if (textureModel.IsDecoration())
                    {
                        _framework.Monitor.Log($"Unable to add alternative texture for item {textureModel.ItemName} from {textureModel.TextureId}: Split textures (texture_1.png, texture_2.png, etc.) are not allowed for Decoration types (wallpapers / floors)!", LogLevel.Warn);
                        continue;
                    }

                    // Load in the first texture_#.png to get its dimensions for creating stitchedTexture
                    int maxVariationsPerTexture = AlternativeTextureModel.MAX_TEXTURE_HEIGHT / textureModel.TextureHeight;

                    int variation = 0;
                    foreach (var splitTexture in textures)
                    {
                        textureModel.Textures[variation] = splitTexture;

                        variation++;
                    }

                    textureModel.TileSheetPath = String.Empty;
                }
                else
                {
                    // Load in the single vertical texture
                    textureModel.TileSheetPath = String.Empty;
                    Texture2D singularTexture = textures.First();
                    if (singularTexture.Height >= AlternativeTextureModel.MAX_TEXTURE_HEIGHT)
                    {
                        _framework.Monitor.Log($"Unable to add alternative texture for {textureModel.Owner}: The texture {textureModel.TextureId} has a height larger than 16384!\nPlease split it into individual textures (e.g. texture_0.png, texture_1.png, etc.) to resolve this issue.", LogLevel.Warn);
                        continue;
                    }
                    else if (textureModel.IsDecoration() && singularTexture.Width < 256)
                    {
                        _framework.Monitor.Log($"Unable to add alternative texture for {textureModel.ItemName} from {textureModel.TextureId}: The required image width is 256 for Decoration types (wallpapers / floors). Please correct the image's width manually.", LogLevel.Warn);
                        continue;
                    }
                    else if (textureModel.IsDecoration())
                    {
                        if (singularTexture.Width < 256)
                        {
                            _framework.Monitor.Log($"Unable to add alternative texture for {textureModel.ItemName} from {textureModel.TextureId}: The required image width is 256 for Decoration types (wallpapers / floors). Please correct the image's width manually.", LogLevel.Warn);
                            continue;
                        }

                        textureModel.Textures[0] = singularTexture;
                    }
                    else if (!_framework.SplitVerticalTexturesToModel(textureModel, textureModel.TextureId, singularTexture))
                    {
                        continue;
                    }
                }

                // Track the texture model
                AlternativeTextures.textureManager.AddAlternativeTexture(textureModel);

                // Log it
                if (AlternativeTextures.modConfig.OutputTextureDataToLog)
                {
                    _framework.Monitor.Log(textureModel.ToString(), LogLevel.Trace);
                }
            }
        }
    }
}
