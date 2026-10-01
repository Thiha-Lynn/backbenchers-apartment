# The Apartment

A separate apartment tour for Backbenchers Studio, based on the locally supplied HDRP Archviz Apartment package. Website route: `/backbenchers-apartment/`.

## Source and conversion

The 1.29 GB package contains 1,009 asset/folder entries: one scene, 101 FBX models, 176 prefabs, 148 materials, 411 PNG textures, animations and ancillary data. The inspected scene has 1,211 mesh renderers and approximately 993,095 instanced vertices before static batching. The original package is retained in Downloads; imported commercial source stays local in `ApartmentStudio`. Only the compiled experience, captured previews and authored integration code are published.

HDRP shaders are converted to the Built-in Standard pipeline for WebGPU / WebGL 2 compatibility. Base colour, normal, metallic/smoothness and occlusion maps are mapped to the corresponding Standard properties. Missing source material slots are repaired with ceramic, wood or graphite materials. The adapted scene uses baked daylight and reflections, static batching, mipmapped 512 px texture caps with 1024 px colour maps for major surfaces and artwork, a 128 MB initial heap with geometric growth, Brotli with decompression fallback for GitHub Pages, high code stripping, and size-optimized IL2CPP. HDRP ray tracing and screen-space effects are not reproduced.

## Experience

An optional lightweight photo preview lets visitors browse the seven rooms before loading 3D. Seven authored viewpoints cover the lounge, kitchen, bedroom, games area, artist’s corner, bathroom and entrance. Drag to look; WASD/arrows to walk. Touch visitors have a left joystick and independent look gesture. Room changes fade briefly; reduced motion removes easing. Detail, movement speed, sound and atmosphere preferences persist locally. Procedural footsteps are triggered by distance actually traveled; no audio download is required. Sound starts only after visitor interaction.

The initial HTML contains absolute Open Graph and Twitter image metadata. The 1200 × 630 social image is captured from the actual apartment. Messaging services control whether they display or cache previews.

## Reproduction

Import the original package into the local Unity project with `ApartmentImporter` in place. Run `ApartmentBuilder.Convert`, `ApartmentBuilder.Prepare`, then `ApartmentMaterialCleanup.Apply` and `ApartmentPolish.Apply`; bake and save lighting. Run `ApartmentBuilder.Build`, then `python3 scripts/build-manifest.py`. Capture the apartment preview after lighting is saved.

Run `node scripts/verify.cjs` with the bundled Playwright module. Optional `APARTMENT_URL` tests a deployment. Physical phones and high-refresh monitors require hardware validation; browser-emulated layouts do not establish real phone performance.

## Validation (1 October 2026)

Unity 6000.7.0b2 build completed with zero errors. Compressed runtime download is approximately 77 MB, requested only after entering 3D. The opening photo and seven-room preview work without that download. First-time 3D loading depends on connection speed; Unity data caching supports repeat visits.

The adapted scene audit found zero missing or error-shader material slots, including inactive objects. Live Chrome WebGPU testing on this Mac recorded approximately 59.6–60.3 fps. All seven viewpoints, keyboard walking, movement-triggered footsteps, quality settings, reduced motion, leave/re-enter, and 320/390/844/1440 px layouts passed without console errors. A separate headless Chrome run with WebGPU disabled confirmed the WebGL 2 / OpenGLES3 fallback and the same functional checks. Mobile emulation passed simultaneous joystick and look gestures; this does not establish physical phone frame rates. 60+ fps on every device is not guaranteed.

Authored Unity scripts are included in `Source`; copy them into `Assets/Apartment` in a project containing the original package. Copy `Source/Inspection/materials.json` into the project’s `Inspection` folder before running conversion. Generated source assets, Unity Library caches, and the purchased package are not in this web repository.
