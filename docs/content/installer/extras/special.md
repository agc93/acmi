---
title: "Special Files"
weight: 23
anchor: "usage-special"
---

There's a couple of things you can also do to tweak how ACMI picks up your files and includes them in an installer.

### Lightweight Packing

If the mod has been packed using Cherry's "lightweight" packing method, the individual slot files will have nothing but an instance file in them so ACMI won't detect them as a "real" skin file. If you include `(LIGHT)` or `(LW)` somewhere in the file name, ACMI will instead detect those instance files and sort them into planes and slots correctly.

Likewise, if you add `(REQ)` or `(REQD)` in your "base package" file, it should be identified as a required file and users will not be able to toggle the file off during installation.

{{< block tip >}}
You can use any of `[`,`(` or `_` to surround the special keywords. So all of `(LIGHT)`, `[LW]` and `_REQD_` would all be correctly identified.
{{< /block >}}
