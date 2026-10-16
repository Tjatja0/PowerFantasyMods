# PowerFantasyMods
<h1>Requirements:</h1>
<h2>BepInEx 6.0.0.be.755:</h2>
Link to site:<br />
<b><i>https://builds.bepinex.dev/projects/bepinex_be/</i></b><br />
Direct Download Link:x86 systems<br />
<b><i>https://builds.bepinex.dev/projects/bepinex_be/755/BepInEx-Unity.Mono-win-x86-6.0.0-be.755%2B3fab71a.zip</i></b><br />
Direct Download Link:x64 systems<br />
<b><i>https://builds.bepinex.dev/projects/bepinex_be/755/BepInEx-Unity.Mono-win-x64-6.0.0-be.755%2B3fab71a.zip</i></b><br />
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
<h3>Non-Windows platform only:</h3>
<i></i>You'll have to tell emulation layer to use the winhttp.dll by setting a command line parameter.<br />
In Steam, open the game's properties, and put the following text into the launch properties:<br />
WINEDLLOVERRIDES="winhttp.dll=n,b" %command%</i>
<h3>End of Non-Windows platform only.</h3>
<h3>Initialize BepInEx</h3>
Start the game, wait for the main menu, quit the game.<br />
<br />
find the <b><i>BepInEx\config\BepInEx.cfg</i></b> file.<br />
BepInEx.cfg not found?<br />
Delete contents of:<br />
<b><i> C:\Program Files (x86)\Steam\steamapps\common\Power Fantasy\ </i></b><br />
Go to: Steam>Power Fantasy>Manage>Properties>Installed files<br />
<b><i>Verify integrity of game files</i></b><br />
Highly likely you downloaded the file for the wrong system:<br />
Restart the entire guide, this time use the other download link.<br />
<br />
<h3>You are now ready to add mods:</h3>
Unrar the file downloaded:<br />
<b><i> Tjatja - Monster Spawn Rate.rar </i></b><br />
into:<br />
<b><i> C:\Program Files (x86)\Steam\steamapps\common\Power Fantasy\BepInEx\plugins\ </i></b><br />
Make sure to include the folder from the .rar!<br />
<br />
<h3>Tjatja - Monster Spawn Rate.rar </h3>
This mods adds 2 sliders to the general menu to control its settings with<br />
The settings only apply on stage start, not during stages.<br />
<h4>Something to take note off:</h4>
Achievements and highscores are turned off while running this mod.
<br /><br />
<h3>Launch the game. Enjoy the mod.</h3>
<br />
<h3>To turn off the mod:</h3>
Go to: <br />
C:\Program Files (x86)\Steam\steamapps\common\Power Fantasy\BepInEx\config\<br />
open:<br />
Tjatja.powerfantasymods.MonsterSpawnRate.cfg<br />
Set Enabled to false. 
