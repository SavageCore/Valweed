Grow your own weed farm! Roll up joints for smoke-able buffs! 
Now with an immersive Bong!


0.2.5
-----
Fixed an issue that spammed the console when moving away from the smoke FX.
Standardized prefab names.
Tweaked smoke FX some more.


0.2.4
-----
Smoke now emits from the mouth after the Bong is smoked.
Smoke position has been fixed based on pose.



0.2.3
-----
Smoke now emits from mouth when joints or the bong are smoked.
Smoke now emits from the bong when used.
Weed nugs now show in the bowl when it is filled.
More sound effects added.



0.2.2
-----
Bong collider issue fixed. The Bong now sits flush with the surface it's built on.
Bong / Joints / Bud textures updated.



0.2.1
-----

Bong Effects Rework
I've redesigned the Bong's effect on the Rested buff.
	If you aren't Rested, hitting (figuratively) the Bong makes you Rested according to your current comfort level. It also has its own buff.
	If you are already Rested, each hit of the Bong will add 5 minutes to the effect, while refreshing its own buff.

Fixed an issue where Bongs were only placeable in the meadows. This seemed to be an internal bug, as I've used
	a hacky workaround. But it works.

Fixed an issue where Bongs wouldn't let you hit them if you didn't have bud, even if the bowl was full.


0.2.0
-----

Placeable Bong added! Fill the bowl with your weed buds and get ZOOTED.

	Ingredients:
	Bronze Nails 5x
	Resin 10x
	Skeleton Trophy 1x
	Greydwarf Eye 5x

	The bong effect is a combination of all 3 joint effects.
	It also lasts 3x the joint effect time. You can refresh it by smoking more, but the rested time wont be added to
	until the original time that the bong added has elapsed.

Thank you Gravebear for the 3D model! Check out his mods at https://valheim.thunderstore.io/package/OdinPlus/ (He has a BongLantern :)
Thank you Zarboz for helping a lot with various issues throughout the life of this mod. Check out his mods at https://valheim.thunderstore.io/package/sbtoonz/

Bong sounds from Zapsplat.com



0.1.1
-----
Reorganized folder structure to meet Thunderstore standards... ffs



0.1.0
-----
Fixed the lack of sounds when planting / harvesting.
Customizable localizations added. This makes it easier, and quicker, for you to add your own translations.
	To add a custom translation, modify the Translations/{Your language}/{language}.json or create your own.



0.0.6
-------
Fixed an issue where fully grown plants would despawn when either logging out, or traveling far away.
More translations added.


0.0.5.1
----------
Fixed an issue where plant placement only worked in the Meadows.
Fixed a typo in the Indica Joint description.

More translations coming soon!



0.0.5
-------
ModConfig support added. Health / stamina regeneration rates are now adjustable, along with buff effect length.
﻿You can access this from the settings menu. Restart the game for changes to take effect.
Indica Joints reworked to be more useful.
Gives the Rested buff again. This time, if you are already Rested, adds 10m to the buff's timer.
Joints are now placeable using horizontal item stands.
Fixed an issue where ItemDrops weren't persistent on logout.


Requires:

    BepInEx 5.4.10+
    Jotunn 2.0.9+
    HookGenPatcher




Known Incompatibilities / Issues:

    There is currently no planting/growing/harvesting sound
    Loot sparkles have an orange tinge



0.0.4
-------
GROWABLE PLANTS UPDATE

Now you can start your own weed farm!
This update also overhauls the materials again, in addition to adding some new ones.

New Materials:

    Weed Seeds
    Weed Buds
    Joint Paper


New Crafting Requirements:
Grind down 3 Ancient Seeds to create Weed Seeds, then plant them with the Cultivator.
Growing time is similar to Carrots.

Joint Paper
2x Dandelion

Hybrid Joint
1x Joint Paper
1x Weed Buds
1x Raspberries

Indica Joint
1x Joint Paper
1x Weed Buds
1x Blueberries

Sativa Joint
1x Joint Paper
1x Weed Buds
1x Honey

Requires:

    BepInEx 5.4.10+
    Jotunn 2.0.7+
    HookGenPatcher



Known Incompatibilities / Issues:

    There is currently no planting/growing/harvesting sound
    Loot sparkles have an orange tinge


Thank you to RocketKitten5 & GraveBear for helping me fix the Joint Paper mesh's back-face culling issue!



0.0.3
-------
First translation update. Fixes localization issues in languages other than English.

Translated Languages

    German
    Italian
    Spanish
    French


Languages Still in Need of Translation

    Polish
    Russian
    Turkish
    Dutch
    Simplified Chinese
    Japanese
    Brazilian Portugese


If there are any native language speakers who could translate these terms for me in the needed languages, I'd be super appreciative:

Joint (Hybrid)
Joint (Indica)
Joint (Sativa)
Smoke a fat doobie.
You feel high.
You're coming down.

I'm not sure as to what the regional slang tends to be for each term, but I know it can vary a lot. I'd like to avoid translating "Joint" to "Hinge" or something lol



0.0.2
--------
New strains added! Joints can now be hybrid, indica, or sativa.
Effects are now all unique, so they can be stacked with each other and the Rested bonus.
Recipes have been modified to require berries or honey in addition to their originals.

I'll be getting around to modding in pieces with this next update, so hopefully growable plants will come alongside the bong!

Hybrid
You feel balanced.
﻿Health regen +50%
﻿Stamina regen +100%

Indica
You feel relaxed.
﻿Health regen +50%
﻿Stamina Regen +100%
﻿Hunger rate +50%

Sativa
You feel motivated.
﻿Health regen +50%
﻿Stamina Regen +100%
﻿Hunger rate -50%

Requires:

    BepInEx 5.4+
    Jotunn 2.0.7+
    HookGenPatcher



Known Incompatibilities / Issues: None that I know of.

TODO:

    Smoking animation?
    Placeable bong for a longer duration buff on use
    Config file for adjustable rates




0.0.1
-------
Ingredients:
1x Dandelion
1x Thistle

Requires:

    BepInEx
    JotunnLib


Known Incompatibilities: None that I know of.

TODO:

    Multiple growable strains which produce varying effects
    Smoking animation?
    Placeable bong for a longer duration buff on use