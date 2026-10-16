# PowerFantasyMods

<h1>Requirements:</h1>
<br />
<h2>BepInEx 6.0.0.be.755:</h2>
Link to site:<br />
<b><i>https://builds.bepinex.dev/projects/bepinex_be/</i></b><br />
Direct Download Link:<br />
<b><i>https://builds.bepinex.dev/projects/bepinex_be/755/BepInEx-Unity.Mono-win-x86-6.0.0-be.755%2B3fab71a.zip</i></b><br />
<br />
Unzip:<br />
<b><i>BepInEx-Unity.Mono-win-x86-6.0.0-be.755%2B3fab71a.zip</i></b><br />
into:<br />
<b><i>C:\Program Files (x86)\Steam\steamapps\common\Power Fantasy\</i></b><br />
<br />
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
<br /><br />
<h3>Non-Windows platform only:</h3>
<i></i>You'll have to tell emulation layer to use the winhttp.dll by setting a command line parameter.<br />
In Steam, open the game's properties, and put the following text into the launch properties:<br />
WINEDLLOVERRIDES="winhttp.dll=n,b" %command%</i>
<h3>End of Non-Windows platform only.</h3>
<br /><br />
<h3>Initialize BepInEx</h3>
Start the game, wait for the main menu, quit the game.<br />
<br /><br />
find <b><i>BepInEx\config\BepInEx.cfg</i></b> file. Open it with a text editor such as Notepad.<br />
Find the line that says <b><i>HideManagerGameObject = false</i></b> and replace it with  <b><i>HideManagerGameObject = true</i></b><br />
<br /><br />
<h3>You are now ready to add mods:</h3>
Unrar the file downloaded:<br />
<b><i> Tjatja - Monster Spawn Rate.rar </i></b><br />
into:<br />
<b><i> C:\Program Files (x86)\Steam\steamapps\common\Power Fantasy\BepInEx\plugins\ </i></b><br />
Make sure to include the folder from the .rar!<br />
<br />
This mods adds 2 sliders to the general menu to control the values with<br />
The values only apply on stage start, not during stages.<br />
<br />
<h3>Something to take note off:</h3>
Achievements and highscores are turned off while running this mod.
<br /><br />
<h3>Launch the game. Enjoy the mod.</h3>
