import csv
from pathlib import Path

ROOT = Path('.')
MAP_PATH = Path('Assets/LocalizationSource/split/key_map_draft.csv')
OUT_PATH = Path('Assets/LocalizationSource/split/key_reference_report.csv')

SEARCH_EXTS = {'.cs', '.json', '.unity', '.prefab', '.asset'}
EXCLUDE_PARTS = {
    'Library',
    'Temp',
    'Logs',
    'Obj',
    '.git',
    'Assets/LocalizationSource/split_normalized_draft',
}


def should_scan(path: Path) -> bool:
    s = path.as_posix()
    for part in EXCLUDE_PARTS:
        if part in s:
            return False
    return path.suffix.lower() in SEARCH_EXTS


def load_keys() -> list[tuple[str, str]]:
    rows = []
    with MAP_PATH.open('r', encoding='utf-8-sig', newline='') as f:
        reader = csv.DictReader(f)
        for r in reader:
            old_key = (r.get('old_key') or '').strip()
            new_key = (r.get('new_key') or '').strip()
            if old_key and new_key and old_key != new_key:
                rows.append((old_key, new_key))
    # de-duplicate while keeping order
    seen = set()
    ordered = []
    for k in rows:
        if k[0] in seen:
            continue
        seen.add(k[0])
        ordered.append(k)
    return ordered


def main() -> None:
    keys = load_keys()
    files = [p for p in ROOT.rglob('*') if p.is_file() and should_scan(p)]

    report_rows = []
    for old_key, new_key in keys:
        total = 0
        touched_files = 0
        for fp in files:
            try:
                text = fp.read_text(encoding='utf-8', errors='ignore')
            except Exception:
                continue
            c = text.count(old_key)
            if c > 0:
                touched_files += 1
                total += c
        if total > 0:
            report_rows.append([old_key, new_key, str(total), str(touched_files)])

    with OUT_PATH.open('w', encoding='utf-8', newline='') as f:
        writer = csv.writer(f)
        writer.writerow(['old_key', 'new_key', 'total_occurrences', 'files_affected'])
        writer.writerows(sorted(report_rows, key=lambda x: int(x[2]), reverse=True))

    print(f'reportable_keys: {len(report_rows)}')
    print(f'report_path: {OUT_PATH.as_posix()}')


if __name__ == '__main__':
    main()
