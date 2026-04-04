import csv
import re
from pathlib import Path

KEYS = {
    'global_wintext',
    'global_wintext2',
    'level0_goalx_dialog1',
    'level0_goalx_dialog111',
    'level1_goal0',
}


def load_csv_values():
    print('== CSV values ==')
    for f in ['Common_Global.csv', 'Level_Spot0.csv', 'Level_Spot1.csv']:
        p = Path('Assets/LocalizationSource/split') / f
        if not p.exists():
            continue
        with p.open('r', encoding='utf-8-sig', newline='') as fh:
            rd = csv.DictReader(fh)
            for r in rd:
                k = (r.get('key') or '').strip()
                if k not in KEYS:
                    continue
                extra = r.get(None) or []
                en = r.get('en')
                sf = r.get('source_file')
                if extra and en and sf:
                    en = en + ',' + sf
                print(f'{f}:{k} | zh={r.get("zh-CN")!r} | ja={r.get("ja")!r} | en={en!r} | extras={extra}')


def scan_phrase_blocks():
    print('\n== Scene/Prefab phrase blocks ==')
    files = list(Path('Assets/Scenes').rglob('*.unity')) + list(Path('Assets/Prefabs').rglob('*.prefab'))
    for fp in files:
        lines = fp.read_text(encoding='utf-8', errors='ignore').splitlines()
        i = 0
        while i < len(lines):
            m_name = re.match(r'^\s*m_Name:\s*(\S.*?)\s*$', lines[i])
            if m_name:
                name = m_name.group(1)
                if name in KEYS:
                    j = i
                    while j < min(len(lines), i + 120) and lines[j].strip() != 'entries:':
                        j += 1
                    if j < min(len(lines), i + 120) and lines[j].strip() == 'entries:':
                        vals = {}
                        cur = None
                        k = j + 1
                        while k < len(lines):
                            mm_lang = re.match(r'^\s*-\s*Language:\s*(.+?)\s*$', lines[k])
                            if mm_lang:
                                cur = mm_lang.group(1)
                            mm_text = re.match(r'^\s*Text:\s*(.*)$', lines[k])
                            if mm_text and cur:
                                vals[cur] = mm_text.group(1)
                            if lines[k].startswith('--- !u!'):
                                break
                            k += 1
                        print(f'{fp}:{i + 1}:{name} -> {vals}')
            i += 1


if __name__ == '__main__':
    load_csv_values()
    scan_phrase_blocks()
