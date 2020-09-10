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

At current, ACMI will fully detect and specifically handle skins, radio portraits, weapons, crosshairs, canopies and visual effects. Any PAK files that can't be detected properly will still be included in the installer files in a separate "Extra Files" step.

### What happens to my lovingly crafted README files?

If you have any README files that you want to be installed, make sure they're in the **root** of the mod files, not in a folder of their own. If you do that, no matter what choices someone makes in the installer, your README file will still be installed with all its veiled threats and shitpost-y lore details

### Will it affect other users?

In short, no. When a user goes to install your mod manually, they'll unzip the archive and the only impact is that there will be an extra `fomod` folder in the unpacked files. There's only two XML files in there, so it won't even break things if users accidentally "install" that folder, so it shouldn't cause any impact for users.

### Does it affect my mod files?

**No.** We don't make any changes to the `.pak` files the ACMI finds (I wouldn't even know how), and instead we just read some strings out of the file headers to determine aircraft, skin, etc