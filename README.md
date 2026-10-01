# The Apartment

[Live apartment](https://thiha-lynn.github.io/backbenchers-apartment/) · [Repository](https://github.com/Thiha-Lynn/backbenchers-apartment)

An interactive apartment visit based on the locally supplied HDRP Archviz Apartment package. The commercial source package stays local; this repository contains the compiled experience, captured previews, and authored integration scripts.

## Experience

Seven viewpoints cover the lounge, kitchen, bedroom, games area, artist’s corner, bathroom, and entrance. Drag to look; WASD/arrows to walk. Touch visitors have a left joystick and independent look gesture. Tap a visible lamp to open its controls, or use **Room menu → Lighting studio**.

Daylight uses warm-white natural light. Midnight uses a dark exterior and warm indoor fixtures. Ten fixtures have independent power, brightness, and color controls. Master power, brightness, and warmth apply across the apartment. Lighting settings persist separately for each mood. Brightness fades between settings; reduced motion removes camera easing and room-transition fades.

The scene uses baked ambient lighting and reflections plus shadowless real-time fixtures. Emitters are positioned at the visible bulbs; shaded pendants use broad downward cones and floor lamps follow their shade direction, avoiding artificial ceiling hotspots. Switching a fixture off removes its direct lighting and bulb emission. Real-time shadows and per-fixture reflection rebakes are intentionally excluded to control browser rendering cost.

Procedural footsteps follow actual distance traveled. Sound begins after visitor interaction and can be disabled. Optional photo previews cover all rooms in both lighting modes without loading Unity.

## Optimization

The original package contains 1,009 asset/folder entries: one scene, 101 FBX models, 176 prefabs, 148 materials, and 411 PNG textures. The source scene has 1,211 mesh renderers and roughly 993,095 instanced vertices.

The upgrade removes 90 exterior renderers outside the explorable apartment, their colliders, and exterior lighting. Materials use Built-in Standard shaders compatible with WebGPU and WebGL 2. Texture imports use Crunch DXT5: 256 px for small props, 512 px for major normal/mask maps, and 1024 px for major surfaces and artwork. Models use medium mesh compression. Shared geometry is transmitted once and batched at startup; source mesh CPU copies are released afterward. Day/night atlas layouts must match for batched geometry. The runtime explicitly selects non-directional lightmaps, retains the matching shader variants, and preserves UV channels. The build clears prefab batching overrides before saving; runtime batching happens after lighting coordinates are restored.

The build uses Brotli with JavaScript decompression fallback for GitHub Pages, high managed stripping, IL2CPP size optimization, a 128 MB initial heap with geometric growth, data caching, and browser-driven frame pacing. Auto detail limits rendering resolution; manual Light and Detailed modes are available.

## Sharing

Static HTML contains absolute Open Graph and Twitter image metadata. The 1200 × 630 card is captured from the apartment. Messaging services decide whether to display or cache previews.

## Reproduction

Use Unity 6000.7.0b2 with Web build support. Import the original package with `Source/Editor/ApartmentImporter.cs` installed under `Assets/Apartment/Editor`. Copy the remaining authored scripts into their corresponding `Assets/Apartment` folders, and `Source/Inspection/materials.json` into the project’s `Inspection` directory.

For a fresh scene, run `ApartmentBuilder.Convert`, `ApartmentBuilder.Prepare`, `ApartmentMaterialCleanup.Apply`, and `ApartmentPolish.Apply`. Then run `ApartmentUpgrade.Prepare` once and `ApartmentFixtureSetup.Prepare` to trim exterior geometry and create fixture controls. The unmodified local scene is retained as `BeforeUpgrade.unity`.

Run `ApartmentUpgrade.SetBake("day")`, bake to completion, and `ApartmentUpgrade.CaptureBake("day")`. Repeat for `"night"`. Call `ApartmentUpgrade.Finish` and capture the browser-rendered previews after building. Confirm matching day/night lightmap indices and offsets, zero missing/error materials, and readable source meshes for runtime batching. Build with `ApartmentBuilder.Build`, then run `python3 scripts/build-manifest.py` to fingerprint output files. Capture all room previews with `node scripts/render-previews.cjs`, then run `python3 scripts/verify-preview-lighting.py` (Pillow) and render the social card with `node scripts/render-social.cjs` against the local server. The image check detects missing daylight and corrupted lightmap coordinates on a stable concrete surface.

Run `node scripts/verify.cjs` and `node scripts/verify-lights.cjs` using the bundled Playwright module, or configure `PLAYWRIGHT_MODULE`. Set `HEADLESS=1` for regression checks, `WEBGL=1` to exercise fallback in the main suite, and `APARTMENT_URL` to test a deployment. Hardware frame-rate measurements need an otherwise idle browser/device. Mobile emulation checks layout and input; it does not establish physical-phone performance.

Deployment uses GitHub Pages Actions on pushes to `main`. The workflow publishes only HTML, CSS, JavaScript, preview images, and the Unity runtime.

## Release validation — 1 October 2026

The runtime payload is 24,945,668 bytes (23.8 MiB), down from 80,883,214 bytes (77.1 MiB): a 69.2% reduction. Both lighting moods are included in that download. The site does not download Unity until a visitor enters 3D.

The Unity build completed with zero errors. The lighting regression verified all ten lights actually switch off, master intensity and individual color reach Unity, and clicking a visible fixture opens its matching controls. Runtime batching combined 1,058 active renderers in the test scene. Steady Chrome WebGPU samples with midnight fixtures enabled were 60.0 fps. This is a measurement on the test Mac, not a guarantee for every device.

Safari 26.6.2 opened the final WebGL 2 tour and displayed both lighting modes correctly. Safari uses a compatible renderer selection; its frame rate was not measured in an isolated benchmark. Reload recovery handles incomplete startup.
