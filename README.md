# PowerFantasyMods
Power Fantasy Mods

Requirements:

BepInEx 6.0.0.be.755:
https://builds.bepinex.dev/projects/bepinex_be/
https://builds.bepinex.dev/projects/bepinex_be/755/BepInEx-Unity.Mono-win-x86-6.0.0-be.755%2B3fab71a.zip

Unzip:
BepInEx-Unity.Mono-win-x86-6.0.0-be.755%2B3fab71a.zip
into:
C:\Program Files (x86)\Steam\steamapps\common\Power Fantasy\

this folder will now contain:
BepInEx
D3D12
MonoBleedingEdge
Power Fantasy_Data
.doorstop_version
changelog.txt
doorstop_config.ini
Power Fantasy.exe
UnityCrashHandler64.exe
unityPlayer.dll
winhttp.dll

Non-Windows platform only:
You'll have to tell emulation layer to use the winhttp.dll by setting a command line parameter.
In Steam, open the game's properties, and put the following text into the launch properties:
WINEDLLOVERRIDES="winhttp.dll=n,b" %command%
End of Non-Windows platform only.

Initialize BepInEx

Start the game, wait for the main menu, quit the game.

find BepInEx\config\BepInEx.cfg file. Open it with a text editor such as Notepad.
Find the line that says HideManagerGameObject = false and replace it with HideManagerGameObject = true

You are now ready to add mods:
Unrar the file downloaded:
Tjatja - Monster Spawn Rate.rar
into:
C:\Program Files (x86)\Steam\steamapps\common\Power Fantasy\BepInEx\plugins\
Make sure to include the folder from the .rar!

Done

Something to take note off:
Achievements and highscores are turned off while running this mod.

Launch the game. Enjoy the mod.
