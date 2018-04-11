﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.Graphics.Gui;
using Alex.Graphics.Gui.Rendering;
using Microsoft.Xna.Framework;

namespace Alex.Gamestates.Gui
{
    public class GuiItemHotbar : GuiContainer
    {
        private const int ItemWidth = 20;

        public override int Width => 180;
        public override int Height => 20;

        private int _selectedIndex = 0;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value >= 9)
                {
                    value = 0;
                }

                if (value < 0)
                {
                    value = 8;
                }

                _selectedIndex = value;
                OnSelectedIndexChanged();
            }
        }

        private void OnSelectedIndexChanged()
        {
            var items = Children.OfType<GuiInventoryItem>().ToArray();
            foreach (var guiInventoryItem in items)
            {
                guiInventoryItem.IsSelected = false;
            }

            items[SelectedIndex].IsSelected = true;
        }

        public GuiItemHotbar()
        {
            DebugColor = Color.Blue;
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            Background = renderer.GetTexture(GuiTextures.Inventory_HotBar);

            for (int i = 0; i < 9; i++)
            {
                AddChild(new GuiInventoryItem()
                {
                    X = i * ItemWidth,
                    IsSelected = i == SelectedIndex
                });
            }
        }
    }
}