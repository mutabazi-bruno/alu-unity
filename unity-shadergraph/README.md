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

## Asset package

`unity-shadergraph.unitypackage` contains the shader graphs, the materials and the showcase scenes. To use it, import it into a URP project with **Assets > Import Package > Custom Package**, then drag `glow.mat` or `glow_animated.mat` onto any 3D object.
