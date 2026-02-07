# 📚 HeimdallWeb Documentation

This directory contains all organized documentation for the HeimdallWeb project migration and implementation.

## 📁 Directory Structure

### 🏗️ `/migration/` - Migration Planning & Progress
Documentation related to the migration from legacy MVC to DDD Light + Minimal APIs + Next.js:
- **MIGRATION_STRUCTURE.md** - New project structure overview
- **PHASE2_COMPLETED.md** - Infrastructure layer completion status
- **PHASE3_APPLICATION_IMPLEMENTATION_PLAN.md** - Application layer implementation plan
- **PHASE3_APPLICATION_STATUS.md** - Current status of Phase 3
- **PHASE3_NEXT_STEPS.md** - Upcoming tasks for Phase 3
- **PHASE3_PROGRESS_UPDATE.md** - Detailed progress updates

### 🏛️ `/architecture/` - Architecture & Design Decisions
Architectural documentation and refactoring records:
- **PROGRAM_CS_ARCHITECTURE.md** - Program.cs architecture overview
- **PROGRAM_CS_REFACTORING.md** - Program.cs refactoring details
- **CHANGELOG_PROGRAM_CS_REFACTOR.md** - Changelog for Program.cs refactoring

### 📊 `/phases/` - Phase Implementation Summaries
Completion summaries for each major implementation phase:
- **Phase1_Domain_Implementation_Summary.md** - Domain layer summary
- **Phase3_ScanQueryHandlers_Summary.md** - Scan query handlers summary
- **PHASE4_WEBAPI_SUMMARY.md** - WebAPI layer summary

### 📖 `/guides/` - Usage Guides & Examples
Practical guides and usage examples:
- **Domain_Usage_Examples.md** - Domain layer usage examples and patterns

### 🧪 `/testing/` - Testing Documentation
Testing guides for each layer:
- **Phase1_Domain_TestGuide.md** - Domain layer testing guide
- **Phase2_Infrastructure_TestGuide.md** - Infrastructure layer testing guide
- **Phase3_ApplicationLayer_TestGuide.md** - Application layer testing guide

---

## 🎯 Quick Navigation

### Looking for migration plan?
→ See **[plano_migracao.md](../plano_migracao.md)** in the root directory (source of truth for migration)

### Want to understand the architecture?
→ Start with `/architecture/PROGRAM_CS_ARCHITECTURE.md`

### Need to check implementation status?
→ Check `/migration/` for phase-specific status files

### Looking for code examples?
→ See `/guides/Domain_Usage_Examples.md`

### Setting up tests?
→ Navigate to `/testing/` for layer-specific test guides

---

## 📝 Note on AI Agent Files

The following files remain in the root directory and should **NOT** be moved:
- `CLAUDE.md` - Claude AI agent instructions
- `.GEMINI.md` - Gemini AI agent instructions
- `.github/copilot-instructions.md` - GitHub Copilot instructions
- `plano_migracao.md` - Master migration plan (referenced by AI agents)
- `README.md` - Main project README

Moving these files would break AI agent workflows and tooling.

---

**Last Updated:** 2026-02-07  
**Structure Version:** 1.0
