---
title: "Normals and MRECs"
weight: 22
anchor: "pack-normals-mrecs"
---

When ACMI Packer starts packing your files, it will actually read any Instance files (aka `*_Inst.uasset`) it finds and automatically determine the paths for MRECs and Normal files. ACMI Packer will set up any extra paths (like the `ex/` folder) for you depending on what the Instance file uses: just make sure the cooked asset is *named* right.

That means that *in general*, you can just dump all your cooked files and any relevant instance files in the same folder, like so:

```text
C:/Mods/A10Skin
├── a10a_00_D.uasset
├── a10a_00_D.uexp
├── a10a_00_Inst.uasset
├── a10a_00_Inst.uexp
├── a10a_00_MREC.uasset
├── a10a_00_MREC.uexp
├── a10a_00_N.uasset
└── a10a_00_N.uexp

```

...and when ACMI runs, it will open and read `a10a_00_Inst.uasset` and determine the correct place for the `MREC` and `N` files in the final PAK file.

If you have the Instance file, you can also open it with the ACMI Packer to quickly check what paths it uses. Note that the instance reader will show you the paths that the instance file uses **even if it's a vanilla game file**. For example, instances without separated Normals will still show a path/filename for Normals, it will just be a vanilla game file.