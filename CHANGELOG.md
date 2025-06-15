# Changelog

## 🧰 v1.2.5 – Command Modularization, Visual Polish & Project Cleanup

* 🧾 **Chore:** Bump version and update `CHANGELOG.md`
* 🧱 **Refactor:** Migrate to [`Spectre.Console.Cli`](https://spectreconsole.net/cli) with modular command structure
* 📝 **Docs:** Enhance `README.md` with icon, cross*platform usage, and cleaner layout
* 🧼 **Chore:** Restructure project file
* 🖼 **Chore:** Add application icon

## 🐧 v1.2.0 – CLI Polish, Metadata Refinement & UX Enhancements

* 🧾 **Chore:** Update `CHANGELOG.md` with new commits
* 💡 **Refactor:** Extract banner and validation logic into separate methods
* 🧾 **Chore:** Bump version and update `CHANGELOG.md`
* 🏗 **Build:** Add metadata and CI optimizations to project file
* 🎯 **Fix:** Correctly determine `exeName` for usage and help output
* 🧹 **Chore:** Reformat `Program.cs` for consistency and readability
* ✨ **Feat:** Integrate [Spectre.Console](https://spectreconsole.net/) for modern terminal UI
* 📝 **Docs:** Rewrite `CHANGELOG.md` with detailed markdown structure and metadata
* 🏗 **Build:** Set assembly output name to `ResourceMerger`
* 📝 **Docs:** Update `README.md` with improved instructions and metadata
* 🐛 **Fix:** Rename project title to `ResourceMerger` and update package metadata
* ➕ **Add:** Initial commit of `FodyWeavers.xsd`
* 🔥 **Chore:** Remove unused `FodyWeavers.xsd`
* 🧾 **Chore:** Ignore `*.DotSettings.user` in Git
* 🐛 **Fix:** Prevent max stack error with `KeepOldMaxStack` flag
* 🧹 **Chore:** Change explicit types to `var` where applicable
* ✨ **Feat:** Dynamically display executable name in usage/help message
* 🐛 **Fix:** Suppress Costura build warning
* 🔥 **Chore:** Delete legacy `FodyWeavers.xsd`
* 🧾 **Chore:** Update `.gitignore` with modern IDE/user*specific exclusions

## 📦 v1.1.0 – Enhancements & Tooling Integration

* ➕ Added [Fody](https://github.com/Fody/Fody) and [Costura](https://github.com/Fody/Costura) for IL weaving and resource embedding
* 🐞 Enabled full PDB generation for `Debug` configuration
* 🎨 Reformatted `Program.cs` for improved readability
* 🧼 Reformatted `FodyWeavers.xsd` to match XML formatting standards
* 🔧 Updated `.gitignore` with relevant exclusions
* 🏷 Added version prefixing support to the project configuration

***

## 🚀 v1.0.0 – Initial Release

* ✅ Implemented basic functionality for merging embedded resources between assemblies