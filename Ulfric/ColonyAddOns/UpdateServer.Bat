rmdir "C:\Program Files (x86)\Steam\SteamApps\common\Colony Survival\gamedata\mods\Ulfric\ColonyAddOns" /s /Q
xcopy "C:\Users\Ulfric\Source\Repos\Ulfric.ColonyAddOns\Ulfric\ColonyAddOns\bin\Debug\Ulfric\ColonyAddOns\*.*" "C:\Program Files (x86)\Steam\SteamApps\common\Colony Survival\gamedata\mods\Ulfric\ColonyAddOns\" /e
copy "C:\Users\Ulfric\Source\Repos\Ulfric.ColonyAddOns\Ulfric\ColonyAddOns\Debug.txt" "C:\Program Files (x86)\Steam\SteamApps\common\Colony Survival\gamedata\mods\Ulfric\ColonyAddOns\" 
copy "C:\Users\Ulfric\Source\Repos\Ulfric.ColonyAddOns\Ulfric\modinfo.json" "C:\Program Files (x86)\Steam\SteamApps\common\Colony Survival\gamedata\mods\Ulfric\"
pause