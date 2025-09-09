# Documentation System Guide

## 📚 Three-Tier Documentation Structure

This project uses a strategic three-tier documentation system designed to optimize AI assistant comprehension and productivity:

### 1️⃣ [../CLAUDE.md](../CLAUDE.md) (Root Directory)
**Purpose**: Persistent project memory  
**Size**: ~100 lines  
**Contains**:
- Core project configuration
- Architecture overview
- Quick reference patterns
- Links to detailed docs

**When to read**: Start of any new conversation

### 2️⃣ [CURRENT_SESSION.md](CURRENT_SESSION.md) 
**Purpose**: Active working context  
**Size**: ~50 lines  
**Contains**:
- Current task details
- Today's progress
- Immediate next steps
- Recent issues/blockers

**When to read**: Beginning of each work session  
**When to update**: After significant progress or context changes

### 3️⃣ [WORK_LOG.md](WORK_LOG.md)
**Purpose**: Complete historical record  
**Size**: Unlimited  
**Contains**:
- Detailed session-by-session progress
- All implementation steps
- Problems and solutions
- Feature completion history

**When to read**: Only when historical context is needed

## 🎯 Usage Strategy

1. **Start here**: Read [CURRENT_SESSION.md](CURRENT_SESSION.md) for immediate context
2. **Reference**: Check [../CLAUDE.md](../CLAUDE.md) for patterns and project info
3. **Deep dive**: Consult [WORK_LOG.md](WORK_LOG.md) only for historical details

## 📝 Maintenance

- **[CURRENT_SESSION.md](CURRENT_SESSION.md)**: Update at start/end of each work session
- **[../CLAUDE.md](../CLAUDE.md)**: Update only for major architecture changes
- **[WORK_LOG.md](WORK_LOG.md)**: Append completed work details

This structure prevents information overload while maintaining complete project history.