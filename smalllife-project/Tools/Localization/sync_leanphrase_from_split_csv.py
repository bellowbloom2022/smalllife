import csv
import re
from pathlib import Path
from typing import Optional

ROOT = Path('.')
SPLIT_DIR = ROOT / 'Assets/LocalizationSource/split'
FALLBACK_SPLIT_DIR = ROOT / 'Assets/LocalizationSource/split_normalized_draft'
TARGET_DIRS = [ROOT / 'Assets/Scenes', ROOT / 'Assets/Prefabs']
TARGET_SUFFIXES = {'.unity', '.prefab'}

CSV_FILES = [
    'Common_Global.csv',
    'Menu.csv',
    'Intro.csv',
    'Gameplay_UI.csv',
    'Level_Spot0.csv',
    'Level_Spot1.csv',
    'Level_Spot2.csv',
    'Level_Spot3.csv',
    'Level_Spot4.csv',
]

LANG_TO_COL = {
    'English': 'en',
    'Chinese': 'zh-CN',
    'Japanese': 'ja',
}


def yaml_quote(text: str) -> str:
    # Keep multiline text in one YAML scalar with escaped newlines.
    escaped = text.replace('\\', '\\\\').replace('"', '\\"').replace('\r\n', '\n').replace('\r', '\n').replace('\n', '\\n')
    return f'"{escaped}"'


def _read_csv_map(base_dir: Path) -> dict[str, dict[str, Optional[str]]]:
    out: dict[str, dict[str, Optional[str]]] = {}

    if not base_dir.exists():
        return out

    for name in CSV_FILES:
        path = base_dir / name
        if not path.exists():
            continue

        with path.open('r', encoding='utf-8-sig', newline='') as f:
            reader = csv.DictReader(f)
            for row in reader:
                key = (row.get('key') or '').strip()
                if not key:
                    continue

                en = row.get('en')
                source_file = row.get('source_file')
                extras = row.get(None) or []

                # Repair malformed CSV rows where commas in English split into source_file.
                # Pattern observed:
                # en='left part', source_file='right part', extras=['LevelX.csv']
                if extras:
                    if en and source_file:
                        en = f'{en},{source_file}'
                    elif source_file and not en:
                        en = source_file

                out[key] = {
                    'English': en,
                    'Chinese': row.get('zh-CN'),
                    'Japanese': row.get('ja'),
                }

    return out


def load_translation_map() -> dict[str, dict[str, Optional[str]]]:
    primary = _read_csv_map(SPLIT_DIR)
    fallback = _read_csv_map(FALLBACK_SPLIT_DIR)

    out: dict[str, dict[str, Optional[str]]] = {}
    all_keys = set(primary.keys()) | set(fallback.keys())

    for key in all_keys:
        out[key] = {}
        for lang in ('English', 'Chinese', 'Japanese'):
            p = (primary.get(key) or {}).get(lang)
            f = (fallback.get(key) or {}).get(lang)
            out[key][lang] = p if p is not None else f

    return out


def split_sections(lines: list[str]) -> list[tuple[int, int]]:
    starts = [i for i, l in enumerate(lines) if l.startswith('--- !u!')]
    if not starts:
        return []
    sections = []
    for idx, s in enumerate(starts):
        e = starts[idx + 1] if idx + 1 < len(starts) else len(lines)
        sections.append((s, e))
    return sections


def get_gameobject_names(lines: list[str], sections: list[tuple[int, int]]) -> dict[str, str]:
    id_to_name: dict[str, str] = {}
    id_pat = re.compile(r'^--- !u!1 &(\d+)')
    name_pat = re.compile(r'^\s*m_Name:\s*(\S.*?)\s*$')

    for s, e in sections:
        header = lines[s]
        m_id = id_pat.match(header)
        if not m_id:
            continue

        file_id = m_id.group(1)
        name = None
        for i in range(s + 1, e):
            m_name = name_pat.match(lines[i])
            if m_name:
                name = m_name.group(1)
                break

        if name:
            id_to_name[file_id] = name

    return id_to_name


def update_mono_section(lines: list[str], s: int, e: int, key: str, tmap: dict[str, dict[str, Optional[str]]]) -> int:
    gameobj_pat = re.compile(r'^\s*m_GameObject:\s*\{fileID:\s*(\d+)\}')
    lang_pat = re.compile(r'^\s*-\s*Language:\s*(.+?)\s*$')
    text_pat = re.compile(r'^(\s*)Text:\s*(.*)$')

    changed = 0
    current_lang = None

    i = s
    while i < e:
        line = lines[i]

        m_lang = lang_pat.match(line)
        if m_lang:
            current_lang = m_lang.group(1)
            i += 1
            continue

        m_text = text_pat.match(line)
        if m_text and current_lang in LANG_TO_COL:
            indent = m_text.group(1)
            new_text = tmap[key][current_lang]
            if new_text is None:
                i += 1
                continue
            new_line = f'{indent}Text: {yaml_quote(new_text)}'
            if new_line != line:
                lines[i] = new_line
                changed += 1

            # Drop multiline continuation lines under Text scalar if any.
            base_indent = len(indent)
            j = i + 1
            while j < e:
                nxt = lines[j]
                if re.match(r'^\s*-\s*Language:', nxt):
                    break
                if re.match(r'^\s*Object:\s*', nxt):
                    break
                if nxt.startswith('--- !u!'):
                    break
                if len(nxt) - len(nxt.lstrip(' ')) > base_indent:
                    del lines[j]
                    e -= 1
                    changed += 1
                    continue
                break

            i += 1
            continue

        i += 1

    return changed


def process_file(path: Path, tmap: dict[str, dict[str, Optional[str]]]) -> tuple[int, int]:
    lines = path.read_text(encoding='utf-8', errors='ignore').splitlines()
    sections = split_sections(lines)
    if not sections:
        return 0, 0

    id_to_name = get_gameobject_names(lines, sections)

    mono_header = re.compile(r'^--- !u!114 &(\d+)')
    gameobj_pat = re.compile(r'^\s*m_GameObject:\s*\{fileID:\s*(\d+)\}')

    blocks_changed = 0
    lines_changed = 0

    for s, e in sections:
        if not mono_header.match(lines[s]):
            continue

        game_obj_id = None
        has_entries = False
        for i in range(s + 1, e):
            m_go = gameobj_pat.match(lines[i])
            if m_go:
                game_obj_id = m_go.group(1)
            if lines[i].strip() == 'entries:':
                has_entries = True

        if not game_obj_id or not has_entries:
            continue

        key = id_to_name.get(game_obj_id)
        if not key or key not in tmap:
            continue

        c = update_mono_section(lines, s, e, key, tmap)
        if c > 0:
            blocks_changed += 1
            lines_changed += c

    if lines_changed > 0:
        path.write_text('\n'.join(lines) + '\n', encoding='utf-8')

    return blocks_changed, lines_changed


def collect_targets() -> list[Path]:
    out: list[Path] = []
    for base in TARGET_DIRS:
        if not base.exists():
            continue
        for p in base.rglob('*'):
            if p.is_file() and p.suffix.lower() in TARGET_SUFFIXES:
                out.append(p)
    return out


def main() -> None:
    tmap = load_translation_map()
    targets = collect_targets()

    files_changed = 0
    blocks_changed = 0
    lines_changed = 0

    for p in targets:
        b, l = process_file(p, tmap)
        if l > 0:
            files_changed += 1
            blocks_changed += b
            lines_changed += l

    print(f'keys_loaded: {len(tmap)}')
    print(f'target_files_scanned: {len(targets)}')
    print(f'files_changed: {files_changed}')
    print(f'phrase_blocks_changed: {blocks_changed}')
    print(f'lines_changed: {lines_changed}')


if __name__ == '__main__':
    main()
