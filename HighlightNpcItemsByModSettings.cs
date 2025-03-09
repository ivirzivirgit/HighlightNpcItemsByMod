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
    
    public bool DrawOnTabLabels = true;

    public bool HLGeneralHeaderOpen = true;
    public bool HLPropertiesHeaderOpen = true;
    public bool HLRarityHeaderOpen = true;
    public bool HLRulesHeaderOpen = true;


    public bool HLbyQuality = false;
    public int HLQualityPercent = 10;
    public Vector4 HLQualityColor = new Vector4(.7f, .8f, .8f, 1);

    public bool HLbySocket = false;
    public int HLSocketCount = 2;
    public Vector4 HLSocketedColor = new Vector4(.6f, .1f, .8f, 1);

    public bool HLbyItemLevel = false;
    public int HLItemLevel = 80;
    public Vector4 HLItemLevelColor = new Vector4(.5f, .6f, .6f, 1);

    public bool HLNormalItems = false;
    public bool HLMagicItems = false;
    public bool HLRareItems = false;
    public bool HLUniqueItems = false;

    public Vector4 HLRarityNormalColor = new Vector4(.9f, .9f, .9f, 1);
    public Vector4 HLRarityMagicColor = new Vector4(.3f, .5f, .8f, 1);
    public Vector4 HLRarityRareColor = new Vector4(.8f, .8f, .3f, 1);
    public Vector4 HLRarityUniqueColor = new Vector4(.8f, .4f, .1f, 1);



    public bool InspectHoverItemMods = false;

    public Vector4 TabFrameColor = new Vector4(.1f, .8f, .1f, 1);

    public int FrameThickness = 1;

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
        HighLightRules.Add(new HighLightRule("", 0, new Vector4(.7f, .7f, .7f, 1), true));
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
    public int AtLeastTier = 1;
    public Vector4 Color = new Vector4(.7f, .7f, .7f, 1);

    public HighLightRule(string modName,int atLeastTier,Vector4 color, bool enabled)
    {        
        ModName = modName;
        AtLeastTier= atLeastTier;
        Color = color;
        Enabled = enabled;
    }
}
