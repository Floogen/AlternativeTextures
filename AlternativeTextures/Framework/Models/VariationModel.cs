using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlternativeTextures.Framework.Models
{
    public class VariationModel
    {
        public int Id { get; set; }
        public float ChanceWeight { get; set; } = 1f;
        public List<string> Keywords { get; set; } = new List<string>();
    }
}
