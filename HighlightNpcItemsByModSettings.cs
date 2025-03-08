using ExileCore2.Shared.Attributes;
using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.Json.Serialization;
using System.Numerics;
using ImGuiNET;
using System.Collections;

namespace HighlightNpcItemsByMod;

public sealed class HighlightNpcItemsByModSettings : ISettings
{
    public HighlightNpcItemsByModSettings()
    {
        RuleConfig = new RuleRenderer(this);
    }    

    public ToggleNode Enable { get; set; } = new ToggleNode(false);

    public bool HighLightRulesHeaderOpen = true;
    public bool DrawOnTabLabels = true;

    public bool InspectHoverItemMods = false;    

    public ColorNode TabFrameColor { get; set; } = new ColorNode(Color.Red);
    public RangeNode<int> FrameThickness { get; set; } = new RangeNode<int>(1, 1, 20);    

    public List<HighLightRule> HighLightRules = new();

    [JsonIgnore]
    public RuleRenderer RuleConfig { get; set; }

    [Submenu(RenderMethod = nameof(Render))]
    public class RuleRenderer
    {
        private readonly HighlightNpcItemsByModSettings _parent;

        public RuleRenderer(HighlightNpcItemsByModSettings parent)
        {
            _parent = parent;
        }

        public void Render(HighlightNpcItemsByMod plugin)
        {

        }
    }

    public void AddRule()
    {
        HighLightRules.Add(new HighLightRule("Name", 0, new Vector4(.5f, .5f, .5f, 1), true));
    }
    public void RemoveRule(int index)
    {
        if (index >= 0 && index < HighLightRules.Count)
        {
            HighLightRules.RemoveAt(index);
        }
    }


}

public class HighLightRule
{        
    public string ModName = "";
    public bool Enabled= false;
    public int AtLeastTier = 0;    
    public Vector4 Color = new Vector4(.5f, .5f, .5f, 1);

    public HighLightRule(string modName,int atLeastTier,Vector4 color, bool enabled)
    {        
        ModName = modName;
        AtLeastTier= atLeastTier;
        Color = color;
        Enabled = enabled;
    }
}
