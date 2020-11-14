---
title: "Images"
weight: 21
anchor: "usage-images"
---

We'll try our best to find a suitable image for the installer as a whole. This will be shown on the first page of the installer and for any steps without their own images. If there's only one `.png`/`.jpg` in the root folder, we'll use that. If there's more than one, we'll use the first one we find with "preview" in the name.

You can also have images for your individual skin files! Just include a `.png` or `.jpg` with the _same name_ as the `.pak` file, beside it. For example, for a file at `Skins\UntoldGalmPack_F15C_F15ACipher1994_MageSlot.pak`, we would check for a `Skins\UntoldGalmPack_F15C_F15ACipher1994_MageSlot.png` file and use that if it's found. 

Alternatively if your pak files are in separate folders (organised by slot or set, for example) the installer will also check if there's only one image in the same folder as your skin files and use that. If this causes problems for your structure, please [raise an issue](https://github.com/agc93/acmi/issues) and I'll try and come up with a better solution.