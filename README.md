# PowerFantasyMods

<h1>Requirements:</h1>

<h2>BepInEx 6.0.0.be.755:</h2>
<b><i>https://builds.bepinex.dev/projects/bepinex_be/</i></b><br />
<b><i>https://builds.bepinex.dev/projects/bepinex_be/755/BepInEx-Unity.Mono-win-x86-6.0.0-be.755%2B3fab71a.zip</i></b><br />

Unzip:<br />
<b><i>BepInEx-Unity.Mono-win-x86-6.0.0-be.755%2B3fab71a.zip</i></b><br />
into:<br />
<b><i>C:\Program Files (x86)\Steam\steamapps\common\Power Fantasy\</i></b>

this folder will now contain:<br />
<i>BepInEx<br />
D3D12<br />
MonoBleedingEdge<br />
Power Fantasy_Data<br />
.doorstop_version<br />
changelog.txt<br />
doorstop_config.ini<br />
Power Fantasy.exe<br />
UnityCrashHandler64.exe<br />
unityPlayer.dll<br />
winhttp.dll</i><br />

<h3>Non-Windows platform only:</h3>
<i></i>You'll have to tell emulation layer to use the winhttp.dll by setting a command line parameter.
In Steam, open the game's properties, and put the following text into the launch properties:
WINEDLLOVERRIDES="winhttp.dll=n,b" %command%</i>
<h3>End of Non-Windows platform only.</h3>

Initialize BepInEx

Start the game, wait for the main menu, quit the game.

find <b><i>BepInEx\config\BepInEx.cfg</i></b> file. Open it with a text editor such as Notepad.
Find the line that says <b><i>HideManagerGameObject = false</i></b> and replace it with  <b><i>HideManagerGameObject = true</i></b>

You are now ready to add mods:
Unrar the file downloaded:
<b><i> Tjatja - Monster Spawn Rate.rar </i></b>
into:
<b><i> C:\Program Files (x86)\Steam\steamapps\common\Power Fantasy\BepInEx\plugins\ </i></b>
Make sure to include the folder from the .rar!

Done

<h3>Something to take note off:</h3>
Achievements and highscores are turned off while running this mod.
<br /><br />
<h3>Launch the game. Enjoy the mod.</h3>
