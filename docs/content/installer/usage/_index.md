---
title: "Installation and Usage"
weight: 10
anchor: "install-usage"
---

If your mods are in your current directory, just run `acmi.exe build` to build the installer files, or you can pass a specific directory like `acmi.exe build D:/Mods/MyAwesomeSkinPack` if it's not in the current directory. You can also just drag-and-drop your mod folder onto the `acmi.exe` file (see the Walkthrough below) to immediately build installer files for that directory.

If it completes successfully, you should see a new `fomod` directory in your mod files. Just include that folder when you ZIP up your archive (to upload to Nexus/ModDB/wherever) then anyone who installs your archive with a mod manager will get the nice guided installer. Users who want to install manually can just ignore the fomod folder and it will not affect your other files in any way.

{{< block note >}}
You can also just run <code>acmi.exe --help</code> to see some basic help and usage info
{{< /block >}}