# PowerFantasyMods

<h1>Requirements:</h1>

<h2>BepInEx 6.0.0.be.755:</h2><br />
<b><i>https://builds.bepinex.dev/projects/bepinex_be/</i></b><br />
<b><i>https://builds.bepinex.dev/projects/bepinex_be/755/BepInEx-Unity.Mono-win-x86-6.0.0-be.755%2B3fab71a.zip</i></b><br />

Unzip:
<b><i>BepInEx-Unity.Mono-win-x86-6.0.0-be.755%2B3fab71a.zip</i></b>
into:
<b><i>C:\Program Files (x86)\Steam\steamapps\common\Power Fantasy\</i></b>

this folder will now contain:
<i>BepInEx
D3D12
MonoBleedingEdge
Power Fantasy_Data
.doorstop_version
changelog.txt
doorstop_config.ini
Power Fantasy.exe
UnityCrashHandler64.exe
unityPlayer.dll
winhttp.dll</i>

<h3>Non-Windows platform only:</h3>
<i></i>You'll have to tell emulation layer to use the winhttp.dll by setting a command line parameter.
In Steam, open the game's properties, and put the following text into the launch properties:
WINEDLLOVERRIDES="winhttp.dll=n,b" %command%</i>
<h3>End of Non-Windows platform only.</h3>

Initialize BepInEx

Start the game, wait for the main menu, quit the game.

find <b><i>BepInEx\config\BepInEx.cfg</i></b> file. Open it with a text editor such as Notepad.
Find the line that says HideManagerGameObject = false and replace it with HideManagerGameObject = true

You are now ready to add mods:
Unrar the file downloaded:
<b><i>Tjatja - Monster Spawn Rate.rar</i></b>
into:
<b><i>C:\Program Files (x86)\Steam\steamapps\common\Power Fantasy\BepInEx\plugins\</i></b>
Make sure to include the folder from the .rar!

Done

<h3>Something to take note off:</h3>
Achievements and highscores are turned off while running this mod.

Launch the game. Enjoy the mod.
