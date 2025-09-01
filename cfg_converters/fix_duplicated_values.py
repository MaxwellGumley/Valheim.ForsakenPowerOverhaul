#!/usr/bin/env python3
"""
Fix duplicated values in config file caused by the semicolon conversion bug.
Converts semicolon-delimited values to pipe-delimited and fixes duplications.
"""

import re
import sys
from pathlib import Path

def fix_duplicated_values(value_str):
    """Fix duplicated values like '85;85;85;85;85;85;85;85;85' back to proper format."""
    if not value_str or not ';' in value_str:
        return value_str
    
    # Split by semicolon and get unique values while preserving order
    parts = [p.strip() for p in value_str.split(';') if p.strip()]
    
    if not parts:
        return value_str
    
    # If all parts are the same, it's likely duplicated
    if len(set(parts)) == 1:
        # Guess original count - common patterns are 3, 5, or based on length
        original_count = 5 if len(parts) > 6 else 3
        if len(parts) == 9:  # 5 became 9
            original_count = 5
        elif len(parts) == 6:  # 3 became 6  
            original_count = 3
        else:
            # Use half the current count as estimate
            original_count = max(3, len(parts) // 2)
        
        # Return pipe-delimited with estimated original count
        return '|'.join([parts[0]] * original_count)
    
    # If different values, convert all to pipe format
    return '|'.join(parts)

def fix_config_file(cfg_path, backup=True):
    """Fix duplicated semicolon values in config file."""
    cfg_path = Path(cfg_path)
    if not cfg_path.exists():
        print(f"ERROR: Config file not found: {cfg_path}")
        return False
    
    content = cfg_path.read_text(encoding="utf-8", errors="ignore")
    lines = content.splitlines()
    
    if backup:
        backup_path = cfg_path.with_suffix(cfg_path.suffix + ".pre-fix-bak")
        backup_path.write_text(content, encoding="utf-8")
        print(f"Created backup: {backup_path}")
    
    kv_pattern = re.compile(r'^(\s*)([^=#;]+?)(\s*=\s*)(.*?)(\s*)$')
    
    multi_value_keys = [
        "IncomingDamageMultiplierModifiers",
        "OutgoingDamageMultiplierModifiers", 
        "IncomingDamageMultiplierTypes",
        "OutgoingDamageMultiplierTypes",
        "IncomingDamageTypes",
        "OutgoingDamageTypes", 
        "IncomingDamageModifiers",
        "OutgoingDamageModifiers",
        "StatusAttributes"
    ]
    
    fixed_count = 0
    
    for i, line in enumerate(lines):
        match = kv_pattern.match(line)
        if not match:
            continue
            
        lead_ws, key, eq_ws, value, trail_ws = match.groups()
        key = key.strip()
        
        # Check if this is a multi-value key with semicolons
        if any(mvkey.lower() in key.lower() for mvkey in multi_value_keys):
            if ';' in value:
                try:
                    # Split inline comments
                    comment = ""
                    clean_value = value
                    if '#' in value:
                        parts = value.split('#', 1)
                        clean_value = parts[0].rstrip()
                        comment = '#' + parts[1] if len(parts) > 1 else ""
                    
                    # Fix the duplicated value
                    fixed_value = fix_duplicated_values(clean_value)
                    
                    if fixed_value != clean_value:
                        # Rebuild the line
                        new_line = f"{lead_ws}{key}{eq_ws}{fixed_value}{comment}{trail_ws}"
                        lines[i] = new_line
                        fixed_count += 1
                        
                        print(f"Fixed {key}: {clean_value} -> {fixed_value}")
                        
                except Exception as e:
                    print(f"Warning: Could not fix {key} = {value}: {e}")
    
    if fixed_count > 0:
        cfg_path.write_text('\n'.join(lines) + '\n', encoding="utf-8")
        print(f"Fixed {fixed_count} duplicated values in {cfg_path}")
    else:
        print(f"No duplicated values found to fix in {cfg_path}")
    
    return True

def main():
    if len(sys.argv) < 2:
        print("Usage: python fix_duplicated_values.py <config_file>")
        print("Fixes duplicated semicolon values and converts to pipe format")
        return 1
    
    cfg_file = sys.argv[1]
    success = fix_config_file(cfg_file)
    return 0 if success else 1

if __name__ == "__main__":
    sys.exit(main())
