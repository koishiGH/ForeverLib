# ForeverLib

* A modding library for Unity Game Decompilations.

* MAKE SURE TO USE THE SAME DLL VERSION AS THE GAME YOU'RE MODDING, SWAP OUT THE REFERENCES IN THE VS IF NEEDED!

# NOTE:

If you're using an old Unity version like Unity 5.x, you can go ahead and delete/comment out:

```c#
using UnityEngine.AssetBundleModule;
```
and
```c#
using UnityEngine.CoreModule;
```

Since they were added in newer versions of Unity.


# CREDITS

- [MaybeKoi(me!)](https://github.com/MaybeKoi) - Library


# Mod Structure (when loading/setting up a mods folder)

MyMod/
  ├── MyMod.dll
  └── assets/
      └── sharedassets1.assets
