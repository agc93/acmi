---
title: "Multi-Skin Files"
weight: 22
anchor: "usage-merged"
---

If your mod directory includes `.pak` files with more than one skin in them (such as merged NPC files), include the exact string `MULTI` anywhere in the file name. This tells ACMI to scan a much larger chunk of the file to find skins and include _any_ it finds, not just the first one.

{{< block warn >}}
This can slow down the build process quite a bit if you have lots of large merged files, but your installer will be misleading if you don't.
{{< /block >}}

For example, `F15SMTD_Monarch_Skin1_P.pak` will only get scanned for one, but `Flashy_NPC_Aircraft_MULTI_P.pak` would get scanned for _any_ skins and included in a separate step of the final installer. This *should* work with any `pak` file up to about 2.5GB, but [let me know](https://github.com/agc93/acmi/issues) if it doesn't work.