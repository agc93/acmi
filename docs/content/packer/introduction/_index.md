---
title: "Introduction"
weight: 1
anchor: "pack-introduction"
---

ACMI Packer (aka `acmi-pack`) is a simple command-line app that will semi-automatically pack your cooked asset files into `pak` files ready for distribution (remember to run ACMI before you zip up your final package). Unlike existing solutions, ACMI Packer attempts to cut down on the manual work in packing and particularly doesn't usually need folders to be set up correctly.

{{< block warn >}}
ACMI Packer is <strong>extremely</strong> new and <strong>extremely</strong> alpha-quality software. If you try it out, please be aware you might find bugs. Please report them to me.
{{< /block >}}

ACMI Packer is a **highly-opinionated** tool. To boil that down, it means this will **not** be the tool for *every* possible packing scenario and has no intentions to be. ACMI Packer is primarily intended for skin mods and other UI mods and might struggle with more complex stuff.

{{< block note >}}
It's worth knowing that ACMI Packer doesn't generate the pak files itself: it relies on <code>u4pak.py</code> for that.
{{< /block >}}

#### What's the ACMI connection?

ACMI Packer is pretty much impossible to fully separate from ACMI itself as it uses the same underlying logic to read and identify AC7 mod files. Whereas ACMI reads the files from inside a `pak` file, ACMI Packer can just read it straight from the cooked files on disk.

This also means that ACMI Packer works best with any of the file types that ACMI supports. As of 0.2.1, that's aircraft skins, radio portraits, weapon skins, crosshairs, and canopies.
