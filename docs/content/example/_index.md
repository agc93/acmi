---
title: "Walkthrough"
anchor: "example"
weight: 60
---

For an example of how to use this, we're going to build installer files for a chonker of a skin pack: the excellent Skies Untold Historic Warlock Pack by njmksr.

### Getting Started

First, we're going to assume you've unpacked the `acmi.exe` to somewhere convenient. In our example, that's going to be `D:/Mods`, the same folder where our mod packs are installed:

```text
D:\Mods
├── Skies Untold Warlock Pack
│   ├── readme.txt
│   └── Skins
│       ├── Warlock 1959
│       │   ├── UntoldWarlockPack_F104C_Warlock616_SpareSlot_P.pak
│       │   └── UntoldWarlockPack_F104C_Warlock742_MageSlot_P.pak
│       ...trimmed...
│       └── Warlock Heritage
│           ├── UntoldWarlockPack_F15E_HeritagePaint_StriderSlot_P.pak
│           ├── UntoldWarlockPack_YF23_HuxianHeritageF4E_StriderSlot_P.pak
│           └── UntoldWarlockPack_YF23_WisemanHeritageP51_SpareSlot_P.pak
├── Mobius Skin with Gold Nozzles or whatever
│   └── ... your other files...
└── acmi.exe
```

### Running the tool

Now it's just a case of running ACMI and telling it where your mod files are:

```text
D:\Mods> .\acmi.exe build '.\Skies Untold Warlock Pack\'
```
Or in live form:

![walkthrough](/acmi.gif)

Now, you can just package up your mod files like you usually would (or use `./acmi.exe zip` to quickly have it done for you)

![acmi zip](/acmi-zip.gif)

When a user installs that ZIP file with a supported mod installer (Vortex in this example), they'll see a proper wizard stepping them through aircraft and slot choices:

![fomod installer](/ac-fomod-min.gif)