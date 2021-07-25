---
title: "Unsupported Files"
weight: 24
anchor: "pack-unsupported"
---

While ACMI Packer works best with skins and other ACMI-supported files (like radio portraits or canopies), it *should* still work just fine for other mods as well. Just create the full `Nimbus/Content/.../...` folder structure for your cooked file like you usually would and run the packer. It *should* identify the files in their folder and recreate the folder structure from the build folder in the resulting PAK file.

{{< block note >}}
This is much less tested than the supported types, so please let me know if you run into problems using this method.
{{< /block >}}
