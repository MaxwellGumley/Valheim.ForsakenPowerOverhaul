#!/usr/bin/env python3
"""
Test script to verify that the C# ParseCommaSeparatedValues function works correctly.
This simulates the parsing logic to validate our approach with semicolon delimiter support.
"""

def parse_comma_separated_values(input_str):
    """
    Python version of the enhanced C# ParseCommaSeparatedValues function for testing.
    Supports semicolon delimiters (recommended), escaped quotes, and comma delimiters.
    """
    if not input_str:
        return []
    
    # Trim the input
    input_str = input_str.strip()
    
    # Check if trimmed input is empty
    if not input_str:
        return []
    
    # Handle pipe delimiters (recommended - avoids comment conflicts)
    if '|' in input_str:
        return [item.strip() for item in input_str.split('|') if item.strip()]
    
    # Handle escaped quotes (what BepInX creates when saving quoted strings)
    if '\\"' in input_str:
        # Remove outer quotes if present and unescape inner quotes
        cleaned = input_str
        if cleaned.startswith('\\"') and cleaned.endswith('\\"'):
            cleaned = cleaned[2:-2]  # Remove outer escaped quotes
        # Replace escaped quotes with regular quotes, then split by comma
        cleaned = cleaned.replace('\\"', '"')
        return [item.strip().strip('"') for item in cleaned.split(',') if item.strip()]
    
    # Handle regular quoted strings
    if input_str.startswith('"') and input_str.endswith('"') and len(input_str) >= 2:
        # Remove outer quotes and split
        unquoted = input_str[1:-1]
        return [s.strip() for s in unquoted.split(',')]
    else:
        # Normal comma-separated values
        return [s.strip() for s in input_str.split(',')]

def test_parsing():
    """Test various input formats including semicolon delimiters"""
    test_cases = [
        # Pipe format (recommended - no comment conflicts)
        ('85|85|85', ['85', '85', '85'], 'Pipe delimiter - numbers'),
        ('Pierce|Slash|Blunt', ['Pierce', 'Slash', 'Blunt'], 'Pipe delimiter - strings'),
        (' Fire | Frost | Lightning ', ['Fire', 'Frost', 'Lightning'], 'Pipe with spaces'),
        ('85|', ['85'], 'Trailing pipe'),
        ('|85', ['85'], 'Leading pipe'),
        
        # Original comma format
        ('85,85,85', ['85', '85', '85'], 'Normal comma-separated values'),
        ('"85,85,85"', ['85', '85', '85'], 'Quoted comma-separated values'),
        ('Pierce,Slash,Blunt', ['Pierce', 'Slash', 'Blunt'], 'Normal string values'),
        ('"Pierce,Slash,Blunt"', ['Pierce', 'Slash', 'Blunt'], 'Quoted string values'),
        ('150,150,150,150,150', ['150', '150', '150', '150', '150'], 'Multiple values'),
        ('"150,150,150,150,150"', ['150', '150', '150', '150', '150'], 'Quoted multiple values'),
        
        # BepInX escaped format
        ('\\"85,85,85\\"', ['85', '85', '85'], 'BepInX escaped quotes'),
        ('\\"Pierce,Slash,Blunt\\"', ['Pierce', 'Slash', 'Blunt'], 'BepInX escaped strings'),
        
        # Edge cases
        ('', [], 'Empty string'),
        ('   ', [], 'Whitespace only'),
        ('single', ['single'], 'Single value'),
        ('"single"', ['single'], 'Quoted single value'),
        ('  value1  ,  value2  ', ['value1', 'value2'], 'Values with spaces'),
        ('"  value1  ,  value2  "', ['value1', 'value2'], 'Quoted values with spaces'),
    ]
    
    print("Testing Enhanced ParseCommaSeparatedValues function:")
    print("=" * 60)
    
    all_passed = True
    for input_val, expected, description in test_cases:
        result = parse_comma_separated_values(input_val)
        passed = result == expected
        all_passed = all_passed and passed
        
        status = "PASS" if passed else "FAIL"
        print(f"{status} {description}")
        print(f"      Input: '{input_val}'")
        print(f"   Expected: {expected}")
        print(f"     Result: {result}")
        if not passed:
            print(f"   >>> MISMATCH! <<<")
        print()
    
    print("=" * 60)
    if all_passed:
        print("All tests PASSED!")
        print("\nRecommended usage:")
        print("  OK Use pipes (85|85|85) - No comment conflicts or LibreCalc issues")
        print("  WARN Use commas (85,85,85) - May auto-convert in LibreCalc")
        print("  AVOID Avoid quotes (\"85,85,85\") - Gets escaped by BepInX")
    else:
        print("❌ Some tests FAILED!")
    
    return all_passed

if __name__ == "__main__":
    test_parsing()
