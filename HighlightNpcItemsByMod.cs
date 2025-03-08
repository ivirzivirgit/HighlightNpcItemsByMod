using ExileCore2;
using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Elements;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared.Cache;
using ExileCore2.Shared.Helpers;
using ItemFilterLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using ExileCore2.Shared.Nodes;
using static HighlightNpcItemsByMod.ServerAndStashWindow;
using RectangleF = ExileCore2.Shared.RectangleF;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ImGuiNET;
using System.Text.RegularExpressions;

namespace HighlightNpcItemsByMod;

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
        public ServerInventory Inventory { get; set; }

        public override string ToString()
        {
            return $"Tab({Title}) is Index({Index}) IsVisible({IsVisible}) [ServerItems({ServerItems.Count}), TradeWindowItems({TradeWindowItems.Count})]";
        }
    }
}

public class HighlightNpcItemsByMod : BaseSettingsPlugin<HighlightNpcItemsByModSettings>
{
    private readonly CachedValue<List<WindowSet>> _storedStashAndWindows;
    private readonly CachedValue<List<CustomItemData>> _rewardItems;
    private readonly CachedValue<List<CustomItemData>> _ritualItems;
    private PurchaseWindow _purchaseWindowHideout;
    private PurchaseWindow _purchaseWindow;


    public HighlightNpcItemsByMod()
    {
        Name = "Highlight Npc Items By Mod";
        _storedStashAndWindows = new TimeCache<List<WindowSet>>(CacheUtils.RememberLastValue<List<WindowSet>>(UpdateCurrentTradeWindow), 50);
        _rewardItems = new TimeCache<List<CustomItemData>>(GetRewardItems, 1000);
        _ritualItems = new TimeCache<List<CustomItemData>>(GetRitualItems, 1000);
    }
    public override bool Initialise()
    {

        return true;
    }

    public override void Tick()
    {
        _purchaseWindowHideout = GameController.Game.IngameState.IngameUi.PurchaseWindowHideout;
        _purchaseWindow = GameController.Game.IngameState.IngameUi.PurchaseWindow;
    }

    public override void Render()
    {
        var hoveredItem = GetHoveredItem();
        ProcessInspectOnHover(hoveredItem);
        ProcessPurchaseWindow(hoveredItem);
        ProcessRewardsWindow(hoveredItem);
        ProcessRitualWindow(hoveredItem);
    }

    public override void DrawSettings()
    {
        ImGuiUtils.Checkbox($"Write Hovered Item Mod Names On Debug Window", "Hover Item for Inspect Mod Names", ref Settings.InspectHoverItemMods);

        ImGuiUtils.Checkbox($"Draw On Tab Labes", "Draw On Tab Labes", ref Settings.DrawOnTabLabels);

        if (ImGuiUtils.CollapsingHeader("Highlighted ItemMods", ref Settings.HighLightRulesHeaderOpen))
        {
            ImGui.Indent();
            for (int i = 0; i < Settings.HighLightRules.Count; i++) { DrawNPCInvRules(Settings.HighLightRules[i], i); }
            if (ImGui.Button("Add ItemMod")) { Settings.AddRule(); }
            ImGui.Unindent();
        }
    }

    private void DrawNPCInvRules(HighLightRule ruleSettings, int index)
    {
        ImGuiUtils.Checkbox($"##EnableRule{index}", "Enable Rule", ref ruleSettings.Enabled); ImGui.SameLine();
        ImGuiUtils.ColorSwatch($"Frame Color ##{index}", ref ruleSettings.Color); ImGui.SameLine();
        ImGui.PushItemWidth(200);
        ImGui.InputText($"##Item Mod Name{index}", ref ruleSettings.ModName, 100); ImGui.SameLine();
        ImGui.InputInt($"##At Least Mod Tier{index}", ref ruleSettings.AtLeastTier, 1); ImGui.SameLine();
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("ItemModName");
            ImGui.EndTooltip();
        };
        if (ImGui.Button($"Remove##{index}")) Settings.RemoveRule(index);

    }
    private void ProcessInspectOnHover(Element hoveredItem)
    {

        if (Settings.InspectHoverItemMods && hoveredItem != null)
        {
            var hoveredItemData = new ItemData(hoveredItem.Entity, GameController);
            string hoveredItemText = "";
            foreach (var modList in hoveredItemData.ModsInfo.ModsDictionary.Keys.ToList())
            {
                foreach (var mod in modList)
                {
                    hoveredItemText += mod.RawName + " \n";
                }
            }
            DebugWindow.LogMsg($"item Mods on hover: {hoveredItemData.Name}\n{hoveredItemText}");
        }
    }

    private void ProcessRewardsWindow(Element hoveredItem)
    {
        if (!GameController.IngameState.IngameUi.QuestRewardWindow.IsVisible) return;

        foreach (var reward in _rewardItems?.Value ?? [])
        {
            if (IsMatched(reward, Settings.HighLightRules) is { } color)
            {
                var frameColor = color;
                if (hoveredItem != null && hoveredItem.Tooltip.GetClientRectCache.Intersects(reward.ClientRectangle) && hoveredItem.Entity.Address != reward.Entity.Address)
                {
                    frameColor = new Vector4(.5f, .5f, .5f, 0);
                }

                Graphics.DrawFrame(reward.ClientRectangle, ImGuiUtils.Vector4ToColor(frameColor), Settings.FrameThickness);
            }
        }
    }
    private void ProcessRitualWindow(Element hoveredItem)
    {
        if (!GameController.IngameState.IngameUi.RitualWindow.IsVisible) return;

        foreach (var reward in _ritualItems?.Value ?? [])
        {
            if (IsMatched(reward, Settings.HighLightRules) is { } color)
            {
                var frameColor = color;
                if (hoveredItem != null && hoveredItem.Tooltip.GetClientRectCache.Intersects(reward.ClientRectangle) && hoveredItem.Entity.Address != reward.Entity.Address)
                {
                    frameColor = new Vector4(.5f, .5f, .5f, 0);
                }

                Graphics.DrawFrame(reward.ClientRectangle, ImGuiUtils.Vector4ToColor(frameColor), Settings.FrameThickness);
            }
        }
    }

    private List<CustomItemData> GetRewardItems() =>
        GameController.IngameState.IngameUi.QuestRewardWindow.GetPossibleRewards()
            .Where(item => item.Item2 is { Address: not 0, IsValid: true })
            .Select(item => new CustomItemData(item.Item1, GameController, EKind.QuestReward, item.Item2.GetClientRectCache))
            .ToList();

    private List<CustomItemData> GetRitualItems() =>
        GameController.IngameState.IngameUi.RitualWindow.InventoryElement.VisibleInventoryItems
            .Where(item => item.Item is { Address: not 0, IsValid: true })
            .Select(item => new CustomItemData(item.Item, GameController, EKind.RitualReward, item.GetClientRectCache))
            .ToList();

    private void ProcessPurchaseWindow(Element hoveredItem)
    {
        if (!IsPurchaseWindowVisible())
            return;

        List<string> unSeenItems = [];
        ProcessStoredTabs(unSeenItems, hoveredItem);

        PurchaseWindow purchaseWindowItems = GetVisiblePurchaseWindow();
        var serverItemsBox = CalculateServerItemsBox(unSeenItems, purchaseWindowItems);

        DrawServerItems(serverItemsBox, unSeenItems, hoveredItem);
    }

    private void DrawServerItems(RectangleF serverItemsBox, List<string> unSeenItems, Element hoveredItem)
    {
        if (hoveredItem == null || !hoveredItem.Tooltip.GetClientRectCache.Intersects(serverItemsBox))
        {
            var boxColor = Color.FromArgb(150, 0, 0, 0);
            var textColor = Color.FromArgb(230, 255, 255, 255);

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
        return GameController.IngameState.UIHover is { Address: not 0, Entity.IsValid: true } hover ? hover : null;
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
        foreach (var visibleItem in items.Where(item => item != null))
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
            if (IsMatched(hiddenItem, Settings.HighLightRules) != null)
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
        if (IsMatched(item, Settings.HighLightRules) is { } color)
        {
            var frameColor = color;
            if (hoveredItem != null && hoveredItem.Tooltip.GetClientRectCache.Intersects(item.ClientRectangle) && hoveredItem.Entity.Address != item.Entity.Address)
            {
                frameColor = new Vector4(.5f, .5f, .5f, 0);
            }

            Graphics.DrawFrame(item.ClientRectangle, ImGuiUtils.Vector4ToColor(frameColor), Settings.FrameThickness);
        }
    }

    private void DrawTabNameElementFrame(Element tabNameElement, Element hoveredItem)
    {
        var frameColor = Settings.TabFrameColor;
        if (hoveredItem == null || !hoveredItem.Tooltip.GetClientRectCache.Intersects(tabNameElement.GetClientRectCache))
        {
            Graphics.DrawFrame(tabNameElement.GetClientRectCache, frameColor, Settings.FrameThickness);
        }
        else
        {
            Graphics.DrawFrame(tabNameElement.GetClientRectCache, frameColor.Value.ToImguiVec4(45).ToColor(), Settings.FrameThickness);
        }
    }

    private RectangleF CalculateServerItemsBox(List<string> unSeenItems, PurchaseWindow purchaseWindowItems)
    {
        var startingPoint = purchaseWindowItems.TabContainer.GetClientRectCache.TopRight;
        startingPoint.X += 15;

        var longestText = unSeenItems.MaxBy(s => s.Length);
        var textHeight = Graphics.MeasureText(longestText);
        var textPadding = 10;

        return new RectangleF
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

    private List<WindowSet> UpdateCurrentTradeWindow(List<WindowSet> previousValue)
    {
        var previousDict = previousValue?.ToDictionary(x => (x.Title, x.Inventory.Address, x.Inventory.ServerRequestCounter, x.IsVisible, x.TradeWindowItems.Count));
        var purchaseWindowItems = (_purchaseWindowHideout, _purchaseWindow) switch
        {
            ({ IsVisible: true } w, _) => w,
            (_, { IsVisible: true } w) => w,
            _ => null,
        };

        if (purchaseWindowItems == null)
            return [];

        return purchaseWindowItems.TabContainer.Inventories.Select((inventory, i) =>
        {
            var uiInventory = inventory?.Inventory;
            if (uiInventory == null) return null;
            var serverInventory = uiInventory.ServerInventory;
            if (serverInventory == null)
            {
                DebugWindow.LogError($"Server inventory for ui inventory {uiInventory} ({uiInventory.InvType}) is missing");
            }

            var isVisible = uiInventory.IsVisible;
            var visibleValidUiItems = uiInventory.VisibleInventoryItems
                .Where(x => x.Item?.Path != null).ToList();
            var title = $"-{i + 1}-";
            if (previousDict?.TryGetValue((title, serverInventory?.Address ?? 0, serverInventory?.ServerRequestCounter ?? -1, isVisible, visibleValidUiItems.Count),
                    out var previousSet) == true)
            {
                return previousSet;
            }

            var newTab = new WindowSet
            {
                Inventory = serverInventory,
                Index = i,
                ServerItems = serverInventory?.Items.Where(x => x?.Path != null).Select(x => new CustomItemData(x, GameController, EKind.Shop)).ToList() ?? [],
                TradeWindowItems = visibleValidUiItems
                    .Select(x => new CustomItemData(x.Item, GameController, EKind.Shop, x.GetClientRectCache))
                    .ToList(),
                Title = title,
                IsVisible = isVisible,
                TabNameElement = inventory.TabButton
            };

            return newTab;
        }).Where(x => x != null).ToList();
    }

    public Vector4? IsMatched(CustomItemData item, List<HighLightRule> ruleSettings)
    {
        if (item.ModsInfo.ModsDictionary.Count() > 0)
        {
            foreach (var modList in item.ModsInfo.ModsDictionary.Keys.ToList())
            {
                foreach (var mod in modList)
                {
                    var rules = Settings.HighLightRules.Where(x => x.Enabled);
                    if (rules.Count() > 0) {
                        foreach (var rule in rules)
                        {
                            if (mod.RawName.Contains(rule.ModName))
                            {
                                if (rule.AtLeastTier >= 0)
                                {
                                    string tierFromText = Regex.Match(mod.RawName, @"\d+", RegexOptions.RightToLeft)?.Value;
                                    int tier = string.IsNullOrEmpty(tierFromText) ? 0 : int.Parse(tierFromText);
                                    if (tier > 0 && tier >= rule.AtLeastTier)
                                    {
                                        return rule.Color;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                return rule.Color;
                            }
                        } 
                    }
                }
            }
        }
        return null;
    }
}
