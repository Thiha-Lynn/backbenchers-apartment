"""Check actual browser captures for the missing/mismatched baked-lighting regression."""
import json
from pathlib import Path
from PIL import Image, ImageStat
root = Path(__file__).resolve().parents[1]
for mood in ('room', 'night'):
    for index in range(7):
        with Image.open(root / 'assets' / f'{mood}-{index}.jpg') as image:
            assert image.size == (1600, 900), f'Incomplete preview: {mood}-{index}'
with Image.open(root / 'assets/room-0.jpg') as image:
    # Flat concrete pillar: atlas corruption creates high contrast; missing GI makes it dark.
    stats = ImageStat.Stat(image.crop((315, 90, 395, 550)).convert('L'))
    mean, deviation = stats.mean[0], stats.stddev[0]
    assert 90 < mean < 220, f'Daylight missing or clipped: {mean:.1f}'
    assert deviation < 35, f'Lightmap atlas/direction mismatch: {deviation:.1f}'
report = {'passed': True, 'previews': 14, 'daylightPillarMean': mean, 'daylightPillarDeviation': deviation}
(root / 'validation/preview-lighting.json').write_text(json.dumps(report, indent=2) + '\n')
print(json.dumps(report, indent=2))
