using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace AlternativeTextures.Framework.UI
{
    internal class PaintBucketMenu : IClickableMenu
    {
        public ClickableTextureComponent okButton;
        public ClickableTextureComponent randomButton;
        public ClickableTextureComponent hovered;

        public List<ClickableTextureComponent> availableTextures = new List<ClickableTextureComponent>();

        private int _texturesPerRow = 6;

        public PaintBucketMenu(Object target) : base(0, 0, 832, 576, showUpperRightCloseButton: true)
        {
            if (!target.modData.ContainsKey("AlternativeTextureOwner") || !target.modData.ContainsKey("AlternativeTextureName"))
            {
                this.exitThisMenu();
                return;
            }

            // Set up menu structure
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr)
            {
                base.height += 64;
            }

            Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height);
            base.xPositionOnScreen = (int)topLeft.X;
            base.yPositionOnScreen = (int)topLeft.Y + 32;

            // Populate the texture selection components
            var modelName = target.modData["AlternativeTextureName"].Replace($"{target.modData["AlternativeTextureOwner"]}.", String.Empty);
            var availableModels = AlternativeTextures.textureManager.GetAvailableTextureModels(modelName);
            for (int m = 0; m < availableModels.Count; m++)
            {
                for (int v = 0; v < availableModels[m].Variations; v++)
                {
                    var objectWithVariation = target.getOne();
                    objectWithVariation.modData["AlternativeTextureName"] = availableModels[m].GetId();
                    objectWithVariation.modData["AlternativeTextureVariation"] = v.ToString();

                    var componentId = this.availableTextures.Count();
                    var displayName = string.Concat(availableModels[m].GetId(), "_", v + 1);
                    var sourceRect = target is Fence ? this.GetFenceSourceRect(target as Fence, availableModels[m].TextureHeight, v) : new Rectangle(0, v * availableModels[m].TextureHeight, availableModels[m].TextureWidth, availableModels[m].TextureHeight);
                    this.availableTextures.Add(new ClickableTextureComponent(displayName, new Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth + componentId % _texturesPerRow * 64 * 2, base.yPositionOnScreen + sourceRect.Height + componentId / _texturesPerRow * sourceRect.Height, 4 * sourceRect.Width, 4 * sourceRect.Height), null, displayName, availableModels[m].Texture, sourceRect, 4f, false)
                    {
                        item = objectWithVariation,
                        myID = componentId,
                        rightNeighborID = ((componentId % _texturesPerRow == 2) ? (-1) : (componentId + 1)),
                        leftNeighborID = ((componentId % _texturesPerRow == 0) ? (-1) : (componentId - 1)),
                        downNeighborID = componentId + _texturesPerRow,
                        upNeighborID = componentId - _texturesPerRow
                    });
                }
            }

            if (Game1.options.SnappyMenus)
            {
                base.populateClickableComponentList();
                this.snapToDefaultClickableComponent();
            }
        }
        public override void performHoverAction(int x, int y)
        {
            this.hovered = null;
            if (Game1.IsFading())
            {
                return;
            }

            foreach (ClickableTextureComponent c in this.availableTextures)
            {
                if (c.containsPoint(x, y))
                {
                    c.scale = Math.Min(c.scale + 0.05f, 4.1f);
                    this.hovered = c;
                }
                else
                {
                    c.scale = Math.Max(4f, c.scale - 0.025f);
                }
            }
        }

        public override void draw(SpriteBatch b)
        {
            if (!Game1.dialogueUp && !Game1.IsFading())
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
                SpriteText.drawStringWithScrollCenteredAt(b, "Paint Bucket", base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, Color.White, 4f);
                foreach (ClickableTextureComponent c in this.availableTextures)
                {
                    c.draw(b, Color.White, 0.87f);
                }
            }
            if (!Game1.IsFading() && this.okButton != null)
            {
                this.okButton.draw(b);
            }
            if (this.hovered != null)
            {
                if (this.hovered.item.modData.ContainsKey("AlternativeTextureName") && this.hovered.item.modData.ContainsKey("AlternativeTextureVariation"))
                {
                    var displayName = String.Concat(this.hovered.item.modData["AlternativeTextureName"], "_", Int32.Parse(this.hovered.item.modData["AlternativeTextureVariation"]) + 1);
                    IClickableMenu.drawHoverText(b, Game1.parseText(displayName, Game1.dialogueFont, 320), Game1.dialogueFont);
                }
            }

            Game1.mouseCursorTransparency = 1f;
            base.drawMouse(b);
        }

        private Rectangle GetFenceSourceRect(Fence fence, int textureHeight, int variation)
        {
            int sourceRectPosition = 1;
            if ((float)fence.health > 1f || fence.repairQueued.Value)
            {
                int drawSum = fence.getDrawSum(Game1.currentLocation);
                sourceRectPosition = Fence.fenceDrawGuide[drawSum];
            }

            return new Rectangle((sourceRectPosition * Fence.fencePieceWidth % fence.fenceTexture.Value.Bounds.Width), (variation * textureHeight) + (sourceRectPosition * Fence.fencePieceWidth / fence.fenceTexture.Value.Bounds.Width * Fence.fencePieceHeight), Fence.fencePieceWidth, Fence.fencePieceHeight);
        }
    }
}