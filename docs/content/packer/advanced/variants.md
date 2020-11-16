---
title: "Alternative Files"
weight: 21
anchor: "pack-alt-variants"
---

Since ACMI Packer doesn't need to set up the full folder structure, it's a bit easier to do variations of the same files.

#### Walkthrough: Skins

Let's take the example of two variants of a skin for the same slot: an XFA-27 skin in Slot 7. When you cooked your files, you will have produced some files that you can copy into a folder like this:

```text
C:/Mods/XFASkin
├── Standard Version
│   ├── fa27_06_D.uasset
│   ├── fa27_06_D.uexp
│   ├── fa27_06_MREC.uasset
│   └── fa27_06_MREC.uexp
```

Easy enough, right? Now cook your alternate version and put them in their own folder:

```text
C:/Mods/XFASkin
├── Standard Version
│   ├── fa27_06_D.uasset
│   ├── fa27_06_D.uexp
│   ├── fa27_06_MREC.uasset
│   └── fa27_06_MREC.uexp
├── Alternate Skin
│   ├── fa27_06_D.uasset
│   ├── fa27_06_D.uexp
│   ├── fa27_06_MREC.uasset
│   └── fa27_06_MREC.uexp
```

Now to build the `pak`s for your standard version, just drag the `Standard Version` directory to `acmi-pack.exe`. Then, to pack your alternate skins, drag the `Alternate Skin` directory onto `acmi-pack.exe`. You'll get the relevant `pak` files put into each folder and if you give the alternate version a different name when prompted, that will appear in the `pak` file names too!
