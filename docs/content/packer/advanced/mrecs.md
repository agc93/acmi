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