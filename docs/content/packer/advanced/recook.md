---
title: "Recooking Assets"
weight: 23
anchor: "pack-recooking"
---

{{< block note >}}
This is highly experimental functionality! It also only supports aircraft skins.
{{< /block >}}

If you've already cooked your skin's assets for one slot, ACMI can "recook" your files for different slots without having to duplicate and cook them all in UE4.

Once you have all your cooked assets for one slot, you should have files like the below:

```text
C:/Mods/XFASkin
├── fa27_00_D.uasset
├── fa27_00_D.uexp
├── fa27_00_Inst.uasset
├── fa27_00_Inst.uexp
├── fa27_00_MREC.uasset
└── fa27_00_MREC.uexp
```

Next, you **must** put any instance files for slots you want to use in the folder as well. Otherwise ACMI might recook MRECs or Normals with the wrong file paths.

Next, grab the _source_ assets (i.e the cooked assets you want to copy to other slots) and drop them all onto `acmi-pack.exe`. If ACMI parses your cooked assets correctly, it will prompt you to select what slots you want to "recook" for, so you might enter `03,04,05` to repack for the Mage/Spare/Strider slots.

ACMI will then create copies of the cooked assets but modified to use whichever slot(s) you entered. As long as you put your instance files in the folder as well, it will even read the instance files and use the correct names for any MRECs or Normals it recooks.

You can then grab the folder and drop it onto `acmi-pack.exe` to immediately pack your newly "recooked" assets.