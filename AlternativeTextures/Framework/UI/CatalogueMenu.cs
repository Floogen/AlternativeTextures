using AlternativeTextures.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace AlternativeTextures.Framework.UI
{
    internal class CatalogueMenu : IClickableMenu
    {
        private List<Object> _displayableObjects;
        private List<Object> _currentlyDisplayedObjects;

        private const int PAGE_SIZE = 5;

        private int _currentTabIndex;
        private int _currentObjectIndex;
        private int _currentButtonNeighborID;

        private TextBox _searchBox;
        private string _cachedTextValue;


        private List<ClickableTextureComponent> _tabButtons;
        private List<ClickableComponent> _objectButtons;

        private enum Filter
        {
            None,
            Tables,
            Chairs,
            Pictures,
            Rugs,
            Decorations
        };

        public CatalogueMenu() : base(0, 0, 900, 600, showUpperRightCloseButton: false)
        {
            // Set up menu structure
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr)
            {
                base.height += 64;
            }

            Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height);
            base.xPositionOnScreen = (int)topLeft.X;
            base.yPositionOnScreen = (int)topLeft.Y;

            // Set the items to display
            // TODO: Filter this so it only uses furniture that have alternative textures available
            _displayableObjects = new List<Object>();
            _currentlyDisplayedObjects = new List<Object>();
            foreach (Object item in Utility.getAllFurnituresForFree().Keys)
            {
                // Set the stack based on the amount of available textures for the item
                var instanceName = $"{AlternativeTextureModel.TextureType.Furniture}_{item.Name}";
                int texturesAvailable = AlternativeTextures.textureManager.GetAvailableTextureModels(instanceName, Game1.player.currentLocation.GetSeasonForLocation()).Sum(t => t.Variations);

                item.stack.Value = texturesAvailable;
                if (texturesAvailable == 0)
                {
                    continue;
                }

                _displayableObjects.Add(item);
                _currentlyDisplayedObjects.Add(item);
            }

            // Establish the object buttons
            _objectButtons = new List<ClickableComponent>();
            for (int i = 0; i < PAGE_SIZE; i++)
            {
                _objectButtons.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 16, base.yPositionOnScreen + 112 + i * ((base.height - 256) / 4 + 8), base.width - 32, (base.height - 256) / 4 + 12), i.ToString() ?? "")
                {
                    myID = IncrementAndGetButtonID(),
                    leftNeighborID = 1000,
                    rightNeighborID = 99999,
                    fullyImmutable = true
                });
            }

            // Establish the tabs
            _tabButtons = new List<ClickableTextureComponent>()
            {
                new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(96, 48, 16, 16), 4f)
                {
                    myID = SetAndGetButtonID(1000),
                    upNeighborID = -1,
                    downNeighborID = -1,
                    rightNeighborID = -1,
                    name = Filter.None.ToString()
                },
                new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(80, 48, 16, 16), 4f)
                {
                    myID = IncrementAndGetButtonID(),
                    upNeighborID = -1,
                    downNeighborID = -1,
                    rightNeighborID = -1,
                    name = Filter.Tables.ToString()
                },
                new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(64, 48, 16, 16), 4f)
                {
                    myID = IncrementAndGetButtonID(),
                    upNeighborID = -1,
                    downNeighborID = -1,
                    rightNeighborID = -1,
                    name = Filter.Chairs.ToString()
                },
                new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(64, 64, 16, 16), 4f)
                {
                    myID = IncrementAndGetButtonID(),
                    upNeighborID = -1,
                    downNeighborID = -1,
                    rightNeighborID = -1,
                    name = Filter.Pictures.ToString()
                },
                new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(96, 64, 16, 16), 4f)
                {
                    myID = IncrementAndGetButtonID(),
                    upNeighborID = -1,
                    downNeighborID = -1,
                    rightNeighborID = -1,
                    name = Filter.Rugs.ToString()
                },
                new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(80, 64, 16, 16), 4f)
                {
                    myID = IncrementAndGetButtonID(),
                    upNeighborID = -1,
                    downNeighborID = -1,
                    rightNeighborID = -1,
                    name = Filter.Decorations.ToString()
                }
            };

            // Establish the search box
            _searchBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
            {
                X = base.xPositionOnScreen + 32,
                Y = base.yPositionOnScreen + 32,
                Width = 384,
                limitWidth = false,
                Text = String.Empty
            };

            Game1.keyboardDispatcher.Subscriber = this._searchBox;
            _searchBox.Selected = true;

            // Call snap functions
            if (Game1.options.SnappyMenus)
            {
                base.populateClickableComponentList();
                this.snapToDefaultClickableComponent();
            }
        }

        private int SetAndGetButtonID(int value)
        {
            _currentButtonNeighborID = value;
            return _currentButtonNeighborID;
        }

        private int IncrementAndGetButtonID(int offset = 1)
        {
            return _currentButtonNeighborID += offset;
        }

        private void UpdateSaleButtonNeighbors()
        {
            ClickableComponent lastValidButton = _objectButtons[0];
            for (int i = 0; i < _objectButtons.Count; i++)
            {
                ClickableComponent button = _objectButtons[i];
                button.upNeighborImmutable = true;
                button.downNeighborImmutable = true;
                button.upNeighborID = ((i > 0) ? (i + 3546 - 1) : (-7777));
                button.downNeighborID = ((i < 3 && i < _currentlyDisplayedObjects.Count - 1) ? (i + 3546 + 1) : (-7777));

                if (i >= _currentlyDisplayedObjects.Count)
                {
                    if (button == base.currentlySnappedComponent)
                    {
                        base.currentlySnappedComponent = lastValidButton;
                        if (Game1.options.SnappyMenus)
                        {
                            base.snapCursorToCurrentSnappedComponent();
                        }
                    }
                }
                else
                {
                    lastValidButton = button;
                }
            }
        }


        private void RepositionTabs()
        {
            for (int i = 0; i < _tabButtons.Count; i++)
            {
                if (i == _currentTabIndex)
                {
                    _tabButtons[i].bounds.X = base.xPositionOnScreen - 56;
                }
                else
                {
                    _tabButtons[i].bounds.X = base.xPositionOnScreen - 64;
                }

                _tabButtons[i].bounds.Y = base.yPositionOnScreen + i * 16 * 4 + 16;
            }
        }

        private void SetTabFilter(int index)
        {
            _currentTabIndex = index;

            // Reset the currently object index
            _currentObjectIndex = 0;

            // Refresh view to filter based on the new tab
            _currentlyDisplayedObjects.Clear();
            foreach (var item in _displayableObjects)
            {
                switch (Enum.Parse(typeof(Filter), _tabButtons[_currentTabIndex].name))
                {
                    case Filter.None:
                        _currentlyDisplayedObjects.Add(item);
                        break;
                    case Filter.Tables:
                        if (item is Furniture && ((item as Furniture).furniture_type.Value == 5 || (item as Furniture).furniture_type.Value == 4 || (item as Furniture).furniture_type.Value == 11))
                        {
                            _currentlyDisplayedObjects.Add(item);
                        }
                        break;
                    case Filter.Chairs:
                        if (item is Furniture && ((item as Furniture).furniture_type.Value == 0 || (item as Furniture).furniture_type.Value == 1 || (item as Furniture).furniture_type.Value == 2 || (item as Furniture).furniture_type.Value == 3))
                        {
                            _currentlyDisplayedObjects.Add(item);
                        }
                        break;
                    case Filter.Pictures:
                        if (item is Furniture && ((item as Furniture).furniture_type.Value == 6 || (item as Furniture).furniture_type.Value == 13))
                        {
                            _currentlyDisplayedObjects.Add(item);
                        }
                        break;
                    case Filter.Rugs:
                        if (item is Furniture && (item as Furniture).furniture_type.Value == 12)
                        {
                            _currentlyDisplayedObjects.Add(item);
                        }
                        break;
                    case Filter.Decorations:
                        if (item is Furniture && ((item as Furniture).furniture_type.Value == 7 || (item as Furniture).furniture_type.Value == 17 || (item as Furniture).furniture_type.Value == 10 || (item as Furniture).furniture_type.Value == 8 || (item as Furniture).furniture_type.Value == 9 || (item as Furniture).furniture_type.Value == 14))
                        {
                            _currentlyDisplayedObjects.Add(item);
                        }
                        break;
                }
            }
        }

        private void SetTextFilter(string text)
        {
            _cachedTextValue = text;

            _currentlyDisplayedObjects = string.IsNullOrEmpty(text) ? _currentlyDisplayedObjects : _currentlyDisplayedObjects.Where(o => o.Name.Contains(text, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Escape && base.readyToClose())
            {
                base.exitThisMenu();
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = false)
        {
            if (Game1.activeClickableMenu is null)
            {
                return;
            }

            for (int k = 0; k < _tabButtons.Count; k++)
            {
                if (_tabButtons[k].containsPoint(x, y))
                {
                    SetTabFilter(k);
                    SetTextFilter(_searchBox.Text);
                    Game1.playSound("shwip");

                    if (Game1.options.snappyMenus && Game1.options.gamepadControls)
                    {
                        this.snapCursorToCurrentSnappedComponent();
                    }
                }
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);

            int offset = direction > 0 ? 1 : -1;
            _currentObjectIndex = Math.Max(0, _currentObjectIndex - offset);
            _currentObjectIndex = Math.Min(_currentObjectIndex, _currentlyDisplayedObjects.Count - PAGE_SIZE);
            //this.setScrollBarToCurrentIndex();
            UpdateSaleButtonNeighbors();
        }

        public override void update(GameTime time)
        {
            base.update(time);

            if (_searchBox.Text != _cachedTextValue)
            {
                SetTabFilter(_currentTabIndex);
                SetTextFilter(_searchBox.Text);
            }

            RepositionTabs();
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            SpriteText.drawStringWithScrollCenteredAt(b, AlternativeTextures.modHelper.Translation.Get("ui.labels.catalogue"), base.xPositionOnScreen + base.width / 6, base.yPositionOnScreen - 64);
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, Color.White, 4f);

            // Draw the tabs
            foreach (var tab in _tabButtons)
            {
                tab.draw(b);
            }

            // Draw the search box
            _searchBox.Draw(b);

            // Draw the display objects
            Texture2D purchase_texture = Game1.mouseCursors;
            Rectangle purchase_item_rect = new Rectangle(384, 396, 15, 15);
            Rectangle purchase_item_background = new Rectangle(296, 363, 18, 18);
            Color purchase_selected_color = Color.Wheat;
            for (int k = 0; k < _objectButtons.Count; k++)
            {
                if (_currentlyDisplayedObjects.Count == 0)
                {
                    break;
                }
                else if (_currentObjectIndex + k >= _currentlyDisplayedObjects.Count)
                {
                    continue;
                }

                IClickableMenu.drawTextureBox(b, purchase_texture, purchase_item_rect, _objectButtons[k].bounds.X, _objectButtons[k].bounds.Y, _objectButtons[k].bounds.Width, _objectButtons[k].bounds.Height, (_objectButtons[k].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY())) ? purchase_selected_color : Color.White, 4f, drawShadow: false);
                Object item = _currentlyDisplayedObjects[_currentObjectIndex + k];

                string displayName = item.DisplayName;
                if (item.ShouldDrawIcon())
                {
                    b.Draw(purchase_texture, new Vector2(_objectButtons[k].bounds.X + 32 - 12, _objectButtons[k].bounds.Y + 24 - 12), purchase_item_background, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                    item.drawInMenu(b, new Vector2(_objectButtons[k].bounds.X + 32 - 8, _objectButtons[k].bounds.Y + 24 - 8), 1f, 1f, 0.9f, StackDrawType.Draw_OneInclusive, Color.White, drawShadow: true);
                    SpriteText.drawString(b, displayName, _objectButtons[k].bounds.X + 96 + 8, _objectButtons[k].bounds.Y + 28, 999999, -1, 999999, 1f, 0.88f, junimoText: false, -1, "");
                }
            }

            Game1.mouseCursorTransparency = 1f;
            base.drawMouse(b);
        }
    }
}