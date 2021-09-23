using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlternativeTextures.Framework.Models
{
    public class DisabledTextureModel
    {
        public string TextureId { get; set; }
        public List<int> DisabledVariations { get; set; } = new List<int>();
    }
}
