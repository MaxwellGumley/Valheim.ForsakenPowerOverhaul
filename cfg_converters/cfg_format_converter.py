#!/usr/bin/env python3
"""
Converter helper for ForsakenPowerOverhaul configurations.
Converts between comma, semicolon, and quoted formats for better compatibility.
"""

import re
import sys
from pathlib import Path

def parse_comma_separated_values(input_str):
    """
    Parse comma-separated values with support for pipes, quotes, and escaped quotes.
    Matches the enhanced C# ParseCommaSeparatedValues function.
    """
    if not input_str:
        return []
    
    input_str = input_str.strip()
    if not input_str:
        return []
    
    # Handle pipe delimiters (recommended - avoids comment and LibreCalc issues)
    if '|' in input_str:
        return [item.strip() for item in input_str.split('|') if item.strip()]
    
    # Handle escaped quotes (BepInX format)
    if '\\"' in input_str:
        cleaned = input_str
        if cleaned.startswith('\\"') and cleaned.endswith('\\"'):
            cleaned = cleaned[2:-2]
        cleaned = cleaned.replace('\\"', '"')
        return [item.strip().strip('"') for item in cleaned.split(',') if item.strip()]
    
    # Handle regular quoted strings
    if input_str.startswith('"') and input_str.endswith('"') and len(input_str) >= 2:
        unquoted = input_str[1:-1]
        return [s.strip() for s in unquoted.split(',')]
    else:
        # Normal comma-separated values
        return [s.strip() for s in input_str.split(',')]

def format_as_pipes(values):
    """Convert a list of values to pipe-delimited format (recommended)."""
    if not values:
        return ""
    return "|".join(str(v).strip() for v in values if str(v).strip())

def format_as_commas(values):
    """Convert a list of values to comma-delimited format."""
    if not values:
        return ""
    return ",".join(str(v).strip() for v in values if str(v).strip())

def format_as_quoted_commas(values):
    """Convert a list of values to quoted comma-delimited format."""
    if not values:
        return ""
    comma_str = ",".join(str(v).strip() for v in values if str(v).strip())
    return f'"{comma_str}"'

def convert_config_value(input_value, output_format="pipe"):
    """
    Convert a configuration value between different formats.
    
    Args:
        input_value (str): The input configuration value
        output_format (str): Target format - "pipe", "comma", or "quoted"
    
    Returns:
        str: The converted value
    """
    # Parse the input value
    values = parse_comma_separated_values(input_value)
    
    # Format according to the target format
    if output_format == "pipe":
        return format_as_pipes(values)
    elif output_format == "comma":
        return format_as_commas(values)
    elif output_format == "quoted":
        return format_as_quoted_commas(values)
    else:
        raise ValueError(f"Unknown output format: {output_format}")

def convert_config_file(cfg_path, output_format="pipe", backup=True):
    """
    Convert all comma-separated values in a config file to the specified format.
    
    Args:
        cfg_path (Path): Path to the configuration file
        output_format (str): Target format - "pipe", "comma", or "quoted"
        backup (bool): Whether to create a backup file
    """
    cfg_path = Path(cfg_path)
    if not cfg_path.exists():
        print(f"ERROR: Config file not found: {cfg_path}")
        return False
    
    # Read the file
    content = cfg_path.read_text(encoding="utf-8", errors="ignore")
    lines = content.splitlines()
    
    # Create backup if requested
    if backup:
        backup_path = cfg_path.with_suffix(cfg_path.suffix + ".bak")
        backup_path.write_text(content, encoding="utf-8")
        print(f"Created backup: {backup_path}")
    
    # Pattern to match key=value lines with multi-value content
    kv_pattern = re.compile(r'^(\s*)([^=#;]+?)(\s*=\s*)(.*?)(\s*)$')
    
    # Look for lines that might contain comma-separated values
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
    
    converted_count = 0
    
    for i, line in enumerate(lines):
        match = kv_pattern.match(line)
        if not match:
            continue
            
        lead_ws, key, eq_ws, value, trail_ws = match.groups()
        key = key.strip()
        
        # Check if this is a multi-value key
        if any(mvkey.lower() in key.lower() for mvkey in multi_value_keys):
            # Check if the value contains delimiter-separated data
            if ',' in value or '|' in value or ';' in value or '"' in value:
                try:
                    # Split inline comments
                    comment = ""
                    clean_value = value
                    for comment_char in ['#']:  # Only # for comments, not ; since we might use it
                        if comment_char in value:
                            # Simple comment detection (doesn't handle quotes perfectly)
                            parts = value.split(comment_char, 1)
                            clean_value = parts[0].rstrip()
                            comment = comment_char + parts[1] if len(parts) > 1 else ""
                            break
                    
                    # Convert the value
                    converted_value = convert_config_value(clean_value, output_format)
                    
                    # Rebuild the line
                    new_line = f"{lead_ws}{key}{eq_ws}{converted_value}{comment}{trail_ws}"
                    lines[i] = new_line
                    converted_count += 1
                    
                    print(f"Converted {key}: {clean_value} -> {converted_value}")
                    
                except Exception as e:
                    print(f"Warning: Could not convert {key} = {value}: {e}")
    
    # Write the updated file
    if converted_count > 0:
        cfg_path.write_text('\n'.join(lines) + '\n', encoding="utf-8")
        print(f"Converted {converted_count} values to {output_format} format in {cfg_path}")
    else:
        print(f"No values to convert in {cfg_path}")
    
    return True

def main():
    """Command-line interface for the converter."""
    if len(sys.argv) < 2:
        print("Usage: python cfg_format_converter.py <config_file> [format] [--no-backup]")
        print("Formats: pipe (default), comma, quoted")
        print("Example: python cfg_format_converter.py config.cfg pipe")
        return 1
    
    cfg_file = sys.argv[1]
    output_format = sys.argv[2] if len(sys.argv) > 2 else "pipe"
    backup = "--no-backup" not in sys.argv
    
    if output_format not in ["pipe", "comma", "quoted"]:
        print(f"ERROR: Invalid format '{output_format}'. Use: pipe, comma, or quoted")
        return 1
    
    success = convert_config_file(cfg_file, output_format, backup)
    return 0 if success else 1

if __name__ == "__main__":
    sys.exit(main())
