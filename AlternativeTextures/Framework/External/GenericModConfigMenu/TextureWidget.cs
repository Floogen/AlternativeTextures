using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace AlternativeTextures.Framework.External.GenericModConfigMenu
{
    public class TextureWidget
    {
        public string TextureId { get; set; }
        public int Variation { get; set; }
        public bool Enabled { get; set; }

        private bool wasClicking = false;

        public void Draw(SpriteBatch b, Vector2 pos)
        {
            var bounds = new Rectangle((int)pos.X, (int)pos.Y, OptionsCheckbox.sourceRectChecked.Width * 4, OptionsCheckbox.sourceRectChecked.Width * 4);
            bool isHovering = bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY());

            bool isClicking = Game1.input.GetMouseState().LeftButton == ButtonState.Pressed;
            if (isHovering && isClicking && !wasClicking)
            {
                Enabled = !Enabled;
            }
            wasClicking = isClicking;

            b.Draw(Game1.mouseCursors, pos, Enabled ? OptionsCheckbox.sourceRectChecked : OptionsCheckbox.sourceRectUnchecked, Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, 0);
        }

        internal void BeforeSave(ModConfig modConfig)
        {
            modConfig.SetTextureStatus(TextureId, Variation, Enabled);
        }
    }
}
