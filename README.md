# ACMI: AC7 Mod Installers

> wow that's a horrible name. my bad.

### What is it?

This is a simple command-line app that will automatically generate the XML files required for mod installers (like Vortex) to show a nice guided installer rather than users having to pick the right mod files on their own.

> These installers are usually known as FOMODs, but aren't actually game-specific.

This way, with essentially no extra effort, you can include the files in your mod uploads (on Nexus, ModDB, wherever) and if someone installs it with Vortex they'll get a nice wizard walking them through which files to choose.

### Who is it for?

**Modders**. More specifically, it's for whoever is _packing_ mod archives. Before you actually ZIP up the mod files, just run `acmi build`, answer the prompts, and the files will be generated for you (in their own `fomod` folder).

At present, **do not** use this with model mods! It will detect model swaps as a skin file and group it with everything else. This will hopefully be fixable, but it will be fine for skins for custom models.

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

## Usage

#### Download

Download the [latest release](https://github.com/agc93/acmi/releases) to somewhere convenient on your computer (put it with your packing scripts if you're using them).

#### Run

> There's much more detailed docs at [acmi.modding.app](https://acmi.modding.app/)

If your mods are in your current directory, just run `acmi.exe build` to build the installer files, or you can pass a specific directory like `acmi.exe build D:/Mods/MyAwesomeSkinPack` if it's not in the current directory. You can also just drag-and-drop your mod folder onto the `acmi.exe` file to immediately build installer files for that directory.

If it completes successfully, you should see a new `fomod` directory in your mod files. Just include that folder when you ZIP up your archive (to upload to Nexus/ModDB/wherever) then anyone who installs your archive with a mod manager will get the nice guided installer. Users who want to install manually can just ignore the fomod folder and it will not affect your other files in any way.