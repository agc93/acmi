---
title: "Usage"
weight: 12
anchor: "usage"
---

You'll need a command prompt open (Windows PowerShell, Terminal, whatever) then just run `acmi.exe` (if it's in the current directory) to see the basic help. If your mods are in your current directory, just run `acmi.exe build` to build the installer files, or you can pass a specific directory like `acmi.exe build D:/Mods/MyAwesomeSkinPack` if it's not in the current directory.

If it completes successfully, you should see a new `fomod` directory in your mod files. Just include that folder when you ZIP up your archive (to upload to Nexus/ModDB/wherever) then anyone who installs your archive with a mod manager will get the nice guided installer. Users who want to install manually can just ignore the fomod folder and it will not affect your other files in any way.

{{< block tip >}}
You can also just run `acmi.exe` (or `acmi.exe --help`) to see some basic help and usage info
{{< /block >}}