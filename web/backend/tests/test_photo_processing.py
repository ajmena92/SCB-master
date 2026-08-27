from io import BytesIO
from pathlib import Path

from PIL import Image

from scripts.import_student_photos import normalize_photo


def test_normalize_photo_limits_dimensions_and_returns_jpeg(tmp_path: Path):
    source = tmp_path / "student.png"
    Image.new("RGBA", (1600, 1200), (40, 120, 80, 255)).save(source)

    content, width, height = normalize_photo(source)

    assert (width, height) == (800, 600)
    assert content[:2] == b"\xff\xd8"
    with Image.open(BytesIO(content)) as result:
        assert result.format == "JPEG"
        assert result.size == (800, 600)
