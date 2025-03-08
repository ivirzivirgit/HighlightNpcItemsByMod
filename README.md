Plugin For ExileCore2, Similar to NpcInvWithLinq, copied most of code from https://github.com/diesal/AuraTracker <p/>

To find item mod names, tick the "Write Hovered Item Mod Names On Debug Window" box and hover the mouse over the item. <p/>

Tick the "Draw on tab labels" box to light, if you want to highlight tab when there are matching items in that tab.<p/>

Click the bar to open or close the rules tab.<p/>

Click the "Add ItemMod" button to add a new rule.<p/>

*Use the first box to enable/disable the rule.<p/>
*Use the second box to change the frame color of the matching items.<p/>
*Use the third box to enter the mod raw name.<p/>
*Use the fourth box to enter the mod tier.<p/>
*Use the "Remove Mod" button to delete the rule.<p/>

Examples;<p/>
By defining a rule like "CastSpeed", you can highlight all items that contain Cast Speed.<p/>

By defining a rule like "CastSpeed5", you can highlight only items that contain Tier5 Cast Speed.<p/>

By defining a rule like "CastSpeed" and increasing Tier to 5, you can highlight items with Cast Speed ​​Tier 5 and above.<p/>

By defining a rule like "Speed", you can highlight items with at least one of all speed modes (attack speed, cast speed, movement speed, minion speeds etc.).<p/>

If you define an empty rule and increase Tier to 8, all items with Tier 8 mod and above will be highlighted.<p/>

The first defined rule takes priority, define mod you want the most at the top. <p/>

if a rule is matched, it does not check the other rules for more match. <p/>
