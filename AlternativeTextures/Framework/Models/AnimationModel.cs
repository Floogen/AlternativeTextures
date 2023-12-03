using AlternativeTextures.Framework.Enums;

namespace AlternativeTextures.Framework.Models
{
    public class AnimationModel
    {
        public int Frame { get; set; }
        public int Duration { get; set; } = 1000;
        public FrameType Type { get; set; } = FrameType.Default;
    }
}
