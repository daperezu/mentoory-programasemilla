# REQ-009: Form Constants Deduplication - Shared JavaScript Module for Specialized Inputs

**Status**: Pending  
**Priority**: Medium  
**Created**: 2025-01-10  
**Target Sprint**: Next  

## 1. Executive Summary

The system currently has significant code duplication between `participant-form.js` and `form-review-dual.js`, with both files containing identical hardcoded lists for specialized form inputs (nationalities, ID types, gender, marital status). This requirement eliminates this duplication by creating a shared JavaScript constants module, improving maintainability and reducing bundle size. The solution uses a client-side-only approach with ES6 modules, requiring no backend changes while maintaining full compatibility with existing functionality.

## 2. Business Context

### Current State
- **Duplicated nationality list**: 194 countries defined identically in 2 files (~400 lines each)
- **Duplicated option mappings**: ID types, gender, marital status repeated
- **Maintenance burden**: Updates must be made in multiple places
- **Risk of inconsistency**: Easy to miss updating one location
- **Bundle size impact**: Same data loaded and parsed twice

### Desired State
- **Single source of truth**: All form constants in one shared module
- **DRY principle**: No duplication across JavaScript files
- **Easy maintenance**: Update options in one place
- **Reduced bundle size**: ~800 lines of code removed
- **Consistent behavior**: Same options everywhere guaranteed

## 3. Technical Analysis

### Files Affected
1. **Primary duplication sources**:
   - `/Web/wwwroot/js/businessincubators/participant-form.js` (lines ~1000-1400)
   - `/Web/wwwroot/js/coordination/form-review-dual.js` (lines 554-833)

2. **Duplicated constants**:
   - Nationality list: 194 countries with ISO codes and Spanish names
   - ID Types: 4 options (CC, CE, PA, TI)
   - Gender: 3 options (M, F, O)
   - Marital Status: 5 options (S, C, U, D, V)

### Current Implementation Pattern
Both files use the same pattern:
```javascript
// Hardcoded in each file
const nationalities = [
    { value: 'AR', text: 'Argentina' },
    { value: 'BO', text: 'Bolivia' },
    // ... 192 more entries
];
nationalities.sort((a, b) => a.text.localeCompare(b.text, 'es'));
```

## 4. Proposed Solution

### Architecture Decision
Create a **shared JavaScript module** using ES6 module pattern for clean imports and exports. No backend API needed - this is purely a client-side refactoring to eliminate duplication.

### Module Structure
```javascript
// /Web/wwwroot/js/shared/form-constants.js
export const FormConstants = {
    // Nationality list with ISO codes
    nationalities: [
        { value: 'AR', text: 'Argentina' },
        { value: 'BO', text: 'Bolivia' },
        // ... complete list
    ],
    
    // ID Type options
    idTypes: {
        'CC': 'Cédula de Ciudadanía',
        'CE': 'Cédula de Extranjería',
        'PA': 'Pasaporte',
        'TI': 'Tarjeta de Identidad'
    },
    
    // Gender options
    genders: {
        'M': 'Masculino',
        'F': 'Femenino',
        'O': 'Otro'
    },
    
    // Marital Status options
    maritalStatuses: {
        'S': 'Soltero(a)',
        'C': 'Casado(a)',
        'U': 'Unión Libre',
        'D': 'Divorciado(a)',
        'V': 'Viudo(a)'
    }
};

// Utility functions
export const FormUtils = {
    // Get sorted nationalities for dropdowns
    getSortedNationalities() {
        return [...FormConstants.nationalities].sort((a, b) => 
            a.text.localeCompare(b.text, 'es')
        );
    },
    
    // Get display text for a value
    getDisplayText(type, value) {
        switch(type) {
            case 'nationality':
                const nation = FormConstants.nationalities.find(n => n.value === value);
                return nation ? nation.text : value || 'Sin respuesta';
            case 'idType':
                return FormConstants.idTypes[value] || value || 'Sin respuesta';
            case 'gender':
                return FormConstants.genders[value] || value || 'Sin respuesta';
            case 'maritalStatus':
                return FormConstants.maritalStatuses[value] || value || 'Sin respuesta';
            default:
                return value || 'Sin respuesta';
        }
    },
    
    // Generate select options HTML
    renderSelectOptions(type, selectedValue = '') {
        let options = '<option value="">Seleccione...</option>';
        
        switch(type) {
            case 'nationality':
                const nations = this.getSortedNationalities();
                nations.forEach(n => {
                    const selected = n.value === selectedValue ? 'selected' : '';
                    options += `<option value="${n.value}" ${selected}>${n.text}</option>`;
                });
                break;
            case 'idType':
                Object.entries(FormConstants.idTypes).forEach(([value, text]) => {
                    const selected = value === selectedValue ? 'selected' : '';
                    options += `<option value="${value}" ${selected}>${text}</option>`;
                });
                break;
            case 'gender':
                Object.entries(FormConstants.genders).forEach(([value, text]) => {
                    const selected = value === selectedValue ? 'selected' : '';
                    options += `<option value="${value}" ${selected}>${text}</option>`;
                });
                break;
            case 'maritalStatus':
                Object.entries(FormConstants.maritalStatuses).forEach(([value, text]) => {
                    const selected = value === selectedValue ? 'selected' : '';
                    options += `<option value="${value}" ${selected}>${text}</option>`;
                });
                break;
        }
        
        return options;
    }
};
```

## 5. Implementation Changes

### Phase 1: Create Shared Module
Create `/Web/wwwroot/js/shared/form-constants.js` with all constants and utility functions.

### Phase 2: Update participant-form.js
```javascript
// Add at top of file (inside IIFE)
// Import is handled via script tag order in Razor view
const { FormConstants, FormUtils } = window.FormConstants || {};

// Update renderNationalitySelect method
renderNationalitySelect(question, response) {
    const value = response?.answer || '';
    return `
        <select class="form-control nationality-select" 
                id="q_${question.questionId}"
                data-question-id="${question.questionId}"
                data-answer-type="text"
                ${question.isRequired ? 'required' : ''}>
            ${FormUtils.renderSelectOptions('nationality', value)}
        </select>
    `;
}

// Similar updates for other specialized inputs
renderIdTypeSelect(question, response) {
    const value = response?.answer || '';
    return `
        <select class="form-control" 
                id="q_${question.questionId}"
                data-question-id="${question.questionId}"
                data-answer-type="text"
                ${question.isRequired ? 'required' : ''}>
            ${FormUtils.renderSelectOptions('idType', value)}
        </select>
    `;
}
```

### Phase 3: Update form-review-dual.js
```javascript
// Inside DualAnswerReviewManager
// Access shared constants
const { FormConstants, FormUtils } = window.FormConstants || {};

// Update renderDualAnswerLayout function
case 7: // IdType
    starterAnswerDisplay = FormUtils.getDisplayText('idType', starterAnswerValue);
    coordinatorInput = `
        <select class="form-control coordinator-answer-input" 
                data-question-id="${questionId}">
            ${FormUtils.renderSelectOptions('idType', coordinatorAnswers[questionId])}
        </select>
    `;
    break;

case 8: // Gender
    starterAnswerDisplay = FormUtils.getDisplayText('gender', starterAnswerValue);
    coordinatorInput = `
        <select class="form-control coordinator-answer-input" 
                data-question-id="${questionId}">
            ${FormUtils.renderSelectOptions('gender', coordinatorAnswers[questionId])}
        </select>
    `;
    break;

case 9: // MaritalStatus
    starterAnswerDisplay = FormUtils.getDisplayText('maritalStatus', starterAnswerValue);
    coordinatorInput = `
        <select class="form-control coordinator-answer-input" 
                data-question-id="${questionId}">
            ${FormUtils.renderSelectOptions('maritalStatus', coordinatorAnswers[questionId])}
        </select>
    `;
    break;

case 12: // Nationality
    starterAnswerDisplay = FormUtils.getDisplayText('nationality', starterAnswerValue);
    coordinatorInput = `
        <select class="form-control coordinator-answer-input nationality-select-coord" 
                data-question-id="${questionId}">
            ${FormUtils.renderSelectOptions('nationality', coordinatorAnswers[questionId])}
        </select>
    `;
    break;
```

### Phase 4: Update Razor Views
Include the shared module before other scripts:
```html
@section Scripts {
    <!-- Load shared constants first -->
    <script src="~/js/shared/form-constants.js"></script>
    
    <!-- Then load the form scripts that use them -->
    <script src="~/js/businessincubators/participant-form.js"></script>
}
```

## 6. Testing Strategy

### Unit Tests
- Verify all constants are accessible from shared module
- Test utility functions with various inputs
- Validate HTML generation for select options

### Integration Tests
- Participant form renders all specialized inputs correctly
- Coordinator review form shows correct display values
- Dropdown selections work as before
- Form submission includes correct values

### Regression Tests
- All existing form functionality unchanged
- Data submission format remains the same
- Validation still works correctly
- Auto-save preserves specialized input values

## 7. Acceptance Criteria

### Required Functionality
- [ ] Shared module contains all nationality data (194 countries)
- [ ] Shared module contains all specialized input mappings
- [ ] Participant form uses shared constants
- [ ] Coordinator review form uses shared constants
- [ ] No hardcoded lists remain in individual files
- [ ] Utility functions work for all input types
- [ ] Alphabetical sorting maintained for nationalities
- [ ] Display text lookup works correctly
- [ ] HTML generation produces valid select options

### Technical Requirements
- [ ] ES6 module pattern with clean exports
- [ ] No backend dependencies
- [ ] Browser caching utilized for shared module
- [ ] ~800 lines of duplicate code removed
- [ ] Clean build (0 errors, 0 warnings)
- [ ] No breaking changes to existing functionality

## 8. Benefits & Impact

### Quantifiable Benefits
- **Code reduction**: ~800 lines removed (50% reduction in these files)
- **Bundle size**: Smaller JavaScript payload (shared module cached)
- **Maintenance time**: Single update point vs. multiple files
- **Bug reduction**: Eliminate inconsistency bugs

### Qualitative Benefits
- **DRY principle**: Better code organization
- **Developer experience**: Easier to find and update options
- **Consistency**: Guaranteed same options everywhere
- **Extensibility**: Easy to add new specialized types

## 9. Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Script load order issues | High | Ensure form-constants.js loads first in all views |
| Browser caching problems | Low | Use versioning in script URL (?v=1.0) |
| Missing constants during refactor | Medium | Comprehensive testing of all form types |
| IE11 compatibility | Low | Use compatible ES5 syntax or transpile |

## 10. Implementation Timeline

### Day 1: Create Shared Module
- [ ] Create form-constants.js with all data
- [ ] Implement utility functions
- [ ] Test module independently

### Day 2: Refactor Existing Files
- [ ] Update participant-form.js
- [ ] Update form-review-dual.js
- [ ] Remove all duplicate code

### Day 3: Testing & Documentation
- [ ] Test all form scenarios
- [ ] Update Razor views
- [ ] Update CLAUDE.md
- [ ] Create usage examples

## 11. Future Enhancements

### Potential Extensions (Not in scope)
- Add more specialized types (e.g., currencies, languages)
- Implement lazy loading for large lists
- Add search/filter for nationality dropdown
- Create TypeScript definitions
- Add unit tests for the shared module

## 12. Documentation Updates

- Update `.claude/web-patterns.md` with shared constants pattern
- Add section about specialized input handling
- Document module loading order requirements
- Create examples of adding new constant types

## 13. Success Metrics

- Zero duplicate nationality lists in codebase
- All specialized inputs use shared module
- No regression bugs reported
- Faster page load due to caching
- Easier maintenance confirmed by team

---

**Note**: This requirement focuses on client-side deduplication without backend changes, maintaining full compatibility while significantly improving code maintainability. The solution follows ES6 module patterns and aligns with the project's existing JavaScript architecture.