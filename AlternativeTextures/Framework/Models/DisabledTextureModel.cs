using System.Collections.Generic;

namespace AlternativeTextures.Framework.Models
{
    public class DisabledTextureModel
    {
        public string TextureId { get; set; }
        public List<int> DisabledVariations { get; set; } = new List<int>();
    }
}
