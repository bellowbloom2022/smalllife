import csv
import re
from pathlib import Path

ROOT = Path("Assets/LocalizationSource/split")
DRAFT_DIR = Path("Assets/LocalizationSource/split_normalized_draft")
BACKUP_DIR = ROOT / "_backup"

TARGETS = [
    "Common_Global.csv",
    "Menu.csv",
    "Intro.csv",
    "Gameplay_UI.csv",
    "Level_Spot0.csv",
    "Level_Spot1.csv",
    "Level_Spot2.csv",
    "Level_Spot3.csv",
    "Level_Spot4.csv",
]

PREFIX_MAP = {
    "Common_Global.csv": "global",
    "Menu.csv": "menu",
    "Intro.csv": "intro",
    "Gameplay_UI.csv": "gameplay",
    "Level_Spot0.csv": "level0",
    "Level_Spot1.csv": "level1",
    "Level_Spot2.csv": "level2",
    "Level_Spot3.csv": "level3",
    "Level_Spot4.csv": "level4",
}

CAMEL_1 = re.compile(r"(.)([A-Z][a-z]+)")
CAMEL_2 = re.compile(r"([a-z0-9])([A-Z])")

TYPO_ALIAS = {
    "continute": "continue",
    "confirntext": "confirm_text",
    "mstervolume": "master_volume",
    "backtomene": "back_to_menu",
}


def to_snake(raw: str) -> str:
    raw = (raw or "").strip()
    raw = CAMEL_1.sub(r"\1_\2", raw)
    raw = CAMEL_2.sub(r"\1_\2", raw)
    raw = raw.replace("-", "_").replace(".", "_").replace(" ", "_")
    raw = re.sub(r"[^a-zA-Z0-9_]", "_", raw)
    raw = re.sub(r"_+", "_", raw).strip("_")
    raw = raw.lower()
    return TYPO_ALIAS.get(raw, raw)


def ensure_dirs() -> None:
    DRAFT_DIR.mkdir(parents=True, exist_ok=True)
    BACKUP_DIR.mkdir(parents=True, exist_ok=True)


def backup_unsorted() -> None:
    unsorted = ROOT / "Unsorted.csv"
    backup = BACKUP_DIR / "Unsorted_before_cleanup.csv"
    if unsorted.exists():
        backup.write_text(unsorted.read_text(encoding="utf-8"), encoding="utf-8")


def backup_key_map() -> None:
    key_map = ROOT / "key_map_draft.csv"
    backup = BACKUP_DIR / "key_map_draft_verbose_backup.csv"
    if key_map.exists():
        backup.write_text(key_map.read_text(encoding="utf-8"), encoding="utf-8")


def compact_core(source_name: str, core: str) -> str:
    if source_name.startswith("Level_Spot"):
        level_num = source_name.replace("Level_Spot", "").replace(".csv", "")
        core = re.sub(rf"^guide_level{level_num}$", "guide", core)
        core = re.sub(rf"^guide_level{level_num}_", "guide_", core)
        core = re.sub(rf"^spottext{level_num}$", "spot_text", core)
        core = re.sub(rf"^spot{level_num}$", "spot", core)
        core = re.sub(rf"^level{level_num}_", "", core)

    if source_name == "Intro.csv":
        core = re.sub(r"^intro_", "", core)

    core = core.replace("guidetext", "guide_text")
    core = core.replace("goalxdialog", "goalx_dialog")
    core = core.replace("goaldialog", "goal_dialog")
    core = core.replace("spottext", "spot_text")
    core = core.replace("sgoal", "subgoal")
    core = re.sub(r"_+", "_", core).strip("_")
    return core


def normalize_file(name: str, key_map_rows: list[list[str]]) -> int:
    src = ROOT / name
    if not src.exists():
        return 0

    with src.open("r", encoding="utf-8-sig", newline="") as f:
        reader = csv.DictReader(f)
        rows = list(reader)
        fields = reader.fieldnames or []

    prefix = PREFIX_MAP[name]
    seen: dict[str, int] = {}
    out_rows: list[dict[str, str]] = []

    for row_index, row in enumerate(rows, start=2):
        old_key = (row.get("key") or "").strip()

        if not old_key:
            key_map_rows.append([name, str(row_index), old_key, "", "manual_review", "empty_key"])
            out_rows.append(row)
            continue

        core = to_snake(old_key)
        if not core:
            key_map_rows.append([name, str(row_index), old_key, "", "manual_review", "invalid_after_normalize"])
            out_rows.append(row)
            continue

        core = compact_core(name, core)
        if not core:
            key_map_rows.append([name, str(row_index), old_key, "", "manual_review", "empty_core_after_compact"])
            out_rows.append(row)
            continue

        new_key = f"{prefix}_{core}"
        count = seen.get(new_key, 0)
        if count > 0:
            new_key = f"{new_key}_{count + 1:03d}"
            status = "auto_renamed_with_suffix"
            reason = "duplicate_collision_suffix_added"
        else:
            status = "auto_renamed" if new_key != old_key else "keep"
            reason = "normalized"
        seen[new_key] = count + 1

        updated = dict(row)
        updated["key"] = new_key
        out_rows.append(updated)
        key_map_rows.append([name, str(row_index), old_key, new_key, status, reason])

    out_path = DRAFT_DIR / name
    with out_path.open("w", encoding="utf-8", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=fields)
        writer.writeheader()
        writer.writerows(out_rows)

    return len(out_rows)


def write_key_map(key_map_rows: list[list[str]]) -> None:
    map_path = ROOT / "key_map_draft.csv"
    with map_path.open("w", encoding="utf-8", newline="") as f:
        writer = csv.writer(f)
        writer.writerow(["source_file", "row", "old_key", "new_key", "status", "reason"])
        writer.writerows(key_map_rows)


def cleanup_unsorted() -> None:
    unsorted = ROOT / "Unsorted.csv"
    default_header = ["key_type", "key", "avatar", "zh-CN", "ja", "en", "source_file"]

    if unsorted.exists():
        with unsorted.open("r", encoding="utf-8-sig", newline="") as f:
            reader = csv.reader(f)
            header = next(reader, default_header)
    else:
        header = default_header

    with unsorted.open("w", encoding="utf-8", newline="") as f:
        writer = csv.writer(f)
        writer.writerow(header)


def main() -> None:
    ensure_dirs()
    backup_unsorted()
    backup_key_map()

    key_map_rows: list[list[str]] = []
    counts: list[tuple[str, int]] = []

    for name in TARGETS:
        count = normalize_file(name, key_map_rows)
        counts.append((name, count))

    write_key_map(key_map_rows)
    cleanup_unsorted()

    print("Phase 2 complete")
    print(f"key_map: {(ROOT / 'key_map_draft.csv').as_posix()}")
    for name, count in counts:
        print(f"draft {name}: {count}")


if __name__ == "__main__":
    main()
