using AlternativeTextures.Framework.Models;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlternativeTextures.Framework.Managers
{
    internal class TextureManager
    {
        private IMonitor _monitor;
        private List<AlternativeTextureModel> _alternativeTextures;

        public TextureManager(IMonitor monitor)
        {
            _monitor = monitor;
            _alternativeTextures = new List<AlternativeTextureModel>();
        }

        public void AddAlternativeTexture(AlternativeTextureModel model)
        {
            _alternativeTextures.Add(model);
        }

        public List<AlternativeTextureModel> GetAllTextures()
        {
            return _alternativeTextures;
        }

        public List<string> GetValidTextureNames()
        {
            return _alternativeTextures.Select(t => t.GetId()).ToList();
        }

        public bool DoesObjectHaveAlternativeTexture(int objectId)
        {
            return _alternativeTextures.Any(t => t.ItemId == objectId);
        }

        public bool DoesObjectHaveAlternativeTexture(string objectName)
        {
            return _alternativeTextures.Any(t => String.Equals(t.ItemName, objectName, StringComparison.OrdinalIgnoreCase));
        }

        public bool DoesObjectHaveAlternativeTextureById(string objectId)
        {
            return _alternativeTextures.Any(t => String.Equals(t.GetId(), objectId, StringComparison.OrdinalIgnoreCase));
        }

        public AlternativeTextureModel GetRandomTextureModel(int objectId)
        {
            if (!DoesObjectHaveAlternativeTexture(objectId))
            {
                return null;
            }

            var randomTexture = Game1.random.Next(_alternativeTextures.Select(t => t.ItemId == objectId).Count());
            return _alternativeTextures[randomTexture];
        }

        public AlternativeTextureModel GetRandomTextureModel(string objectName)
        {
            if (!DoesObjectHaveAlternativeTexture(objectName))
            {
                return null;
            }

            var validTextures = _alternativeTextures.Where(t => String.Equals(t.ItemName, objectName, StringComparison.OrdinalIgnoreCase)).ToList();
            return validTextures[Game1.random.Next(validTextures.Count())];
        }

        public AlternativeTextureModel GetSpecificTextureModel(string textureId)
        {
            if (!DoesObjectHaveAlternativeTextureById(textureId))
            {
                return null;
            }

            return _alternativeTextures.First(t => String.Equals(t.GetId(), textureId, StringComparison.OrdinalIgnoreCase));
        }

        public void UpdateTexture(string textureId, Texture2D texture)
        {
            if (!DoesObjectHaveAlternativeTextureById(textureId))
            {
                return;
            }

            _alternativeTextures.First(t => String.Equals(t.GetId(), textureId, StringComparison.OrdinalIgnoreCase)).Texture = texture;
        }
    }
}
