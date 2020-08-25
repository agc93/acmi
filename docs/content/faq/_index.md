---
title: "FAQ"
anchor: "faq"
weight: 50
---

Here are some of the Frequently Asked Questions about ACMI.

### Who is ACMI for?

**Modders**. More specifically, it's for whoever is _packing_ mod archives. Before you actually ZIP up the mod files, just run `acmi build`, answer the prompts, and the files will be generated for you (in their own `fomod` folder).

{{< block warn >}}
At present, <strong>do not</strong> use this with model mods! It will detect model swaps as a skin file and group it with everything else. This will hopefully be fixable, but it will be fine for skins for custom models.
{{< /block >}}

> You can also run `acmi zip` to immediately pack all the files in the current directory into a ZIP.

### How does it work?

The short version is that ACMI will scan through the mod root (defaults to the current directory) and find _any_ `.pak` files then check for the actual object path in the file headers (something like `/Game/f15c_06_D`) and use that to guess what aircraft and slot it's for. Next, we build a big list of all the skins in all the files, group them by aircraft and skin slot and then build the XML that mod installers will use to provide choices to the user.

### What file types does it support?

At current, ACMI will fully detect and specifically handle skins, radio portraits, weapons, crosshairs and visual effects. Any PAK files that can't be detected properly will still be included in the installer files in a separate "Extra Files" step.

### What else can I do?

- If you have any README files that you want to be installed, make sure they're in the **root** of the mod files, not in a folder of their own.
- We'll try our best to find a suitable image for the installer
  - If there's only one `.png`/`.jpg` in the root folder, we'll use that. If there's more than one, we'll use the first one we find with "preview" in the name.
- If you want to also have pictures for your skin files, just include a `.png` or `.jpg` with the _same name_ as the `.pak` file, beside it.
  - For example, for a file at `Skins\UntoldGalmPack_F15C_F15ACipher1994_MageSlot.pak`, we would check for a `Skins\UntoldGalmPack_F15C_F15ACipher1994_MageSlot.png` file and use that if it's found.
- If your `.pak` file has more than one skin in it, you can include the exact string `MULTI` anywhere in the file name.
  - This dramatically slows down the build process but should correctly pull *all* included skins out, not just the first.
  - For example, `F15SMTD_Monarch_Skin1_P.pak` will only get scanned for one, but `Flashy_NPC_Aircraft_MULTI_P.pak` would get scanned for _any_ skins

### Will it affect other users?

In short, no. When a user goes to install your mod manually, they'll unzip the archive and the only impact is that there will be an extra `fomod` folder in the unpacked files. There's only two XML files in there, so it won't even break things if users accidentally "install" that folder, so it shouldn't cause any impact for users.

### Does it affect my mod files?

**No.** We don't make any changes to the `.pak` files the ACMI finds (I wouldn't even know how), and instead we just read some strings out of the file headers to determine aircraft, skin, etc