---
title: "Usage"
weight: 12
anchor: "pack-usage"
---

In short, the easiest way to build your PAK files is to drag-and-drop the folder containing all your cooked files onto `acmi-pack.exe`.

## Preparing your cooked files

Once you have your cooked files (should be a set of files like `fa27_05_D.uasset` and so forth), drop them into an empty folder.

- Aircraft skin files (including diffuses, instance files and MREC files) can simply be left as-is, they don't need to be put into their own folder structure.
- Same for weapon skins, cockpits and canopies: just dump them wherever
- Other supported file types (i.e. portraits, emblems, and crosshairs) should be put in a folder but *don't need the whole folder structure*, just the actual folder they end up in.
  - For example, radio portraits just need to be in a folder called `SubtitleSpeakerPortrait` and emblems just need to go in a folder called `Emblem` etc etc
- Any files that ACMI doesn't support, you'll need to recreate the full "Nimbus/..." structure for, but they'll still be packed.

{{< block tip >}}
You <em>can</em> put your skin files in a folder if you want, ACMI will just "ignore" that and use the correct game folder path.
{{< /block >}}

## Running the build

Now drag the folder you set up onto `acmi-pack.exe` and you should get some questions to walk you through the process.

You'll have the option to group certain files together into a single PAK file. If you don't want to do this, just hit `ENTER` when prompted to skip it. Grouping can be useful for doing things like packing all your HUD elements into a single PAK file, or rolling all your NPC skins into one file.

Once you've completed the prompts (and chosen the output format) you will see a short summary of the files that were created and you can check your assets folder to find your new `pak` files.

{{< block note >}}
You can also just run <code>acmi-pack.exe --help</code> to see some basic help and usage info
{{< /block >}}