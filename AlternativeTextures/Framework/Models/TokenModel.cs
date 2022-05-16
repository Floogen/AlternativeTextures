using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlternativeTextures.Framework.Models
{
    public class TokenModel
    {
        public string Id { get; set; }
        public int Variation { get; set; }
        public AlternativeTextureModel AlternativeTexture { get; set; }
    }
}
