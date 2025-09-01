# CFG Converter Scripts - Updated for Semicolon Support

## Overview
The converter scripts in the `cfg_converters/` directory now support the enhanced semicolon delimiter format that solves both LibreCalc and BepInX compatibility issues.

## Updated Scripts

### 1. `test_parser.py` - Enhanced Parser Testing
- **Purpose**: Validates the parsing logic for all supported formats
- **Tests**: 19 comprehensive test cases covering semicolons, commas, quotes, and edge cases
- **Usage**: `(cd cfg_converters && python test_parser.py)`
- **Features**:
  - Semicolon delimiter support (recommended)
  - BepInX escaped quote handling (`\"value\"`)
  - Regular quoted strings (`"value"`)
  - Comma-separated values (legacy)
  - Edge case handling (empty, whitespace, trailing delimiters)

### 2. `cfg_format_converter.py` - NEW Format Converter
- **Purpose**: Converts between different delimiter formats in config files
- **Usage**: `python cfg_format_converter.py <config_file> [format] [--no-backup]`
- **Formats**: 
  - `semicolon` (default, recommended)
  - `comma` (legacy compatibility)
  - `quoted` (quoted comma format)
- **Features**:
  - Automatic backup creation
  - Smart detection of multi-value configuration keys
  - Preserves comments and formatting
  - Handles BepInX escaped quotes

### 3. `cfg_to_tsv.py` - TSV Export (Compatible)
- **Status**: Works with all formats (no changes needed)
- **Purpose**: Exports config to TSV files for LibreCalc editing
- **Compatibility**: Reads semicolon, comma, and quoted formats seamlessly

### 4. `tsv_to_cfg.py` - TSV Import (Compatible)
- **Status**: Works with all formats (no changes needed)  
- **Purpose**: Updates config files from edited TSV files
- **Compatibility**: Preserves existing format or can be combined with converter

## Recommended Workflow

### For New Configurations
1. Use semicolon format directly: `85;85;85`
2. No additional conversion needed

### For Existing Configurations
1. **Convert to semicolons** (recommended):
   ```bash
   (cd cfg_converters && python cfg_format_converter.py Tidalwave.ForsakenPowerOverhaul.cfg semicolon)
   ```

2. **Export to TSV** for LibreCalc editing:
   ```bash
   (cd cfg_converters && python cfg_to_tsv.py)
   ```

3. **Edit in LibreCalc** using semicolons (no auto-conversion issues)

4. **Import back from TSV**:
   ```bash
   (cd cfg_converters && python tsv_to_cfg.py)
   ```

### Command Examples

```bash
# Test the enhanced parser
(cd cfg_converters && python test_parser.py)

# Convert config to semicolon format (recommended)
(cd cfg_converters && python cfg_format_converter.py Tidalwave.ForsakenPowerOverhaul.cfg semicolon)

# Convert back to comma format if needed
(cd cfg_converters && python cfg_format_converter.py Tidalwave.ForsakenPowerOverhaul.cfg comma)

# Export to TSV for LibreCalc (works with any format)
(cd cfg_converters && python cfg_to_tsv.py)

# Import from TSV (works with any format)
(cd cfg_converters && python tsv_to_cfg.py)
```

## Benefits

### ✅ **Semicolon Format Advantages**
- No LibreCalc auto-conversion to formulas
- No BepInX quote escaping
- Clean, readable format
- Full backward compatibility

### 🔄 **Migration Path**
- Existing comma/quoted configs continue to work
- Easy conversion with `cfg_format_converter.py`
- No breaking changes to workflow

### 🛠️ **Tool Compatibility**
- All existing TSV tools work unchanged
- Enhanced testing validates all formats
- Smart conversion preserves comments/formatting

## File Support

The converter automatically detects and converts these configuration keys:
- `IncomingDamageMultiplierModifiers`
- `OutgoingDamageMultiplierModifiers`
- `IncomingDamageMultiplierTypes`
- `OutgoingDamageMultiplierTypes`
- `IncomingDamageTypes`
- `OutgoingDamageTypes`
- `IncomingDamageModifiers`
- `OutgoingDamageModifiers`
- `StatusAttributes`

## Subshell Usage Tip
As demonstrated, use parentheses for temporary directory changes:
```bash
(cd cfg_converters && python script.py)  # Directory change only affects this command
pwd  # Still in original directory
```
