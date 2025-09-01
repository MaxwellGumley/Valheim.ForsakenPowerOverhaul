#!/usr/bin/env python3

# Test the updated ParseCommaSeparatedValues function logic
def parse_comma_separated_values(input_str):
    """Python equivalent of the C# ParseCommaSeparatedValues function"""
    if not input_str:
        return []
    
    # Trim the input
    input_str = input_str.strip()
    
    # Check if trimmed input is empty
    if not input_str:
        return []
    
    # Handle escaped quotes from BepInEx config system
    if input_str.startswith('\\"') and input_str.endswith('\\"') and len(input_str) >= 4:
        # Remove escaped quotes and split by comma
        unquoted = input_str[2:-2]
        return [s.strip() for s in unquoted.split(',')]
    # Check if the entire string is quoted (regular quotes)
    elif input_str.startswith('"') and input_str.endswith('"') and len(input_str) >= 2:
        # Remove outer quotes and split by comma
        unquoted = input_str[1:-1]
        return [s.strip() for s in unquoted.split(',')]
    # Check if using semicolon delimiter (LibreCalc-friendly)
    elif ';' in input_str:
        # Split by semicolon
        return [s.strip() for s in input_str.split(';')]
    else:
        # Normal comma-separated values
        return [s.strip() for s in input_str.split(',')]

# Test cases
test_cases = [
    # Original formats
    ('70,70,70', ['70', '70', '70']),
    ('"70,70,70"', ['70', '70', '70']),
    
    # Escaped quotes from BepInX
    ('\\"70,70,70\\"', ['70', '70', '70']),
    ('\\"85,85,85,85,85\\"', ['85', '85', '85', '85', '85']),
    
    # Semicolon format (LibreCalc-friendly)
    ('70;70;70', ['70', '70', '70']),
    ('85;85;85;85;85', ['85', '85', '85', '85', '85']),
    ('100; 90; 80', ['100', '90', '80']),
    
    # Edge cases
    ('', []),
    ('   ', []),
    ('100', ['100']),
    ('"100"', ['100']),
    ('\\"100\\"', ['100']),
    ('100;', ['100', '']),
    
    # With spaces
    ('70, 70, 70', ['70', '70', '70']),
    ('"70, 70, 70"', ['70', '70', '70']),
    ('\\"70, 70, 70\\"', ['70', '70', '70']),
]

print("Testing ParseCommaSeparatedValues function with semicolon support:")
print("=" * 65)

all_passed = True
for i, (input_str, expected) in enumerate(test_cases, 1):
    result = parse_comma_separated_values(input_str)
    passed = result == expected
    if not passed:
        all_passed = False
    
    status = "✓ PASS" if passed else "✗ FAIL"
    print(f"Test {i:2d}: {status}")
    print(f"  Input:    {repr(input_str)}")
    print(f"  Expected: {expected}")
    print(f"  Got:      {result}")
    print()

print("=" * 65)
if all_passed:
    print("🎉 All tests passed! Semicolon support added for LibreCalc compatibility.")
else:
    print("❌ Some tests failed.")

print("\n📋 Recommended formats for LibreCalc compatibility:")
print("   - Best:     85;85;85;85;85  (semicolons)")
print("   - Works:    85,85,85,85,85  (commas)")
print("   - Avoid:    \"85,85,85,85,85\"  (quotes get escaped by BepInX)")
