#!/usr/bin/env python3
"""
Test script to verify that the C# ParseCommaSeparatedValues function works correctly.
This simulates the parsing logic to validate our approach.
"""

def parse_comma_separated_values(input_str):
    """Python version of the C# ParseCommaSeparatedValues function for testing"""
    if not input_str:
        return []
    
    # Trim the input
    input_str = input_str.strip()
    
    # Check if trimmed input is empty
    if not input_str:
        return []
    
    # Check if the entire string is quoted
    if input_str.startswith('"') and input_str.endswith('"') and len(input_str) >= 2:
        # Remove outer quotes and split
        unquoted = input_str[1:-1]
        return [s.strip() for s in unquoted.split(',')]
    else:
        # Normal comma-separated values
        return [s.strip() for s in input_str.split(',')]

def test_parsing():
    """Test various input formats"""
    test_cases = [
        # (input, expected_output, description)
        ('85,85,85', ['85', '85', '85'], 'Normal comma-separated values'),
        ('"85,85,85"', ['85', '85', '85'], 'Quoted comma-separated values'),
        ('Pierce,Slash,Blunt', ['Pierce', 'Slash', 'Blunt'], 'Normal string values'),
        ('"Pierce,Slash,Blunt"', ['Pierce', 'Slash', 'Blunt'], 'Quoted string values'),
        ('150,150,150,150,150', ['150', '150', '150', '150', '150'], 'Multiple values'),
        ('"150,150,150,150,150"', ['150', '150', '150', '150', '150'], 'Quoted multiple values'),
        ('', [], 'Empty string'),
        ('   ', [], 'Whitespace only'),
        ('single', ['single'], 'Single value'),
        ('"single"', ['single'], 'Quoted single value'),
        ('  value1  ,  value2  ', ['value1', 'value2'], 'Values with spaces'),
        ('"  value1  ,  value2  "', ['value1', 'value2'], 'Quoted values with spaces'),
    ]
    
    print("Testing ParseCommaSeparatedValues function:")
    print("=" * 50)
    
    all_passed = True
    for input_val, expected, description in test_cases:
        result = parse_comma_separated_values(input_val)
        passed = result == expected
        all_passed = all_passed and passed
        
        status = "✓ PASS" if passed else "✗ FAIL"
        print(f"{status} {description}")
        print(f"      Input: '{input_val}'")
        print(f"   Expected: {expected}")
        print(f"     Result: {result}")
        if not passed:
            print(f"   >>> MISMATCH! <<<")
        print()
    
    print("=" * 50)
    if all_passed:
        print("🎉 All tests PASSED!")
    else:
        print("❌ Some tests FAILED!")
    
    return all_passed

if __name__ == "__main__":
    test_parsing()
