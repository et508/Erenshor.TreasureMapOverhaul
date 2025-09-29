# Treasure Map Overhaul
Overhaul to the Repairing Treasure Maps quest. Replaces the 4 map corners with a new item "A Torn Treasure Map".

## Installation
- Install [BepInEx Mod Pack](https://thunderstore.io/c/erenshor/p/BepInEx/BepInExPack/)
- Download the latest [release](https://github.com/et508/Erenshor.TreasureMapOverhaul/releases/1.0.1)
- Extract files from Erenshor.TreasureMapOverhaul.zip into Erenshor\BepInEx\plugins\ folder.

## Changing Drop Rate
- Run Erenshor so the config file will be automatically created
- Open *et508.erenshor.treasuremapoverhaul* in your Erenshor\BepInEx\config
- Change values to your liking
- I recommend using a config manager like [BepInExConfigManager](https://github.com/sinai-dev/BepInExConfigManager) for easier config changes from ingame

## The Treasure Maps
- This mod will replace any treasure map corners you currently have with the new "A Torn Treasure Map". In your bank and your inventory.
- Removes the old corner maps from the loot tables and replaces with the new map.
- Changes the quest turn in for a Treasure Map to 4 of the new "A Torn Treasure Map". No more hunting down corners or stock piling 1 type and never seeing another.
- This does effectively remove the old map corners from the game. So if you use this mod and then remove it you will lose what maps you had. 

## Erenshor LootTables
- If changing drop chance using a ingame config manager, changes to drop rates will only apply to newly spawned enemies. Reload the scene or wait for new respawns for changes to apply.

## Elusive Bug
I have had a few reports now of the quest turn in not working, as well as the converting of the old maps to the new map not working. I have been unable to reproduce these bugs on my end. I put some extra error logging in place as of version 1.0.2 to try and catch these bugs. If you are experiencing any of these problems, please let me know in the [discord post.](https://discord.com/channels/1099145747364057118/1369926805120745612/1369926805120745612)
