---
title: "Required Paths"
weight: 19
anchor: "pack-paths"
---

The advantage to using ACMI's detection logic is that you often won't need to recreate the whole `Nimbus/...` folder structure to pack your files: it will do that automatically. However, because PA can't name shit to save their lives, some file types still need at least some of the path to be recreated to pack properly.

|Type|Top Directory|Example Path|
|:--:|:------------|:-----------|
|Skins|N/A|`./fa44_04_D.uasset`|
|Canopies|N/A|`./f15c_Canopy1_Inst.uasset`|
|Weapons|N/A|`w_lacm_f0_D.uasset`|
|Cockpits|N/A|`asfx_CP_D.uasset`|
|Ships*|N/A*|`./aegs_D.uasset`|
|Radio Portraits|`SubtitleSpeakerPortrait`|`./SubtitleSpeakerPortrai/40_Scream.uasset`|
|Emblems|`Emblem`|`Emblem/png/emblem_022_m.uasset`
|Crosshairs|`HUD`|`./HUD/MultiLockon/hud_Multi_Lock-on_Air-to-Air_Missile_04.uasset`|
|VFX|`VFX`|`VFX/Common/Materials/AfterBurner/M_AfterBurner_Glow_01.uasset`|

> * Support for ship skins is **highly untested** and only supports textures right now

Anything that isn't in the list above will need the full `Nimbus/Content/...` structure in the folder you're building from to end up in the right place, but it will still be packed.