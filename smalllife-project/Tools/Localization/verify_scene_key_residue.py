import csv
import re
from pathlib import Path

ROOT = Path('.')
MAP_PATH = ROOT / 'Assets/LocalizationSource/split/key_map_final.csv'


def load_old_keys() -> set[str]:
    keys: set[str] = set()
    with MAP_PATH.open('r', encoding='utf-8-sig', newline='') as f:
        for row in csv.DictReader(f):
            old_key = (row.get('old_key') or '').strip()
            new_key = (row.get('new_key') or '').strip()
            if old_key and new_key and old_key != new_key:
                keys.add(old_key)
    return keys


def collect_files() -> list[Path]:
    files: list[Path] = []
    for base, pattern in ((ROOT / 'Assets/Scenes', '*.unity'), (ROOT / 'Assets/Prefabs', '*.prefab')):
        if base.exists():
            files.extend(base.rglob(pattern))
    return files


def main() -> None:
    old_keys = load_old_keys()
    files = collect_files()

    direct_total = 0
    override_total = 0

    direct_fields = ('phraseName', 'textKey', 'translationName')
    override_fields = ('phraseName', 'textKey', 'translationName')

    for fp in files:
        # Per user request: ignore Level4 for now.
        if fp.name == 'Level4.unity':
            continue

        lines = fp.read_text(encoding='utf-8', errors='ignore').splitlines()

        local_direct = []
        local_override = []

        pending_prop: str | None = None

        for i, line in enumerate(lines, start=1):
            for field in direct_fields:
                m_direct = re.match(rf'^\s*(?:-\s*)?{field}:\s*(\S+)\s*$', line)
                if m_direct:
                    value = m_direct.group(1)
                    if value in old_keys:
                        local_direct.append((i, field, value))

            m_prop = re.match(r'^\s*propertyPath:\s*(phraseName|textKey|translationName)\s*$', line)
            if m_prop:
                pending_prop = m_prop.group(1)
                continue

            if pending_prop in override_fields:
                m_value = re.match(r'^\s*value:\s*(\S*)\s*$', line)
                if m_value:
                    value = m_value.group(1)
                    if value in old_keys:
                        local_override.append((i, pending_prop, value))
                pending_prop = None

        if local_direct:
            direct_total += len(local_direct)
            print(f'DIRECT {fp.as_posix()} count={len(local_direct)} sample={local_direct[:3]}')

        if local_override:
            override_total += len(local_override)
            print(f'OVERRIDE {fp.as_posix()} count={len(local_override)} sample={local_override[:3]}')

    print(f'SUMMARY direct_total={direct_total} override_total={override_total}')


if __name__ == '__main__':
    main()
