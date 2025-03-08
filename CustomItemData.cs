using ExileCore2;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared;
using ItemFilterLibrary;
using System.Collections.Generic;
using System.Linq;

public class CustomItemData : ItemData
{
    public CustomItemData(Entity queriedItem, GameController gc, EKind kind, RectangleF clientRect = default) : base(queriedItem, gc)
    {
        Kind = kind;
        ClientRectangle = clientRect;
    }

    public RectangleF ClientRectangle { get; set; }
    public EKind Kind { get; }
}

public enum EKind
{
    QuestReward,
    Shop,
    RitualReward
}

