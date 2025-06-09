<div align="center">
    <img src="icon.ico" width="128px" style="margin: 0; padding: 0;" alt="ResourceMerger Icon">
    <h1 style="margin: 0; padding: 0;">ResourceMerger</h1>

**ResourceMerger** is a lightweight .NET CLI utility that merges **embedded resources** from one .NET assembly (DLL or EXE) into another. This is useful for consolidating resources, reducing dependencies, or preparing assemblies for distribution or obfuscation.

---

## 🔧 Features

- ✅ Merge all `EmbeddedResource` entries from a source assembly into a target assembly
- ✅ Automatically remove `AssemblyLinkedResource` entries referencing the source
- ✅ Supports both `.dll` and `.exe` files
- ✅ Preserves metadata and old max stack to avoid CIL write errors

---

## 📦 Usage

### 🪟 Windows
```bash
ResourceMerger.exe <TargetAssembly> <SourceAssembly>
```

### Other Platforms (.NET Supported)
```bash
dotnet ResourceMerger.dll <TargetAssembly> <SourceAssembly>
```
</div>