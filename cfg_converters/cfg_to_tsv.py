#!/usr/bin/env python3
"""
Export ForsakenPowerOverhaul-style CFG to TSV files (no external deps).

Outputs (next to this script by default):
  - ForsakenPowerOverhaul.main.tsv
  - ForsakenPowerOverhaul.grouped.tsv

Main TSV excludes GROUPED_SECTIONS (+EXTRA_SECTIONS_GROUPED) and EXCLUDED_COLUMNS_ON_MAIN.
Grouped TSV contains only GROUPED_SECTIONS + EXTRA_SECTIONS_GROUPED and includes ALL keys.

Headers are written as MULTI-ROW stacked headers (no embedded newlines) so Calc imports cleanly.
"""

from __future__ import annotations
import csv
import re
import sys
from pathlib import Path
from collections import OrderedDict
from typing import List, Dict, Tuple

# ========= USER CONSTANTS =========
INPUT_FILENAME               = "Tidalwave.Valheim.ForsakenPowerOverhaul.cfg"
MAIN_TSV_FILENAME            = "ForsakenPowerOverhaul.main.tsv"
GROUPED_TSV_FILENAME         = "ForsakenPowerOverhaul.grouped.tsv"

# Sections to isolate on their own TSV
GROUPED_SECTIONS             = ["C00 Client", "S00 Server: General"]
EXTRA_SECTIONS_GROUPED       = ["SX1 Server: Corpse Run"]

# Exclude these columns (keys) from the main TSV only
EXCLUDED_COLUMNS_ON_MAIN     = [
    "PowerCycle",
    "Preset",
    "PowerActiveDuration",
    "PowerCooldownDuration",
    "Enable Power Cycling",
    "Enabled",
]
# ==================================

_SECTION_RE = re.compile(r'^\s*\[(?P<name>[^]\r\n]+)\]\s*$')
_KEYVAL_RE  = re.compile(r'^\s*(?P<key>[^=#;]+?)\s*=\s*(?P<val>.*)\s*$')

def read_cfg(path: Path) -> Tuple[List[str], "OrderedDict[str, Dict[str, str]]"]:
    """Parse minimal INI-like cfg; preserve section order; gather global key order."""
    sections: OrderedDict[str, Dict[str, str]] = OrderedDict()
    keys_in_order: List[str] = []
    current_section = None

    with path.open("r", encoding="utf-8", errors="ignore") as f:
        for raw in f:
            line = raw.strip()
            if not line or line.startswith("#") or line.startswith(";"):
                continue
            ms = _SECTION_RE.match(line)
            if ms:
                current_section = ms.group("name")
                sections.setdefault(current_section, {})
                continue
            if current_section:
                mk = _KEYVAL_RE.match(line)
                if mk:
                    key = mk.group("key").strip()
                    val = mk.group("val").strip()
                    sections[current_section][key] = val
                    if key not in keys_in_order:
                        keys_in_order.append(key)
    return keys_in_order, sections

def norm(s: str) -> str:
    return re.sub(r'[^A-Za-z0-9]+', '', (s or '')).lower()

def split_header_lines(title: str) -> List[str]:
    """Split 'MoveSpeedModifier' -> ['Move','Speed','Modifier']; also split on spaces/digits."""
    parts: List[str] = []
    for token in title.split():
        chunks = re.findall(r'[A-Z]+(?=[A-Z][a-z]|$)|[A-Z]?[a-z]+|\d+', token)
        parts.extend(chunks if chunks else [token])
    return parts if parts else [title]

def build_stacked_headers(headers: List[str]) -> List[List[str]]:
    """
    Convert single-row headers into stacked multi-row headers for TSV:
      input:  ["Section", "MoveSpeedModifier", "Enable Power Cycling"]
      output: [
         ["Section", "Move", "Enable"],
         ["",        "Speed", "Power"],
         ["",        "Modifier", "Cycling"]
      ]
    """
    chunks_per_col: List[List[str]] = []
    for h in headers:
        if h == "Section":
            chunks_per_col.append(["Section"])  # single-line header
        else:
            chunks_per_col.append(split_header_lines(h))
    depth = max(len(chunks) for chunks in chunks_per_col) if chunks_per_col else 1

    stacked: List[List[str]] = []
    for row_idx in range(depth):
        row: List[str] = []
        for chunks in chunks_per_col:
            row.append(chunks[row_idx] if row_idx < len(chunks) else "")
        stacked.append(row)
    return stacked

def write_tsv(out_path: Path, headers: List[str], rows: List[List[str]]) -> None:
    out_path.parent.mkdir(parents=True, exist_ok=True)
    with out_path.open("w", encoding="utf-8", newline="") as f:
        w = csv.writer(f, delimiter="\t", quoting=csv.QUOTE_MINIMAL, lineterminator="\n")
        # write stacked headers (multiple header rows)
        for header_row in build_stacked_headers(headers):
            w.writerow(header_row)
        # then write data rows
        w.writerows(rows)
    print(f"Wrote: {out_path}")

def main(argv: List[str]) -> int:
    # Resolve relative to script
    here = Path(__file__).resolve().parent
    in_path   = here / INPUT_FILENAME
    main_tsv  = here / MAIN_TSV_FILENAME
    grp_tsv   = here / GROUPED_TSV_FILENAME

    # Optional CLI overrides
    if len(argv) >= 2:
        p = Path(argv[1]); in_path = p if p.is_absolute() else (Path.cwd() / p).resolve()
    if len(argv) >= 3:
        p = Path(argv[2]); main_tsv = p if p.is_absolute() else (Path.cwd() / p).resolve()
    if len(argv) >= 4:
        p = Path(argv[3]); grp_tsv = p if p.is_absolute() else (Path.cwd() / p).resolve()

    if not in_path.exists():
        print(f"ERROR: input cfg not found: {in_path}", file=sys.stderr)
        return 2

    keys_in_order, sections = read_cfg(in_path)

    excluded_norm = {norm(k) for k in EXCLUDED_COLUMNS_ON_MAIN}
    main_keys     = [k for k in keys_in_order if norm(k) not in excluded_norm]
    main_headers  = ["Section"] + main_keys
    full_headers  = ["Section"] + keys_in_order

    grouped_set = set(GROUPED_SECTIONS) | set(EXTRA_SECTIONS_GROUPED)

    main_rows: List[List[str]] = []
    grouped_rows: List[List[str]] = []

    for section_name, kv in sections.items():
        if section_name in grouped_set:
            grouped_rows.append([section_name] + [kv.get(k, "") for k in keys_in_order])
        else:
            main_rows.append([section_name] + [kv.get(k, "") for k in main_keys])

    write_tsv(main_tsv,   main_headers,  main_rows)
    write_tsv(grp_tsv,    full_headers,  grouped_rows)
    return 0

if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
