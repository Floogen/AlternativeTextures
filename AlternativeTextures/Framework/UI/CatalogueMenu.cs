using AlternativeTextures.Framework.Models;
using AlternativeTextures.Framework.Patches;
using AlternativeTextures.Framework.Utilities;
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
using static AlternativeTextures.Framework.Models.AlternativeTextureModel;
using static StardewValley.Minigames.TargetGame;
using Object = StardewValley.Object;

namespace AlternativeTextures.Framework.UI
{
    internal class CatalogueMenu : IClickableMenu
    {
        private List<Object> _displayableObjects;
        private List<Object> _currentlyDisplayedObjects;
        private List<Item> _displayableTextures;
        private List<Item> _currentlyDisplayedTextures;

        private const int PAGE_SIZE = 5;

        private int _currentTabIndex;
        private int _currentObjectIndex;
        private int _currentButtonNeighborID;

        private int _startingRow = 0;
        private int _texturesPerRow = 6;
        private int _maxRows = 4;

        private string _selectedObjectName;
        private bool _isDisplayingAlternativeTextures;

        private TextBox _searchBox;
        private string _previousTextValue;
        private string _cachedTextValue;

        private List<ClickableTextureComponent> _tabButtons;
        private List<ClickableComponent> _objectButtons;
        private List<ClickableTextureComponent> _alternativeTextureButtons = new List<ClickableTextureComponent>();

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

            // Establish the texture lists
            _displayableTextures = new List<Item>();
            _currentlyDisplayedTextures = new List<Item>();

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
            _startingRow = 0;

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
            _previousTextValue = text;

            _currentlyDisplayedObjects = string.IsNullOrEmpty(text) ? _currentlyDisplayedObjects : _currentlyDisplayedObjects.Where(o => o.Name.Contains(text, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void SetSelectedObjected(Object selectedObject)
        {
            _startingRow = 0;
            _selectedObjectName = selectedObject is null ? string.Empty : selectedObject.Name;

            if (string.IsNullOrEmpty(_selectedObjectName)) 
            {
                // Restore the old search value, if any
                _searchBox.Text = _cachedTextValue;
                _previousTextValue = string.Empty;

                return;
            }
            else
            {
                _cachedTextValue = _searchBox.Text;
                _searchBox.Text = string.Empty;
                _previousTextValue = string.Empty;
            }

            // Get the textures available
            string modelName = $"{AlternativeTextureModel.TextureType.Furniture}_{_selectedObjectName}";
            var availableModels = AlternativeTextures.textureManager.GetAvailableTextureModels(modelName, Game1.GetSeasonForLocation(Game1.currentLocation));

            _displayableTextures = new List<Item>();
            _currentlyDisplayedTextures = new List<Item>();
            for (int m = 0; m < availableModels.Count; m++)
            {
                var manualVariations = availableModels[m].ManualVariations.Where(v => v.Id != -1).ToList();
                if (manualVariations.Count() > 0)
                {
                    for (int v = 0; v < manualVariations.Count(); v++)
                    {
                        var objectWithVariation = selectedObject.getOne();
                        objectWithVariation.modData[ModDataKeys.ALTERNATIVE_TEXTURE_OWNER] = availableModels[m].Owner;
                        objectWithVariation.modData[ModDataKeys.ALTERNATIVE_TEXTURE_NAME] = availableModels[m].GetId();
                        objectWithVariation.modData[ModDataKeys.ALTERNATIVE_TEXTURE_VARIATION] = manualVariations[v].Id.ToString();
                        objectWithVariation.modData[ModDataKeys.ALTERNATIVE_TEXTURE_SEASON] = availableModels[m].Season;
                        objectWithVariation.modData[ModDataKeys.ALTERNATIVE_TEXTURE_DISPLAY_NAME] = manualVariations[v].Name;

                        if (AlternativeTextures.modConfig.IsTextureVariationDisabled(objectWithVariation.modData[ModDataKeys.ALTERNATIVE_TEXTURE_NAME], manualVariations[v].Id))
                        {
                            continue;
                        }

                        if (selectedObject is Furniture furniture)
                        {
                            (objectWithVariation as Furniture).currentRotation.Value = furniture.currentRotation.Value;
                            (objectWithVariation as Furniture).updateRotation();
                        }

                        _currentlyDisplayedTextures.Add(objectWithVariation);
                        _displayableTextures.Add(objectWithVariation);
                    }
                }
                else
                {
                    for (int v = 0; v < availableModels[m].Variations; v++)
                    {
                        var objectWithVariation = selectedObject.getOne();
                        objectWithVariation.modData[ModDataKeys.ALTERNATIVE_TEXTURE_OWNER] = availableModels[m].Owner;
                        objectWithVariation.modData[ModDataKeys.ALTERNATIVE_TEXTURE_NAME] = availableModels[m].GetId();
                        objectWithVariation.modData[ModDataKeys.ALTERNATIVE_TEXTURE_VARIATION] = v.ToString();
                        objectWithVariation.modData[ModDataKeys.ALTERNATIVE_TEXTURE_SEASON] = availableModels[m].Season;
                        objectWithVariation.modData[ModDataKeys.ALTERNATIVE_TEXTURE_DISPLAY_NAME] = String.Empty;

                        if (AlternativeTextures.modConfig.IsTextureVariationDisabled(objectWithVariation.modData[ModDataKeys.ALTERNATIVE_TEXTURE_NAME], v))
                        {
                            continue;
                        }

                        if (selectedObject is Furniture furniture)
                        {
                            (objectWithVariation as Furniture).currentRotation.Value = furniture.currentRotation.Value;
                            (objectWithVariation as Furniture).updateRotation();
                        }

                        _currentlyDisplayedTextures.Add(objectWithVariation);
                        _displayableTextures.Add(objectWithVariation);
                    }
                }
            }

            // Establish the alternative texture buttons
            _alternativeTextureButtons = new List<ClickableTextureComponent>();

            var sourceRect = PaintBucketMenu.GetSourceRectangle(availableModels.First(), selectedObject, availableModels.First().TextureWidth, availableModels.First().TextureHeight, -1);
            if (sourceRect.Height >= 64)
            {
                _maxRows = 2;
            }
            else if (sourceRect.Height >= 32)
            {
                _maxRows = 3;
                _texturesPerRow = 6;
            }
            else if (sourceRect.Height <= 16)
            {
                sourceRect.Height = 32;
            }
            for (int r = 0; r < _maxRows; r++)
            {
                for (int c = 0; c < _texturesPerRow; c++)
                {
                    var componentId = c + r * _texturesPerRow;
                    _alternativeTextureButtons.Add(new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth + 32 + componentId % _texturesPerRow * 128, base.yPositionOnScreen + sourceRect.Height + 32 + componentId / _texturesPerRow * (4 * sourceRect.Height), 4 * sourceRect.Width, 4 * sourceRect.Height), availableModels.First().GetTexture(0), new Rectangle(), 4f, false)
                    {
                        myID = componentId,
                        downNeighborID = componentId + _texturesPerRow,
                        upNeighborID = r >= _texturesPerRow ? componentId - _texturesPerRow : -1,
                        rightNeighborID = c == 5 ? 9997 : componentId + 1,
                        leftNeighborID = c > 0 ? componentId - 1 : 9998
                    });
                }
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Escape && base.readyToClose())
            {
                if (_isDisplayingAlternativeTextures)
                {
                    SetSelectedObjected(null);
                }
                else
                {
                    base.exitThisMenu();
                }
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = false)
        {
            if (Game1.activeClickableMenu is null)
            {
                return;
            }

            // Handle tab buttons
            for (int k = 0; k < _tabButtons.Count; k++)
            {
                if (_tabButtons[k].containsPoint(x, y))
                {
                    SetTabFilter(k);
                    SetTextFilter(_searchBox.Text);
                    SetSelectedObjected(null);
                    Game1.playSound("shwip");

                    if (Game1.options.snappyMenus && Game1.options.gamepadControls)
                    {
                        this.snapCursorToCurrentSnappedComponent();
                    }
                }
            }

            // Handle object buttons
            for (int k = 0; k < _objectButtons.Count; k++)
            {
                if (_isDisplayingAlternativeTextures is true || _currentlyDisplayedObjects.Count == 0 || _currentObjectIndex + k >= _currentlyDisplayedObjects.Count || !_objectButtons[k].containsPoint(x, y))
                {
                    continue;
                }

                SetSelectedObjected(_currentlyDisplayedObjects[_currentObjectIndex + k]);
                break;
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);

            int offset = direction > 0 ? 1 : -1;

            _currentObjectIndex = Math.Max(0, _currentObjectIndex - offset);
            _currentObjectIndex = Math.Min(_currentObjectIndex, _currentlyDisplayedObjects.Count - PAGE_SIZE);

            if (direction > 0 && _startingRow > 0)
            {
                _startingRow--;
            }
            else if (direction < 0 && (_maxRows + _startingRow) * _texturesPerRow < _currentlyDisplayedTextures.Count)
            {
                _startingRow++;
            }

            Game1.playSound("shiny4");

            //this.setScrollBarToCurrentIndex();
            UpdateSaleButtonNeighbors();
        }

        public override void update(GameTime time)
        {
            base.update(time);

            // Handle search box changes
            if (_searchBox.Text != _previousTextValue)
            {
                SetTabFilter(_currentTabIndex);
                SetTextFilter(_searchBox.Text);
            }

            // Handle adjusting the tabs, if needed
            RepositionTabs();

            // Change display if an object has been selected to view textures
            _isDisplayingAlternativeTextures = string.IsNullOrEmpty(_selectedObjectName) is false;
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, Color.White, 4f);

            // Draw the tabs
            foreach (var tab in _tabButtons)
            {
                tab.draw(b);
            }

            // Draw the search box
            _searchBox.Draw(b);

            string titleBarText = AlternativeTextures.modHelper.Translation.Get("ui.labels.catalogue");
            if (_isDisplayingAlternativeTextures)
            {
                titleBarText = $"{titleBarText} > {_selectedObjectName}";

                for (int i = 0; i < _alternativeTextureButtons.Count; i++)
                {
                    _alternativeTextureButtons[i].item = null;
                    _alternativeTextureButtons[i].texture = null;

                    var textureIndex = i + _startingRow * _texturesPerRow;
                    if (textureIndex < _currentlyDisplayedTextures.Count)
                    {
                        _alternativeTextureButtons[i].item = _currentlyDisplayedTextures[textureIndex];
                        _alternativeTextureButtons[i].item.drawInMenu(b, new Vector2(_alternativeTextureButtons[i].bounds.X, _alternativeTextureButtons[i].bounds.Y + 96f), 2f, 1f, 0.87f, StackDrawType.Hide, Color.White, false);
                    }
                }
            }
            else
            {
                // Draw the display objects
                Texture2D purchaseTexture = Game1.mouseCursors;
                Rectangle purchaseTextureRectangle = new Rectangle(384, 396, 15, 15);
                Rectangle purchaseTextureBackground = new Rectangle(296, 363, 18, 18);
                Color purchaseTextColor = Color.Wheat;
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

                    IClickableMenu.drawTextureBox(b, purchaseTexture, purchaseTextureRectangle, _objectButtons[k].bounds.X, _objectButtons[k].bounds.Y, _objectButtons[k].bounds.Width, _objectButtons[k].bounds.Height, (_objectButtons[k].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY())) ? purchaseTextColor : Color.White, 4f, drawShadow: false);
                    Object item = _currentlyDisplayedObjects[_currentObjectIndex + k];

                    string displayName = item.DisplayName;
                    if (item.ShouldDrawIcon())
                    {
                        b.Draw(purchaseTexture, new Vector2(_objectButtons[k].bounds.X + 32 - 12, _objectButtons[k].bounds.Y + 24 - 12), purchaseTextureBackground, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                        item.drawInMenu(b, new Vector2(_objectButtons[k].bounds.X + 32 - 8, _objectButtons[k].bounds.Y + 24 - 8), 1f, 1f, 0.9f, StackDrawType.Hide, Color.White, drawShadow: true);
                        Utility.drawTinyDigits(item.Stack, b, new Vector2(_objectButtons[k].bounds.X + 32 - 8, _objectButtons[k].bounds.Y + 24 - 8) + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(item.Stack, 3f)) + 4f, 64f - 18f + 2f), 3f, 1f, Color.White);
                        SpriteText.drawString(b, displayName, _objectButtons[k].bounds.X + 96 + 8, _objectButtons[k].bounds.Y + 28, 999999, -1, 999999, 1f, 0.88f, junimoText: false, -1, "");
                    }
                }
            }
            SpriteText.drawStringWithScrollCenteredAt(b, titleBarText, base.xPositionOnScreen + SpriteText.getWidthOfString(titleBarText) / 2, base.yPositionOnScreen - 64);

            Game1.mouseCursorTransparency = 1f;
            base.drawMouse(b);
        }
    }
}