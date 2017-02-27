------ BASIC INFO -----------

This tool automates the replacement of mod files. This includes .dat files in your Wargame install dir and your WRD player profile in STEAM_INSTALL_DIR\userdata\...\251060\remote. 

The program has not been extensively tested in multi-user settings. If multiple people play Wargame on your computer, you should either:
-have everyone use the same ModManager
-OR launch the game using the vanilla configuration after every play session, so that the game is restored to unmodded state.

------ DETAILS --------------

The program does two things when you select a configuration and launch:
1. Mod installation. The contents of the mods/modname/bin folder will be copied to the corresponding wargame subdir. Any files that are being replaced will be backed up to the mods/vanilla folder if they don't exist there already.
2. Profile management. Wargame profiles are located in [STEAM_INSTALL_DIR\userdata\...\251060\remote]. There, modmanager will make a text file recording which configuration was played last time, and a profile for each launch configuration.
2.a. When you launch the game, modmanager will save any profile changes from your last session, and load the appropriate mod-specific profile.
2.b. Note that the program has no way of knowing which of your Steam accounts you're logged into, so all of them will be changed in each launch. This is unlikely to cause issues but is important if you deleted the program and are undoing its changes manually.


------ TROUBLESHOOTING --------------

If selecting "vanilla" launches a modded version of the game, delete the contents of the modmanager/mods/vanilla folder and verify the integrity of WRD through Steam.

It is not recommended to use multiple copies of modmanager. If you want to be able to launch it from multiple places, use shortcuts, don't copy the entire folder.

During updates, if you get an error about a file already existing in ../TEMP, go to the mentioned TEMP directory and delete the mentioned file. Also, it would be nice if you report the specific circumstances that caused the error to me.

The mods/ folder contains all launch configurations. Renaming the folders will change the display name for the mods. It's also likely to make modmanager create a new player profile for the renamed mod, so adjust the names in STEAM_INSTALL_DIR\userdata\...\251060\remote accordingly if you want to avoid this.

mods/vanilla is a special folder and should not be renamed. This is your backup folder. When modmanager replaces a wargame file for the first time, that's where the "original" goes.


------ UNINSTALL --------------

Before deleting this tool, launch the game with the "vanilla" option so that any mods are removed. After that you can just delete this folder.
Alternatively, verify integrity of game files and copy the vanilla player profile to STEAM_INSTALL_DIR\userdata\...\251060\remote.



------ FOR MOD MAKERS --------------

Mods go in mods/modname/bin/. Modname can be anything.

You can either put the file in its version folder [like mods/modname/bin/354645637/436543636/ZZ_Win.dat];
Or you can put it directly in bin/, in which case the program will try to find and replace the newest file of the same name.

The manager will only let you launch one mod, but if you have two that are compatible with each other you can put them in the same modname/bin folder to get around that limitation.

If you want to set up auto update for your mod, you'll need a (free to make) github repository. For details, look at a working example or read https://github.com/pvutov/WargameModManager/blob/master/WargameModManager/UPDATE_INSTRUCTIONS .

------ CONTACT ----------------------

For bugs : https://github.com/pvutov/WargameModManager - register and open an issue. 
Alternatively, PM me on the eugen forum: http://forums.eugensystems.com/memberlist.php?mode=viewprofile&u=18222
Feature requests and general comments are welcome too.