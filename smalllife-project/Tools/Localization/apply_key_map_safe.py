import csv
import re
from pathlib import Path

ROOT = Path('.')
MAP_PATH = Path('Assets/LocalizationSource/split/key_map_final.csv')

SPLIT_DIR = Path('Assets/LocalizationSource/split')
SCRIPT_DIR = Path('Assets/Script')
RESOURCES_DIR = Path('Assets/Resources')
LEVEL_DATA_DIR = RESOURCES_DIR / 'LevelDataAssets'
SCENES_DIR = Path('Assets/Scenes')
PREFABS_DIR = Path('Assets/Prefabs')

CSV_TARGETS = [
    SPLIT_DIR / 'Common_Global.csv',
    SPLIT_DIR / 'Menu.csv',
    SPLIT_DIR / 'Intro.csv',
    SPLIT_DIR / 'Gameplay_UI.csv',
    SPLIT_DIR / 'Level_Spot0.csv',
    SPLIT_DIR / 'Level_Spot1.csv',
    SPLIT_DIR / 'Level_Spot2.csv',
    SPLIT_DIR / 'Level_Spot3.csv',
    SPLIT_DIR / 'Level_Spot4.csv',
]


def load_mapping() -> dict[str, str]:
    mapping: dict[str, str] = {}
    with MAP_PATH.open('r', encoding='utf-8-sig', newline='') as f:
        reader = csv.DictReader(f)
        for row in reader:
            old_key = (row.get('old_key') or '').strip()
            new_key = (row.get('new_key') or '').strip()
            if not old_key or not new_key:
                continue
            if old_key == new_key:
                continue
            mapping.setdefault(old_key, new_key)
    return mapping


def replace_csv_key_column(path: Path, mapping: dict[str, str]) -> tuple[int, int]:
    if not path.exists():
        return 0, 0

    with path.open('r', encoding='utf-8-sig', newline='') as f:
        reader = csv.DictReader(f)
        rows = list(reader)
        fieldnames = reader.fieldnames or []

    if 'key' not in fieldnames:
        return 0, 0

    changed_rows = 0
    for row in rows:
        key = (row.get('key') or '').strip()
        if key in mapping:
            row['key'] = mapping[key]
            changed_rows += 1

    if changed_rows > 0:
        with path.open('w', encoding='utf-8', newline='') as f:
            writer = csv.DictWriter(f, fieldnames=fieldnames)
            writer.writeheader()
            writer.writerows(rows)

    return (1 if changed_rows > 0 else 0), changed_rows


def list_text_files(base: Path, suffixes: set[str]) -> list[Path]:
    if not base.exists():
        return []
    return [p for p in base.rglob('*') if p.is_file() and p.suffix.lower() in suffixes]


def replace_quoted_keys(path: Path, mapping: dict[str, str]) -> int:
    try:
        text = path.read_text(encoding='utf-8')
    except UnicodeDecodeError:
        text = path.read_text(encoding='utf-8', errors='ignore')

    original = text
    replacements = 0

    # Replace only exact quoted keys to avoid changing natural language text.
    for old_key, new_key in mapping.items():
        pattern = re.compile(r'"' + re.escape(old_key) + r'"')
        text, n = pattern.subn('"' + new_key + '"', text)
        replacements += n

    if text != original:
        path.write_text(text, encoding='utf-8')

    return replacements


def replace_level_data_asset_keys(path: Path, mapping: dict[str, str]) -> int:
    try:
        lines = path.read_text(encoding='utf-8').splitlines()
    except UnicodeDecodeError:
        lines = path.read_text(encoding='utf-8', errors='ignore').splitlines()

    out: list[str] = []
    replacements = 0
    list_section: str | None = None

    for line in lines:
        stripped = line.strip()

        if stripped == 'goalDescriptionKeys:':
            list_section = 'goalDescriptionKeys'
            out.append(line)
            continue
        if stripped == 'goalSummaryKeys:':
            list_section = 'goalSummaryKeys'
            out.append(line)
            continue

        if list_section is not None:
            m_item = re.match(r'^([ \t]*)-\s*(\S+)\s*$', line)
            if m_item:
                indent, value = m_item.groups()
                new_value = mapping.get(value, value)
                if new_value != value:
                    replacements += 1
                out.append(f'{indent}- {new_value}\n')
                continue
            list_section = None

        m_single = re.match(r'^([ \t]*)(titleKey|descriptionKey):\s*(\S+)\s*$', line)
        if m_single:
            indent, field_name, value = m_single.groups()
            new_value = mapping.get(value, value)
            if new_value != value:
                replacements += 1
            out.append(f'{indent}{field_name}: {new_value}\n')
            continue

        out.append(line + '\n')

    updated = ''.join(out)
    original = '\n'.join(lines) + ('\n' if lines else '')
    if updated != original:
        path.write_text(updated, encoding='utf-8')

    return replacements


def replace_scene_prefab_localization_fields(path: Path, mapping: dict[str, str]) -> tuple[int, int]:
    try:
        lines = path.read_text(encoding='utf-8').splitlines()
    except UnicodeDecodeError:
        lines = path.read_text(encoding='utf-8', errors='ignore').splitlines()

    out: list[str] = []
    phrase_replacements = 0
    name_replacements = 0
    key_field_replacements = 0
    pending_value_for_property: str | None = None

    for line in lines:
        m_property = re.match(r'^([ \t]*)propertyPath:\s*(phraseName|textKey|translationName)\s*$', line)
        if m_property:
            pending_value_for_property = m_property.group(2)
            out.append(line + '\n')
            continue

        if pending_value_for_property is not None:
            m_value = re.match(r'^([ \t]*value:\s*)(\S*)\s*$', line)
            if m_value:
                prefix, value = m_value.groups()
                new_value = mapping.get(value, value)
                if new_value != value:
                    if pending_value_for_property == 'phraseName':
                        phrase_replacements += 1
                    else:
                        key_field_replacements += 1
                out.append(f'{prefix}{new_value}\n')
                pending_value_for_property = None
                continue
            pending_value_for_property = None

        m_phrase = re.match(r'^([ \t]*phraseName:\s*)(\S+)\s*$', line)
        if m_phrase:
            prefix, value = m_phrase.groups()
            new_value = mapping.get(value, value)
            if new_value != value:
                phrase_replacements += 1
            out.append(f'{prefix}{new_value}\n')
            continue

        m_text_key = re.match(r'^([ \t]*(?:-\s*)?textKey:\s*)(\S+)\s*$', line)
        if m_text_key:
            prefix, value = m_text_key.groups()
            new_value = mapping.get(value, value)
            if new_value != value:
                key_field_replacements += 1
            out.append(f'{prefix}{new_value}\n')
            continue

        m_translation_name = re.match(r'^([ \t]*(?:-\s*)?translationName:\s*)(\S+)\s*$', line)
        if m_translation_name:
            prefix, value = m_translation_name.groups()
            new_value = mapping.get(value, value)
            if new_value != value:
                key_field_replacements += 1
            out.append(f'{prefix}{new_value}\n')
            continue

        m_name = re.match(r'^([ \t]*m_Name:\s*)(\S+)\s*$', line)
        if m_name:
            prefix, value = m_name.groups()
            new_value = mapping.get(value, value)
            if new_value != value:
                name_replacements += 1
            out.append(f'{prefix}{new_value}\n')
            continue

        out.append(line + '\n')

    updated = ''.join(out)
    original = '\n'.join(lines) + ('\n' if lines else '')
    if updated != original:
        path.write_text(updated, encoding='utf-8')

    return phrase_replacements + key_field_replacements, name_replacements


def main() -> None:
    mapping = load_mapping()
    print(f'mapping_size: {len(mapping)}')

    csv_files_changed = 0
    csv_rows_changed = 0
    for p in CSV_TARGETS:
        f_changed, r_changed = replace_csv_key_column(p, mapping)
        csv_files_changed += f_changed
        csv_rows_changed += r_changed

    cs_files = list_text_files(SCRIPT_DIR, {'.cs'})
    json_files = list_text_files(RESOURCES_DIR, {'.json'})
    asset_files = list_text_files(LEVEL_DATA_DIR, {'.asset'})
    scene_files = [p for p in list_text_files(SCENES_DIR, {'.unity'}) if p.name != 'Level4.unity']
    prefab_files = list_text_files(PREFABS_DIR, {'.prefab'})

    quoted_file_changes = 0
    quoted_replacements = 0
    for p in cs_files + json_files:
        count = replace_quoted_keys(p, mapping)
        if count > 0:
            quoted_file_changes += 1
            quoted_replacements += count

    asset_files_changed = 0
    asset_replacements = 0
    for p in asset_files:
        count = replace_level_data_asset_keys(p, mapping)
        if count > 0:
            asset_files_changed += 1
            asset_replacements += count

    scene_prefab_files_changed = 0
    phrase_replacements = 0
    name_replacements = 0
    for p in scene_files + prefab_files:
        p_count, n_count = replace_scene_prefab_localization_fields(p, mapping)
        if p_count > 0 or n_count > 0:
            scene_prefab_files_changed += 1
            phrase_replacements += p_count
            name_replacements += n_count

    print(f'csv_files_changed: {csv_files_changed}')
    print(f'csv_rows_changed: {csv_rows_changed}')
    print(f'quoted_files_changed: {quoted_file_changes}')
    print(f'quoted_replacements: {quoted_replacements}')
    print(f'asset_files_changed: {asset_files_changed}')
    print(f'asset_replacements: {asset_replacements}')
    print(f'scene_prefab_files_changed: {scene_prefab_files_changed}')
    print(f'phrase_replacements: {phrase_replacements}')
    print(f'name_replacements: {name_replacements}')


if __name__ == '__main__':
    main()
