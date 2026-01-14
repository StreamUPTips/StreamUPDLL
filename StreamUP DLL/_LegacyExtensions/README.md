# Legacy Extensions

This folder contains deprecated extension methods that are kept for **backwards compatibility only**.

## Do Not Use for New Development

All methods in this folder are marked with:
- `[Obsolete]` - Generates compiler warnings
- `[EditorBrowsable(Never)]` - Hidden from IntelliSense

## Migration Guide

When moving an extension to this folder:

1. Move the file(s) here
2. Add both attributes to each public method/class:

```csharp
using System.ComponentModel;

[Obsolete("Use SUP.NewMethodName() instead")]
[EditorBrowsable(EditorBrowsableState.Never)]
public static bool OldMethodName(this IInlineInvokeProxy CPH, ...)
{
    // Keep implementation for backwards compatibility
}
```

3. The csproj automatically includes `_LegacyExtensions\**\*.cs`

## Folder Structure

Organize by original location:
```
_LegacyExtensions/
├── ProductExtensions/      ← Old product methods
├── SettingsExtensions/     ← Old settings methods
├── UIExtensions/           ← Old UI methods
└── ...
```
