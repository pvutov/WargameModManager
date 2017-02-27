For users:

This tool automates the replacement of mod files. This includes .dat files in you Wargame install dir and your wargame player profile in STEAM_INSTALL_DIR\userdata\...\251060\remote. 

If you share the Steam or Wargame installation with other users on the same PC, the mod manager will affect all users. You can clear the effects of modmanager by launching with the "vanilla" option selected.

The mods/vanilla folder is automatically managed by the program. It will only be correct if your wargame wasn't modded when you started using this program [delete the vanilla folder and verify game files if selecting "vanilla" starts a modded game].

Troubleshooting: IF vanilla doesn't launch an umodded game, delete the contents of the mods/vanilla folder and verify game cache. The next time you use a mod, your now verified files will be backed up in the now emptied mods/vanilla folder.

UNINSTALL: 
Before deleting this tool, launch the game with the "vanilla" option so that any mods are removed.
Alternatively, verify integrity of game files and set the vanilla player profile in STEAM_INSTALL_DIR\userdata\...\251060\remote.



For mod makers:

mods go in mods/modname/bin/

you can either put the file in its version folder [like mods/modname/bin/354645637/436543636/ZZ_Win.dat]
or you can put it directly in bin/, in which case the program will try to find it

the manager will only let you launch one mod, but if you have two that are compatible with each other you can put them in the same modname/bin folder to get around that limitation

If you want to set up auto update for your mod, you'll need a (free to make) github repository. For details, look at a working example or read https://github.com/pvutov/WargameModManager/blob/master/WargameModManager/UPDATE_INSTRUCTIONS