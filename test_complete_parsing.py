#!/usr/bin/env python3
"""
Complete parsing test to verify all supported formats work correctly.
This tests the three recommended formats for configuration values.
"""

def parse_comma_separated_values(input_string):
    """
    Python equivalent of the C# ParseCommaSeparatedValues function.
    Tests semicolon delimiters (recommended), comma delimiters, and quoted strings.
    """
    if not input_string or input_string.strip() == "":
        return []
    
    # Handle semicolon delimiters (recommended for LibreCalc compatibility)
    if ';' in input_string:
        return [item.strip() for item in input_string.split(';') if item.strip()]
    
    # Handle escaped quotes (what BepInX creates when saving quoted strings)
    if '\\"' in input_string:
        # Remove outer quotes if present and unescape inner quotes
        cleaned = input_string.strip()
        if cleaned.startswith('\\"') and cleaned.endswith('\\"'):
            cleaned = cleaned[2:-2]  # Remove outer escaped quotes
        # Replace escaped quotes with regular quotes, then split by comma
        cleaned = cleaned.replace('\\"', '"')
        return [item.strip().strip('"') for item in cleaned.split(',') if item.strip()]
    
    # Handle regular quoted strings
    if '"' in input_string:
        # Remove outer quotes if present
        cleaned = input_string.strip()
        if cleaned.startswith('"') and cleaned.endswith('"'):
            cleaned = cleaned[1:-1]
        # Split by comma and clean up
        return [item.strip().strip('"') for item in cleaned.split(',') if item.strip()]
    
    # Handle simple comma-separated values
    return [item.strip() for item in input_string.split(',') if item.strip()]

def test_parsing():
    """Test all supported parsing formats."""
    test_cases = [
        # Semicolon format (recommended for LibreCalc)
        ("85;85;85;85;85", ["85", "85", "85", "85", "85"], "Semicolon format"),
        ("Fire;Frost;Lightning", ["Fire", "Frost", "Lightning"], "Semicolon text"),
        (" Fire ; Frost ; Lightning ", ["Fire", "Frost", "Lightning"], "Semicolon with spaces"),
        
        # Regular comma format
        ("85,85,85,85,85", ["85", "85", "85", "85", "85"], "Comma format"),
        ("Fire,Frost,Lightning", ["Fire", "Frost", "Lightning"], "Comma text"),
        (" Fire , Frost , Lightning ", ["Fire", "Frost", "Lightning"], "Comma with spaces"),
        
        # Quoted format (what user types)
        ('"85,85,85,85,85"', ["85", "85", "85", "85", "85"], "Quoted comma format"),
        ('"Fire,Frost,Lightning"', ["Fire", "Frost", "Lightning"], "Quoted comma text"),
        
        # Escaped quoted format (what BepInX saves)
        ('\\"85,85,85,85,85\\"', ["85", "85", "85", "85", "85"], "BepInX escaped format"),
        ('\\"Fire,Frost,Lightning\\"', ["Fire", "Frost", "Lightning"], "BepInX escaped text"),
        
        # Edge cases
        ("", [], "Empty string"),
        ("   ", [], "Whitespace only"),
        ("85", ["85"], "Single value"),
        ("Fire", ["Fire"], "Single text value"),
        ("85;", ["85"], "Trailing semicolon"),
        ("85,", ["85"], "Trailing comma"),
        (";85", ["85"], "Leading semicolon"),
        (",85", ["85"], "Leading comma"),
    ]
    
    passed = 0
    failed = 0
    
    print("Testing ParseCommaSeparatedValues function with all formats:")
    print("=" * 80)
    
    for i, (input_val, expected, description) in enumerate(test_cases, 1):
        try:
            result = parse_comma_separated_values(input_val)
            if result == expected:
                print(f"✓ Test {i:2d}: {description}")
                print(f"         Input: {repr(input_val)}")
                print(f"        Result: {result}")
                passed += 1
            else:
                print(f"✗ Test {i:2d}: {description}")
                print(f"         Input: {repr(input_val)}")
                print(f"      Expected: {expected}")
                print(f"           Got: {result}")
                failed += 1
        except Exception as e:
            print(f"✗ Test {i:2d}: {description} - EXCEPTION: {e}")
            print(f"         Input: {repr(input_val)}")
            failed += 1
        print()
    
    print("=" * 80)
    print(f"Results: {passed} passed, {failed} failed")
    
    if failed == 0:
        print("\n🎉 All tests passed! The parsing function supports all three formats:")
        print("   1. Semicolons (85;85;85) - RECOMMENDED for LibreCalc compatibility")
        print("   2. Commas (85,85,85) - Works but may auto-convert in LibreCalc")
        print("   3. Quoted strings (\"85,85,85\") - Gets escaped by BepInX but still works")
        print("\nFor best results, use semicolons to avoid both LibreCalc auto-conversion")
        print("and BepInX quote escaping issues.")
    else:
        print(f"\n❌ {failed} test(s) failed!")
    
    return failed == 0

if __name__ == "__main__":
    success = test_parsing()
    exit(0 if success else 1)
