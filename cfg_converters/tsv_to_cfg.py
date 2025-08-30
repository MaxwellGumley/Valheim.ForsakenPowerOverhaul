#!/usr/bin/env python3
"""
Update a ForsakenPowerOverhaul-style CFG from TSV file(s), preserving comments & formatting.

Reads (if present; order matters):
  1) ForsakenPowerOverhaul.main.tsv
  2) ForsakenPowerOverhaul.grouped.tsv
Later files override earlier ones for the same Section/Key.

Supports *stacked header rows* (multi-row headers). It reconstructs each column's key by
concatenating the stacked header cells in that column, then normalizes to match CFG keys.
"""

from __future__ import annotations
import csv
import re
import sys
from pathlib import Path
from typing import Dict, List, Tuple

# ========= USER CONSTANTS =========
CFG_FILENAME               = "Tidalwave.Valheim.ForsakenPowerOverhaul.cfg"
MAIN_TSV_FILENAME          = "ForsakenPowerOverhaul.main.tsv"
GROUPED_TSV_FILENAME       = "ForsakenPowerOverhaul.grouped.tsv"

BACKUP_SUFFIX              = ".bak"
IGNORE_EMPTY_CELLS         = True  # True: keep CFG value if TSV cell is empty
# ==================================

# CFG parsing/patching
_SECTION_RE = re.compile(r'^\s*\[(?P<name>[^\]\r\n]+)\]\s*$')
_KV_RE      = re.compile(r'^(\s*)([^=#;]+?)(\s*=\s*)(.*?)(\s*)$')

def normalize(s: str) -> str:
    return re.sub(r'[^A-Za-z0-9]+', '', (s or '')).lower()

def split_value_and_inline_comment(val_part: str) -> Tuple[str, str]:
    """Split trailing inline comment while respecting quotes."""
    s = val_part
    in_single = False
    in_double = False
    for i, ch in enumerate(s):
        if ch == "'" and not in_double:
            in_single = not in_single
        elif ch == '"' and not in_single:
            in_double = not in_double
        elif (ch in '#;') and not in_single and not in_double:
            return s[:i].rstrip(), s[i:]
    return s.rstrip(), ""

def read_cfg_index(lines: List[str]) -> Tuple[Dict[str, Dict[str, int]], Dict[str, str]]:
    """Index cfg lines: section->key->line_index, and normalized key->original key (first seen)."""
    index: Dict[str, Dict[str, int]] = {}
    key_canon: Dict[str, str] = {}
    current = None
    for i, raw in enumerate(lines):
        m = _SECTION_RE.match(raw.strip())
        if m:
            current = m.group("name")
            index.setdefault(current, {})
            continue
        if current is None:
            continue
        mk = _KV_RE.match(raw.rstrip("\n"))
        if mk:
            key = mk.group(2).strip()
            index[current][key] = i
            key_canon.setdefault(normalize(key), key)
    return index, key_canon

def detect_header_rows_and_section_col(rows: List[List[str]]) -> Tuple[int, int]:
    """
    Return (header_rows_count, section_col_index).
    Heuristic:
      - Find a column where ANY row's cell (from top) equals 'Section' (case/space-insensitive).
      - Count consecutive top rows where that column is either 'Section' or empty -> header rows.
      - First row where that column is NOT 'Section' or empty is considered the first data row.
    """
    if not rows:
        return 0, -1

    # Candidate columns where top row says 'Section' (normalized)
    norm_top = [normalize(c) for c in rows[0]]
    candidates = [i for i, v in enumerate(norm_top) if v == "section"]
    if not candidates:
        # fallback: search first few rows
        for i in range(min(5, len(rows))):
            for j, v in enumerate(rows[i]):
                if normalize(v) == "section":
                    candidates.append(j)
                    break
            if candidates:
                break
    if not candidates:
        return 0, -1

    section_col = candidates[0]

    # Count header rows
    header_rows = 0
    for r in rows:
        cell = r[section_col] if section_col < len(r) else ""
        nv = normalize(cell)
        if nv in ("section", ""):
            header_rows += 1
        else:
            break
    return header_rows, section_col

def reconstruct_headers_from_stacked(rows: List[List[str]],
                                     header_rows: int) -> List[str]:
    """
    Concatenate stacked header cells column-wise to form full header strings.
    Example stacked:
      col0: ["Section", "", ""]
      col1: ["Move", "Speed", "Modifier"]
    -> ["Section", "MoveSpeedModifier"]
    """
    if header_rows <= 0:
        return []
    cols = max(len(r) for r in rows[:header_rows]) if rows else 0
    headers: List[str] = []
    for c in range(cols):
        parts: List[str] = []
        for r in range(header_rows):
            cell = rows[r][c] if c < len(rows[r]) else ""
            parts.append(cell.strip())
        # join without spaces to recreate the original key name
        joined = "".join(parts).strip() or ""
        headers.append(joined if joined else "")
    return headers

def read_tsv_updates(tsv_path: Path,
                     cfg_key_canon: Dict[str, str]) -> Dict[str, Dict[str, str]]:
    """Read one TSV and return updates[section][cfg_key] = value."""
    updates: Dict[str, Dict[str, str]] = {}
    if not tsv_path.exists():
        return updates

    with tsv_path.open("r", encoding="utf-8", newline="") as f:
        reader = csv.reader(f, delimiter="\t")
        rows = [row for row in reader]

    if not rows:
        return updates

    header_rows, section_col = detect_header_rows_and_section_col(rows)
    if header_rows == 0 or section_col < 0:
        return updates  # not a recognized TSV format

    full_headers = reconstruct_headers_from_stacked(rows, header_rows)
    header_norms = [normalize(h) for h in full_headers]

    # Map columns to canonical CFG keys
    col_to_key: Dict[int, str] = {}
    for idx, hnorm in enumerate(header_norms):
        if idx == section_col or not hnorm:
            continue
        cfg_key = cfg_key_canon.get(hnorm)
        if cfg_key:
            col_to_key[idx] = cfg_key

    # Data rows follow header_rows
    for row in rows[header_rows:]:
        if section_col >= len(row):
            continue
        section = (row[section_col] or "").strip()
        if not section:
            continue
        for col_i, cfg_key in col_to_key.items():
            if col_i >= len(row):
                continue
            val = row[col_i]
            # If cell is empty, set value to empty string (do not skip)
            if val == "":
                updates.setdefault(section, {})[cfg_key] = ""
            else:
                updates.setdefault(section, {})[cfg_key] = val

    return updates

def main(argv: List[str]) -> int:
    here   = Path(__file__).resolve().parent
    cfg    = here / CFG_FILENAME
    main_t = here / MAIN_TSV_FILENAME
    grp_t  = here / GROUPED_TSV_FILENAME

    # Optional CLI overrides: [cfg] [main.tsv] [grouped.tsv]
    if len(argv) >= 2:
        p = Path(argv[1]); cfg = p if p.is_absolute() else (Path.cwd() / p).resolve()
    if len(argv) >= 3:
        p = Path(argv[2]); main_t = p if p.is_absolute() else (Path.cwd() / p).resolve()
    if len(argv) >= 4:
        p = Path(argv[3]); grp_t  = p if p.is_absolute() else (Path.cwd() / p).resolve()

    if not cfg.exists():
        print(f"ERROR: cfg not found: {cfg}", file=sys.stderr)
        return 2

    original = cfg.read_text(encoding="utf-8", errors="ignore")
    lines = original.splitlines(True)

    idx, key_canon = read_cfg_index(lines)

    # Apply updates; grouped last (wins)
    updates_main = read_tsv_updates(main_t, key_canon)
    updates_grp  = read_tsv_updates(grp_t,  key_canon)
    updates: Dict[str, Dict[str, str]] = {}
    for src in (updates_main, updates_grp):
        for sect, kv in src.items():
            updates.setdefault(sect, {}).update(kv)

    if not updates:
        print("No updates found from TSV files; nothing to do.")
        return 0

    # No backup file generation

    # Apply changes
    for section, kv in updates.items():
        pass  # no-op; ensures dicts exist

    # do the actual update
    for section, kv in updates.items():
        pass

    # simpler: reuse function
    def apply(lines_: List[str]) -> None:
        for section, kvs in updates.items():
            sk = idx.get(section)
            if not sk:
                continue
            for key, new_val in kvs.items():
                line_i = sk.get(key)
                if line_i is None:
                    continue
                orig = lines_[line_i].rstrip("\n")
                mk = _KV_RE.match(orig)
                if not mk:
                    continue
                lead_ws, key_txt, eq_ws, val_tail, trail_ws = mk.groups()
                _, comment = split_value_and_inline_comment(val_tail)
                safe_val = new_val.replace("\r\n", "\n").replace("\r", "\n")
                if "\n" in safe_val:
                    safe_val = safe_val.replace("\n", r'\n')
                lines_[line_i] = f"{lead_ws}{key_txt}{eq_ws}{safe_val}{comment}{trail_ws}\n"

    apply(lines)
    cfg.write_text("".join(lines), encoding="utf-8")
    print(f"Updated: {cfg}")
    return 0

if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
