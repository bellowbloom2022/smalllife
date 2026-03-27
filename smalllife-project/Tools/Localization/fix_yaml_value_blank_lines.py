from pathlib import Path
import re

ROOT = Path('.')
TARGET_DIRS = [ROOT / 'Assets/Scenes', ROOT / 'Assets/Prefabs']
SUFFIXES = {'.unity', '.prefab'}


def process_file(path: Path) -> int:
    text = path.read_text(encoding='utf-8', errors='ignore')
    original = text

    # Remove blank lines inserted between "value:" and "objectReference:" entries.
    text = re.sub(r'(\n[ \t]*value:[ \t]*\n)(?:[ \t]*\n)+([ \t]*objectReference:)', r'\1\2', text)

    if text != original:
        path.write_text(text, encoding='utf-8')
        return 1
    return 0


def main() -> None:
    changed = 0
    scanned = 0
    for base in TARGET_DIRS:
        if not base.exists():
            continue
        for p in base.rglob('*'):
            if p.is_file() and p.suffix.lower() in SUFFIXES:
                scanned += 1
                changed += process_file(p)

    print(f'scanned_files: {scanned}')
    print(f'changed_files: {changed}')


if __name__ == '__main__':
    main()
