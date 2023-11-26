﻿using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Helpers;
using ImGuiNET;
using ItemFilterLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using static NPCInvWithLinq.ServerAndStashWindow;

namespace NPCInvWithLinq
{
    public class ServerAndStashWindow
    {
        public IList<WindowSet> Tabs { get; set; }

        public class WindowSet
        {
            public int Index { get; set; }
            public string Title { get; set; }
            public bool IsVisible { get; set; }
            public Element TabNameElement { get; set; }
            public List<CustomItemData> ServerItems { get; set; }
            public List<CustomItemData> TradeWindowItems { get; set; }

            public override string ToString()
            {
                return $"Tab({Title}) is Index({Index}) IsVisible({IsVisible}) [ServerItems({ServerItems.Count}), TradeWindowItems({TradeWindowItems.Count})]";
            }
        }
    }

    public class NPCInvWithLinq : BaseSettingsPlugin<NPCInvWithLinqSettings>
    {
        private readonly TimeCache<List<WindowSet>> _storedStashAndWindows;
        private List<ItemFilter> _itemFilters;
        private PurchaseWindow _purchaseWindowHideout;
        private PurchaseWindow _purchaseWindow;
        private IList<InventoryHolder> _npcInventories;

        public NPCInvWithLinq()
        {
            Name = "NPC Inv With Linq";
            _storedStashAndWindows = new TimeCache<List<WindowSet>>(UpdateCurrentTradeWindow, 50);
        }

        public override bool Initialise()
        {
            Settings.ReloadFilters.OnPressed = LoadRuleFiles;
            LoadRuleFiles();
            return true;
        }

        public override Job Tick()
        {
            _purchaseWindowHideout = GameController.Game.IngameState.IngameUi.PurchaseWindowHideout;
            _purchaseWindow = GameController.Game.IngameState.IngameUi.PurchaseWindow;
            _npcInventories = GameController.Game.IngameState.ServerData.NPCInventories;

            return null;
        }

        public override void Render()
        {
            var hoveredItem = GetHoveredItem();
            if (!IsPurchaseWindowVisible())
                return;

            List<string> unSeenItems = new List<string>();
            ProcessStoredTabs(unSeenItems, hoveredItem);

            PurchaseWindow purchaseWindowItems = GetVisiblePurchaseWindow();
            var serverItemsBox = CalculateServerItemsBox(unSeenItems, purchaseWindowItems);

            DrawServerItems(serverItemsBox, unSeenItems, hoveredItem);

            PerformItemFilterTest(hoveredItem);
        }

        private void DrawServerItems(SharpDX.RectangleF serverItemsBox, List<string> unSeenItems, Element hoveredItem)
        {
            if (hoveredItem == null || !hoveredItem.Tooltip.GetClientRectCache.Intersects(serverItemsBox))
            {
                var boxColor = new SharpDX.Color(0, 0, 0, 150);
                var textColor = new SharpDX.Color(255, 255, 255, 230);

                Graphics.DrawBox(serverItemsBox, boxColor);

                for (int i = 0; i < unSeenItems.Count; i++)
                {
                    string stringItem = unSeenItems[i];
                    var textHeight = Graphics.MeasureText(stringItem);
                    var textPadding = 10;

                    Graphics.DrawText(stringItem, new Vector2(serverItemsBox.X + textPadding, serverItemsBox.Y + (textHeight.Y * i)), textColor);
                }
            }
        }

        private Element GetHoveredItem()
        {
            return GameController.IngameState.UIHover?.Address != 0 && GameController.IngameState.UIHover.Entity.IsValid
                ? GameController.IngameState.UIHover
                : null;
        }

        private bool IsPurchaseWindowVisible()
        {
            return _purchaseWindowHideout.IsVisible || _purchaseWindow.IsVisible;
        }

        private void ProcessStoredTabs(List<string> unSeenItems, Element hoveredItem)
        {
            foreach (var storedTab in _storedStashAndWindows.Value)
            {
                if (storedTab.IsVisible)
                    ProcessVisibleTabItems(storedTab.TradeWindowItems, hoveredItem);
                else
                    ProcessHiddenTabItems(storedTab, unSeenItems, hoveredItem);
            }
        }

        private void ProcessVisibleTabItems(IEnumerable<CustomItemData> items, Element hoveredItem)
        {
            foreach (var visibleItem in items.Where(item => item != null && ItemInFilter(item)))
            {
                DrawItemFrame(visibleItem, hoveredItem);
            }
        }

        private void ProcessHiddenTabItems(WindowSet storedTab, List<string> unSeenItems, Element hoveredItem)
        {
            var tabHadWantedItem = false;
            foreach (var hiddenItem in storedTab.ServerItems)
            {
                if (hiddenItem == null) continue;
                if (ItemInFilter(hiddenItem))
                {
                    ProcessUnseenItems(unSeenItems, storedTab, hoveredItem);
                    unSeenItems.Add($"\t{hiddenItem.Name}");
                    tabHadWantedItem = true;
                }
            }
            if (tabHadWantedItem)
                unSeenItems.Add("");
        }

        private void ProcessUnseenItems(List<string> unSeenItems, WindowSet storedTab, Element hoveredItem)
        {
            if (!unSeenItems.Contains($"Tab [{storedTab.Title}]"))
            {
                unSeenItems.Add($"Tab [{storedTab.Title}]");
                if (Settings.DrawOnTabLabels)
                {
                    DrawTabNameElementFrame(storedTab.TabNameElement, hoveredItem);
                }
            }
        }

        private void DrawItemFrame(CustomItemData item, Element hoveredItem)
        {
            if (hoveredItem != null && hoveredItem.Tooltip.GetClientRectCache.Intersects(item.ClientRectangleCache) && hoveredItem.Entity.Address != item.Entity.Address)
            {
                Graphics.DrawFrame(item.ClientRectangleCache, Settings.FrameColor.Value with { A = 45 }, Settings.FrameThickness);
            }
            else
            {
                Graphics.DrawFrame(item.ClientRectangleCache, Settings.FrameColor, Settings.FrameThickness);
            }
        }

        private void DrawTabNameElementFrame(Element tabNameElement, Element hoveredItem)
        {
            if (hoveredItem == null || !hoveredItem.Tooltip.GetClientRectCache.Intersects(tabNameElement.GetClientRectCache))
            {
                Graphics.DrawFrame(tabNameElement.GetClientRectCache, Settings.FrameColor, Settings.FrameThickness);
            }
            else
            {
                Graphics.DrawFrame(tabNameElement.GetClientRectCache, Settings.FrameColor.Value with { A = 45 }, Settings.FrameThickness);
            }
        }

        private SharpDX.RectangleF CalculateServerItemsBox(List<string> unSeenItems, PurchaseWindow purchaseWindowItems)
        {
            var startingPoint = purchaseWindowItems.TabContainer.GetClientRectCache.TopRight.ToVector2Num();
            startingPoint.X += 15;

            var longestText = unSeenItems.OrderByDescending(s => s.Length).FirstOrDefault();
            var textHeight = Graphics.MeasureText(longestText);
            var textPadding = 10;

            return new SharpDX.RectangleF
            {
                Height = textHeight.Y * unSeenItems.Count,
                Width = textHeight.X + (textPadding * 2),
                X = startingPoint.X,
                Y = startingPoint.Y
            };
        }

        private PurchaseWindow GetVisiblePurchaseWindow()
        {
            return _purchaseWindowHideout.IsVisible ? _purchaseWindowHideout : _purchaseWindow.IsVisible ? _purchaseWindow : null;
        }

        private void PerformItemFilterTest(Element hoveredItem)
        {
            if (Settings.FilterTest.Value is { Length: > 0 } && hoveredItem != null)
            {
                var filter = ItemFilter.LoadFromString(Settings.FilterTest);
                var matched = filter.Matches(new ItemData(hoveredItem.Entity, GameController));
                DebugWindow.LogMsg($"Debug item match on hover: {matched}");
            }
        }

        public record FilterDirItem(string Name, string Path);

        public override void DrawSettings()
        {
            base.DrawSettings();

            if (ImGui.Button("Open Build Folder"))
            {
                var configDir = ConfigDirectory;
                var customConfigFileDirectory = !string.IsNullOrEmpty(Settings.CustomConfigDir)
                    ? Path.Combine(Path.GetDirectoryName(ConfigDirectory), Settings.CustomConfigDir)
                    : null;

                var directoryToOpen = Directory.Exists(customConfigFileDirectory)
                    ? customConfigFileDirectory
                    : configDir;

                Process.Start("explorer.exe", directoryToOpen);
            }

            ImGui.Separator();
            ImGui.BulletText("Select Rules To Load");
            ImGui.BulletText("Ordering rule sets so general items will match first rather than last will improve performance");

            var tempNPCInvRules = new List<NPCInvRule>(Settings.NPCInvRules); // Create a copy

            for (int i = 0; i < tempNPCInvRules.Count; i++)
            {
                if (ImGui.ArrowButton($"##upButton{i}", ImGuiDir.Up) && i > 0)
                    (tempNPCInvRules[i - 1], tempNPCInvRules[i]) = (tempNPCInvRules[i], tempNPCInvRules[i - 1]);

                ImGui.SameLine(); ImGui.Text(" "); ImGui.SameLine();

                if (ImGui.ArrowButton($"##downButton{i}", ImGuiDir.Down) && i < tempNPCInvRules.Count - 1)
                    (tempNPCInvRules[i + 1], tempNPCInvRules[i]) = (tempNPCInvRules[i], tempNPCInvRules[i + 1]);

                ImGui.SameLine(); ImGui.Text(" - "); ImGui.SameLine();

                var refToggle = tempNPCInvRules[i].Enabled;
                if (ImGui.Checkbox($"{tempNPCInvRules[i].Name}##checkbox{i}", ref refToggle))
                    tempNPCInvRules[i].Enabled = refToggle;
            }

            Settings.NPCInvRules = tempNPCInvRules;
        }

        private void LoadRuleFiles()
        {
            var pickitConfigFileDirectory = ConfigDirectory;
            var existingRules = Settings.NPCInvRules;

            if (!string.IsNullOrEmpty(Settings.CustomConfigDir))
            {
                var customConfigFileDirectory = Path.Combine(Path.GetDirectoryName(ConfigDirectory), Settings.CustomConfigDir);

                if (Directory.Exists(customConfigFileDirectory))
                {
                    pickitConfigFileDirectory = customConfigFileDirectory;
                }
                else
                {
                    DebugWindow.LogError("[NPC Inventory] custom config folder does not exist.", 15);
                }
            }

            try
            {
                var newRules = new DirectoryInfo(pickitConfigFileDirectory).GetFiles("*.ifl")
                    .Select(x => new NPCInvRule(x.Name, Path.GetRelativePath(pickitConfigFileDirectory, x.FullName), false))
                    .ExceptBy(existingRules.Select(x => x.Location), x => x.Location)
                    .ToList();
                foreach (var groundRule in existingRules)
                {
                    var fullPath = Path.Combine(pickitConfigFileDirectory, groundRule.Location);
                    if (File.Exists(fullPath))
                    {
                        newRules.Add(groundRule);
                    }
                    else
                    {
                        LogError($"File '{groundRule.Name}' not found.");
                    }
                }

                _itemFilters = newRules
                    .Where(rule => rule.Enabled)
                    .Select(rule => ItemFilter.LoadFromPath(Path.Combine(pickitConfigFileDirectory, rule.Location)))
                    .ToList();

                Settings.NPCInvRules = newRules;
            }
            catch (Exception e)
            {
                LogError($"An error occurred while loading rule files: {e.Message}");
            }
        }

        private List<WindowSet> UpdateCurrentTradeWindow()
        {
            if (_purchaseWindowHideout == null || _purchaseWindow == null)
                return new List<WindowSet>();

            var purchaseWindowItems = GetVisiblePurchaseWindow();

            if (purchaseWindowItems == null)
                return new List<WindowSet>();

            return _npcInventories.Select((inventory, i) =>
            {
                var newTab = new WindowSet
                {
                    Index = i,
                    ServerItems = inventory.Inventory.Items.Where(x => x?.Path != null).Select(x => new CustomItemData(x, GameController)).ToList(),
                    TradeWindowItems = purchaseWindowItems.TabContainer.AllInventories[i].VisibleInventoryItems
                        .Where(x => x.Item?.Path != null)
                        .Select(x => new CustomItemData(x.Item, GameController, x.GetClientRectCache))
                        .ToList(),
                    Title = $"-{i + 1}-",
                    IsVisible = purchaseWindowItems.TabContainer.AllInventories[i].IsVisible
                };

                newTab.TabNameElement = purchaseWindowItems.TabContainer.TabSwitchBar.Children
                    .FirstOrDefault(x => x?.GetChildAtIndex(0)?.GetChildAtIndex(1)?.Text == newTab.Title);

                return newTab;
            }).ToList();
        }

        private bool ItemInFilter(ItemData item)
        {
            return _itemFilters?.Any(filter => filter.Matches(item)) ?? false;
        }
    }
}