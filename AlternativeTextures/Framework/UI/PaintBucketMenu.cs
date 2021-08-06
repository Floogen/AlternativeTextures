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
        public ClickableTextureComponent hovered;
        public ClickableTextureComponent forwardButton;
        public ClickableTextureComponent backButton;

        public List<Item> textureOptions = new List<Item>();
        public List<ClickableTextureComponent> availableTextures = new List<ClickableTextureComponent>();

        private int _startingRow = 0;
        private int _texturesPerRow = 6;
        private int _maxRows = 4;

        private Object _textureTarget;

        public PaintBucketMenu(Object target) : base(0, 0, 832, 576, showUpperRightCloseButton: true)
        {
            if (!target.modData.ContainsKey("AlternativeTextureOwner") || !target.modData.ContainsKey("AlternativeTextureName"))
            {
                this.exitThisMenu();
                return;
            }

            _textureTarget = target;

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

                    this.textureOptions.Add(objectWithVariation);
                }
            }

            var sourceRect = target is Fence ? this.GetFenceSourceRect(target as Fence, availableModels.First().TextureHeight, 0) : new Rectangle(0, 0, availableModels.First().TextureWidth, availableModels.First().TextureHeight);
            for (int r = 0; r < _maxRows; r++)
            {
                for (int c = 0; c < _texturesPerRow; c++)
                {
                    var componentId = c + r * _texturesPerRow;
                    this.availableTextures.Add(new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth + componentId % _texturesPerRow * 64 * 2, base.yPositionOnScreen + sourceRect.Height + componentId / _texturesPerRow * (4 * sourceRect.Height), 4 * sourceRect.Width, 4 * sourceRect.Height), availableModels.First().Texture, new Rectangle(), 4f, false)
                    {
                        myID = componentId,
                        downNeighborID = componentId + _texturesPerRow,
                        upNeighborID = r >= _texturesPerRow ? componentId - _texturesPerRow : -1,
                        rightNeighborID = c == 5 ? 9997 : componentId + 1,
                        leftNeighborID = c > 0 ? componentId - 1 : 9998
                    });
                }
            }

            this.backButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen - 64, base.yPositionOnScreen + 8, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
            {
                myID = 9998,
                rightNeighborID = 0
            };
            this.forwardButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 64 - 48, base.yPositionOnScreen + base.height - 48, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
            {
                myID = 9997
            };

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

            this.forwardButton.tryHover(x, y, 0.2f);
            this.backButton.tryHover(x, y, 0.2f);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = false)
        {
            base.receiveLeftClick(x, y, playSound);
            if (Game1.activeClickableMenu == null)
            {
                return;
            }

            foreach (ClickableTextureComponent c in this.availableTextures)
            {
                if (c.containsPoint(x, y) && c.item != null)
                {
                    _textureTarget.modData.Clear();
                    foreach (string key in c.item.modData.Keys)
                    {
                        _textureTarget.modData[key] = c.item.modData[key];
                    }

                    for (int j = 0; j < 12; j++)
                    {
                        var randomColor = new Color(Game1.random.Next(256), Game1.random.Next(256), Game1.random.Next(256));
                        AlternativeTextures.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(355, 1200, 16, 16), _textureTarget.tileLocation.Value * 64f, flipped: false, 0.01f, randomColor)
                        {
                            motion = new Vector2((float)Game1.random.Next(-10, 11) / 10f, -Game1.random.Next(5, 7)),
                            acceleration = new Vector2(0f, (float)Game1.random.Next(13, 17) / 100f),
                            accelerationChange = new Vector2(0f, -0.001f),
                            scale = 4f,
                            layerDepth = (_textureTarget.tileLocation.Y + 1f) * 64f / 10000f,
                            animationLength = 11,
                            totalNumberOfLoops = 99,
                            interval = Game1.random.Next(20, 90),
                            delayBeforeAnimationStart = j * 20
                        });
                        if (j % 6 == 0)
                        {
                            AlternativeTextures.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(50, _textureTarget.tileLocation.Value * 64f, randomColor));
                            AlternativeTextures.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(12, _textureTarget.tileLocation.Value * 64f, Color.White));
                        }
                    }
                    this.exitThisMenu();
                    return;
                }
            }

            if (_startingRow > 0 && this.backButton.containsPoint(x, y))
            {
                _startingRow--;
                Game1.playSound("shiny4");
                return;
            }
            if ((_maxRows + _startingRow) * _texturesPerRow < this.textureOptions.Count && this.forwardButton.containsPoint(x, y))
            {
                _startingRow++;
                Game1.playSound("shiny4");
                return;
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && _startingRow > 0)
            {
                _startingRow--;
                Game1.playSound("shiny4");
            }
            else if (direction < 0 && (_maxRows + _startingRow) * _texturesPerRow < this.textureOptions.Count)
            {
                _startingRow++;
                Game1.playSound("shiny4");
            }
        }

        public override void draw(SpriteBatch b)
        {
            if (!Game1.dialogueUp && !Game1.IsFading())
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
                SpriteText.drawStringWithScrollCenteredAt(b, "Paint Bucket", base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 64);
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, Color.White, 4f);

                for (int i = 0; i < this.availableTextures.Count; i++)
                {
                    this.availableTextures[i].item = null;
                    this.availableTextures[i].sourceRect = new Rectangle();

                    var textureIndex = i + _startingRow * _texturesPerRow;
                    if (textureIndex < textureOptions.Count)
                    {
                        var target = textureOptions[textureIndex];
                        var textureModel = AlternativeTextures.textureManager.GetSpecificTextureModel(target.modData["AlternativeTextureName"]);

                        this.availableTextures[i].item = target;
                        this.availableTextures[i].texture = textureModel.Texture;
                        this.availableTextures[i].sourceRect = _textureTarget is Fence ? this.GetFenceSourceRect(_textureTarget as Fence, textureModel.TextureHeight, Int32.Parse(target.modData["AlternativeTextureVariation"])) : new Rectangle(0, Int32.Parse(target.modData["AlternativeTextureVariation"]) * textureModel.TextureHeight, textureModel.TextureWidth, textureModel.TextureHeight);
                    }
                    this.availableTextures[i].draw(b, Color.White, 0.87f);
                }
            }

            if (this.hovered != null && this.hovered.item != null)
            {
                if (this.hovered.item.modData.ContainsKey("AlternativeTextureName") && this.hovered.item.modData.ContainsKey("AlternativeTextureVariation"))
                {
                    var displayName = String.Concat(this.hovered.item.modData["AlternativeTextureName"], "_", Int32.Parse(this.hovered.item.modData["AlternativeTextureVariation"]) + 1);
                    IClickableMenu.drawHoverText(b, Game1.parseText(displayName, Game1.dialogueFont, 320), Game1.dialogueFont);
                }
            }

            if (_startingRow > 0)
            {
                this.backButton.draw(b);
            }
            if ((_maxRows + _startingRow) * _texturesPerRow < this.textureOptions.Count)
            {
                this.forwardButton.draw(b);
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