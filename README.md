![mod1](https://github.com/user-attachments/assets/e6beacbd-4a57-41c6-86bd-76883c775249)
![mod3](https://github.com/user-attachments/assets/0bff9d5a-851e-4bef-95f0-1cfc9e53c2ef)
![mod2](https://github.com/user-attachments/assets/92caf70f-694e-4578-9537-6fd6e6ab4d1f)


Plugin For ExileCore2, Similar to NpcInvWithLinq, copied most of code from https://github.com/diesal/AuraTracker <p/>

To find item mod names, tick the "Write Hovered Item Mod Names On Debug Window" box and hover the mouse over the item. <p/>

Tick the "Tab labels" box, if you want to highlight tab when there are matching items in that tab. You can change color or thickess for tab highlighting. thickness also affect item frames<p/>

Click the bar/title to open or close that section<p/>

<h2>First section: Highlight by item properties</h2><p/>
Highlight items with socket, quality or item level. set different frame color for every option and change socket count, quality percent or item level. it works like "and above". for example if you set quality percent "10", it means quality percent 10 and above will highlight.<p/>

<h2>Second section: Highlight by item rarity</h2><p/>
Highlight items with selected rarity. you can set different frame color for every rarity level.<p/>

<h2>Third section: Highlight by item mods</h2><p/>
Added rule texts are searched through item mod raw names and item mod tiers of npc item's. matched items are highlighted. you can set different frame color for every rule.<p/>

Click the "Add ItemMod" button to add a new rule.<p/>
![mod4](https://github.com/user-attachments/assets/1e8d7d5d-51cf-4132-a527-d255fba134bf)
<p/>
<b>Add new rule section</b>
<ul>
  <li>up/down arrows: move rule up/down</li>
  <li>checkbox: enable/disable rule</li>
  <li>colorbox: set color for matched item highlight</li>
  <li>textbox: set rule text (item mod raw name)</li>
  <li>textbox: set tier</li>
  <li>minus/plus: increase/decrease tier</li>
  <li>remove button: removes rule</li>
</ul>
<p/>
<b>Rule definition examples</b><p/>
<table>
  <th>Rule</th>
  <th>Tier</th>
  <th>Result</th>
  <tr>
    <td>CastSpeed</td>
    <td></td>
    <td>highlight all items that contain Cast Speed</td>
  </tr>
  <tr>
    <td>CastSpeed5</td>
    <td></td>
    <td>highlight items that only contain Tier5 Cast Speed</td>
  </tr>
  <tr>
    <td>CastSpeed</td>
    <td>5</td>
    <td>highlight items with Cast Speed ​​Tier 5 and above</td>
  </tr>
  <tr>
    <td>Speed</td>
    <td></td>
    <td>highlight items all items that contains "Speed" (attack speed, cast speed, movement speed, minion speeds etc.)</td>
  </tr>
  <tr>
    <td></td>
    <td>8</td>
    <td>highlight items all items that have at least one tier8+ (8 and above) mod</td>
  </tr>
</table>
<p/>
The first defined rule takes priority, define mod you want the most at the top. you can move mods up and down or enable/disable rules<p/>
<p/>
Highlight algorithm looks for a matched rule first.<p/>
if there are not any matched rule then look for matched properties settings. <p/>
if there are not any matched item by property then look for rarity settings. <p/>
basically <b>Rules>Quality>Socked>ItemLevel>Rarity</b><p/>
<p/>
if a rule is matched, it does not check the other rules for more match. <p/>
