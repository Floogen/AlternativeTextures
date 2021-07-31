using AlternativeTextures.Framework.Models;
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
        private List<AlternativeTextureModel> _alternativeTextures;

        public TextureManager()
        {
            _alternativeTextures = new List<AlternativeTextureModel>();
        }

        public void AddAlternativeTexture(AlternativeTextureModel model)
        {
            _alternativeTextures.Add(model);
        }

        public bool DoesObjectHaveAlternativeTexture(int objectId)
        {
            return _alternativeTextures.Any(t => t.ItemId == objectId);
        }

        public AlternativeTextureModel GetRandomTextureModel(int objectId)
        {
            if (!_alternativeTextures.Any(t => t.ItemId == objectId))
            {
                return null;
            }

            var randomTexture = Game1.random.Next(_alternativeTextures.Select(t => t.ItemId == objectId).Count());
            return _alternativeTextures[randomTexture];
        }

        public AlternativeTextureModel GetSpecificTextureModel(string textureId)
        {
            if (!_alternativeTextures.Any(t => t.GetId() == textureId))
            {
                return null;
            }

            return _alternativeTextures.First(t => t.GetId() == textureId);
        }
    }
}
