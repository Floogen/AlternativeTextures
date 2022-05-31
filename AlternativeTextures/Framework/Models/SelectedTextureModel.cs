using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlternativeTextures.Framework.Models
{
    public class SelectedTextureModel
    {
        public string Owner { get; set; }
        public string TextureName { get; set; }
        public List<int> Variations { get; set; } = new List<int>();
    }
}
