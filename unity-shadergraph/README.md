# unity-shadergraph

Custom shaders built with Unity's ShaderGraph on the Universal Render Pipeline (URP).

![Glow shader on three spheres](0-glow_up.png)

## Setup

- Unity 6000.4.7f1 (Unity 6, newer than the required 2022.3 LTS)
- Universal RP 17.4.0 and Shader Graph 17.4.0
- The URP asset is in `Assets/Settings/URP_Asset.asset` and is assigned under **Edit > Project Settings > Graphics** and every Quality level

## Tasks

### 0. Glow up

`Assets/Shaders/glow.shadergraph` is a URP Lit graph that makes an untextured object glow around its edges. The material is `Assets/Materials/glow.mat`.

How it works: a **Fresnel Effect** node gets brighter where the surface turns away from the camera, so it is strongest at the silhouette. Its output is multiplied by the glow color and the glow strength and sent to **Emission**, while the base color goes to **Base Color** as usual.

Properties you can change in the Inspector:

| Property | What it does |
| --- | --- |
| Base Color | Main surface color |
| Glow Color | HDR color of the edge glow |
| Glow Strength | How bright the glow is (0 turns it off) |
| Glow Power | Fresnel exponent: higher values give a thinner rim |
| Smoothness | Surface smoothness, for the specular highlight |

`glow_cyan.mat` and `glow_pink.mat` are extra presets that use the same shader. The `GlowShowcase` scene shows all three on spheres.

### 1. Pulse

![Pulsing glow on three spheres](1-pulse.gif)

`Assets/Shaders/glow_animated.shadergraph` takes the glow shader and makes the glow fade in and out. The material is `Assets/Materials/glow_animated.mat`.

How it works: the **Time** node is multiplied by the pulse frequency and by 2π, then goes through a **Sine** node, so it repeats once per second when the frequency is 1. A **Remap** node turns the sine's -1 to 1 range into 0 to 1, and a **Lerp** node uses that value to blend between the minimum and maximum glow strength. The result replaces the fixed strength from the previous task.

Properties you can change in the Inspector:

| Property | What it does |
| --- | --- |
| Base Color | Main surface color |
| Glow Color | HDR color of the edge glow |
| Min Glow Strength | Glow brightness at the dimmest point of the pulse |
| Max Glow Strength | Glow brightness at the brightest point of the pulse |
| Pulse Frequency | Pulses per second |
| Glow Power | Fresnel exponent: higher values give a thinner rim |
| Smoothness | Surface smoothness, for the specular highlight |

`glow_animated_cyan.mat` and `glow_animated_pink.mat` are extra presets with different speeds. The `GlowPulse` scene shows all three. The pulse plays in Play mode, or in the Scene view when **Always Refresh** is turned on in the effects menu.

### 2. Mr. Stark, I don't feel so good

![Disintegrating spheres](2-disintegrate.gif)

`Assets/Shaders/disintegrate.shadergraph` makes an object break apart into fragments and then put itself back together, on a loop. The material is `Assets/Materials/disintegrate.mat`.

How it works:

- A **Voronoi** node splits the surface into cells. Its **Cells** output gives every cell one random value between 0 and 1, so each cell acts like one fragment. Fragment Scale sets the cell density.
- **Time** times the speed and 2π goes through a **Sine** and a **Remap**, which gives a threshold that moves up and down between 0 and 1.
- The cell value goes into **Alpha** and the threshold into **Alpha Clip Threshold**, so every fragment whose value is below the threshold is cut away. As the threshold rises, more fragments disappear, one by one.
- A **Step** node finds the fragments that are about to disappear (their value is just above the threshold) and lights them with the edge color, so the pieces look like they burn away. Those fragments drop their base color so the glow stays bright.
- The graph renders both faces, so through the holes you see the inside of the shell instead of nothing.

Properties you can change in the Inspector:

| Property | What it does |
| --- | --- |
| Base Color | Main surface color |
| Disintegration Speed | How many times per second the object falls apart and comes back |
| Fragment Scale | Number of fragments: higher values give smaller pieces |
| Edge Color | HDR color of the fragments that are about to disappear |
| Edge Width | How many fragments glow before they disappear |
| Smoothness | Surface smoothness, for the specular highlight |

`disintegrate_fine.mat` (small fast fragments) and `disintegrate_chunky.mat` (big fragments) are extra presets. The `Disintegrate` scene shows all three. Like the pulse, the animation plays in Play mode, or in the Scene view with **Always Refresh** turned on.

### 3. Iceman

![Ice cubes](3-ice.png)

`Assets/Shaders/ice.shadergraph` makes an object look like a block of ice. The material is `Assets/Materials/ice.mat`.

How it works:

- The graph uses a **Transparent** surface with alpha blending and renders **both faces**, so you can see the back edges of a cube through its front.
- The frost is procedural, so no texture is needed. A **Simple Noise** node is multiplied by a **Voronoi** node, which gives a cracked, crystal-like pattern. That pattern blends the base color toward the frost color and also makes the frosty spots less transparent.
- The edges come from a fresnel term built by hand: **Normal Vector** dot **View Direction**, then **Absolute**, **One Minus** and **Power**. Faces seen at a steep angle get brighter and more solid, like real ice. The Absolute step keeps it working on back faces too, because URP does not flip their normals and the built-in Fresnel Effect node would turn them solid white.
- Alpha is the opacity plus the frost plus the edge term, clamped to 0–1 with a **Saturate** node.

Properties you can change in the Inspector:

| Property | What it does |
| --- | --- |
| Base Color | Main color of the ice |
| Opacity | How see-through the clear parts are |
| Frost Color | Color of the frosty pattern |
| Frost Scale | Size of the frost pattern: higher values give finer detail |
| Frost Amount | How strong the frost is |
| Edge Color | HDR color added on faces seen at a steep angle |
| Edge Power | How quickly the edge effect falls off |
| Edge Opacity | How much the edges add to the opacity |
| Smoothness | Surface smoothness, for the specular highlight |

`ice_clear.mat` (clearer, less frost) and `ice_frosted.mat` (heavier frost) are extra presets. The `Ice` scene shows all three on cubes.

### 4. Hey Cortana, what's UV positioning?

![Flickering hologram](4-hologram.gif)

`Assets/Shaders/hologram.shadergraph` gives an object a flickering hologram look built from a texture. The material is `Assets/Materials/hologram.mat` and it uses `Assets/Textures/hologram_scanlines.png`.

How it works:

- The graph is **Unlit** and **Transparent** and renders both faces, so the hologram is see-through and lights itself.
- The mesh **UV** goes through a **Tiling And Offset** node. Tiling (Line Density) repeats the texture more times across each face, and Offset is **Time** times the scroll speed, so the lines slowly crawl over the object. The result is the UV for a **Sample Texture 2D** node that reads the hologram texture.
- The flicker is a **Simple Noise** node fed with Time times the flicker speed. A **Lerp** between 1 and that noise, controlled by Flicker Strength, multiplies both the color and the alpha, so the whole hologram randomly dims and brightens.
- A fresnel term (Normal Vector dot View Direction, Absolute, One Minus, Power) makes the edges brighter and more solid, the same two-sided version as the ice shader.
- Color is the base color times the texture plus the edges. Alpha is the texture times the opacity plus the edges, then multiplied by the flicker.

Properties you can change in the Inspector:

| Property | What it does |
| --- | --- |
| Base Color | HDR color of the hologram |
| Hologram Texture | Texture that gives the hologram its pattern (white parts glow, black parts are see-through) |
| Line Density | How many times the texture repeats on each face |
| Scroll Speed | How fast the texture moves (negative values scroll the other way) |
| Opacity | How solid the bright parts of the texture are |
| Flicker Speed | How fast the flicker changes |
| Flicker Strength | How much the flicker dims the hologram (0 turns it off) |
| Rim Power | How quickly the edge glow falls off |

The two textures in `Assets/Textures` were made for this project: `hologram_scanlines.png` (horizontal scanlines) and `hologram_grid.png` (a grid with faint scanlines). `hologram_grid.mat` is an extra blue preset that uses the grid texture. The `Hologram` scene shows both. The animation plays in Play mode, or in the Scene view with **Always Refresh** turned on.

## Asset package

`unity-shadergraph.unitypackage` contains the shader graphs, the materials, the textures and the showcase scenes. To use it, import it into a URP project with **Assets > Import Package > Custom Package**, then drag `glow.mat`, `glow_animated.mat` or `disintegrate.mat` onto any 3D object, or `ice.mat` or `hologram.mat` onto a cube.
