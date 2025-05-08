# Treasure Map Overhaul
Overhaul to the Repairing Treasure Maps quest. Replaces the 4 map corners with a new item "A Torn Treasure Map".

## Installation
- Install [BepInEx Mod Pack](https://thunderstore.io/package/bbepis/BepInExPack/)
- Download the latest [release](https://github.com/et508/Erenshor.TreasureMapOverhaul/releases)
- Extract files from Erenshor."ModNameHere".zip into Erenshor\BepInEx\plugins\ folder.

## Changing Drop Rate
- Run Erenshor so the config file will be automatically created
- Open *et508.erenshor."modnamehere"* in your Erenshor\BepInEx\config
- Change values to your liking
- I recommend using a config manager like [BepInExConfigManager](https://github.com/sinai-dev/BepInExConfigManager) for easier config changes from ingame

## The Treasure Maps
- This mod will replace any treasure map corners you currently have with the new "A Torn Treasure Map". In your bank and your inventory.
- Removes the old corner maps from the loot tables and replaces with the new map.
- Changes the quest turn in for a Treasure Map to 4 of the new "A Torn Treasure Map". No more hunting down corners or stock piling 1 type and never seeing another.
- This does effectivly remove the old map corners from the game. So if you use this mod and then remove it you will lose what maps you had. 

## Erenshor LootTables
- Erenshor pre loads the loot table for every NPC when the scene loads.
- If changing drop chance using a ingame config manager, changes to drop rates will only apply to newly spawned enemies. Reload the scene or wait for new respawns for changes to apply.
