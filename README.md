# ForeverLib

* A modding library for Unity Game Decompilations.

* MAKE SURE TO USE THE SAME DLL VERSION AS THE GAME YOU'RE MODDING, SWAP OUT THE REFERENCES IN THE VS IF NEEDED!

* THE GAME CLOSES AFTER A MOD IS LOADED, YOU WILL HAVE TO REOPEN THE GAME MANUALLY, SORRY LOL!

# NOTES:

* IF YOU PLAN ON USING THIS FOR YOUR GAME DECOMPILATION, YOU **HAVE** TO CREDIT maybekoi (me) FOR THE LIBRARY.

If you're using an old Unity version like Unity 5.x, you can go ahead and delete/comment out:

```c#
using UnityEngine.AssetBundleModule;
```
and
```c#
using UnityEngine.CoreModule;
```

Since they were added in newer versions of Unity.

## PLATFORM SPECIFIC NOTES:

- Android (NOT DONE YET, TRYING TO FIND A WAY TO REPLACE ASSETS):

* If you're using Android, you'll need to add UnityEngine.AndroidJNIModule as a reference in the vs project.

* Mods will be stored in:

```
/storage/emulated/0/Games/GAMENAME/Mods
```

# How to add to your decomp/project:

* Download or Build ForeverLib.dll

* Place ForeverLib.dll in your Unity Project's Assets/Plugins folder.

* Add the ForeverLib prefab to the first scene of your decomp/project.

* Profit

# CREDITS

- [MaybeKoi(me!)](https://github.com/MaybeKoi) - Library


# Mod Structure (when loading/setting up a mods folder)

```
MyMod/
  ├── MyMod.dll
  └── assets/
      └── sharedassets1.assets
```