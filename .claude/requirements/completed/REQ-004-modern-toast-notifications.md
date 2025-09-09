# REQ-004: Modern Toast Notification System - Phoenix Theme Aligned

## Overview
Transform the existing toast notification system into a modern, Phoenix Admin Template-aligned experience with smart auto-dismiss behavior and enhanced visual design.

## Current State Analysis
- **Sticky behavior**: All toasts never auto-dismiss (autohide: false)
- **Timer display**: Shows elapsed time counter after 15 seconds ("hace Xs", "hace Xmin")
- **Visual design**: Basic Bootstrap color schemes (text-bg-danger, text-bg-success)
- **Position**: Fixed top-right corner with z-index 1080
- **Icons**: Feather icons for type indication
- **No backward compatibility required**: Breaking changes are acceptable

## Problems to Solve
1. **Cognitive overload**: Sticky toasts accumulate and clutter the interface
2. **Timer confusion**: Elapsed time counter provides little value and adds visual noise
3. **Visual inconsistency**: Not aligned with Phoenix Admin Template design system
4. **Poor UX patterns**: All toasts behave the same regardless of importance
5. **Outdated aesthetics**: Missing modern design trends like glassmorphism

## Proposed Solution

### 1. Phoenix Theme Integration
```css
/* Use Phoenix CSS variables for consistency */
--phoenix-primary: #3874ff;
--phoenix-success: #25b003;
--phoenix-warning: #e5780b;
--phoenix-danger: #fa3b1d;
--phoenix-info: #0097eb;
--phoenix-box-shadow: 0px 2px 4px -2px rgba(36, 40, 46, 0.08);
--phoenix-border-radius: 0.375rem;
--phoenix-body-font-family: "Nunito Sans";
```

### 2. Smart Auto-Dismiss Behavior
```javascript
const durations = {
  success: 4000,    // Quick acknowledgment
  info: 5000,       // Informational messages
  warning: 8000,    // Needs attention
  danger: 0         // Errors remain sticky (critical)
};
```

### 3. Modern Visual Design
- **Glassmorphism**: Backdrop blur with semi-transparent backgrounds
- **Phoenix shadows**: Use --phoenix-box-shadow for consistency
- **Smooth animations**: Slide-in from top-right with fade
- **Progress bar**: Visual countdown for auto-dismiss
- **Compact design**: Remove unnecessary padding and borders
- **Dark mode support**: Full compatibility with Phoenix dark theme

### 4. Enhanced Interactions
- **Hover to pause**: Stop auto-dismiss countdown on hover
- **Click to dismiss**: Immediate dismissal on click
- **Stack management**: Vertical stacking with proper spacing
- **Message grouping**: Combine similar messages with counter badge
- **Remove elapsed timer**: No more confusing time counter

## Technical Implementation

### Files to Modify
1. `Web/wwwroot/assets/js/site.js` - Complete rewrite of showToast function
2. `Web/wwwroot/assets/css/linasys.css` - Add modern toast styles
3. Update all existing toast calls throughout the application

### New showToast Function
```javascript
function showToast(message, type = 'info', header = null) {
  // Auto-dismiss durations
  const durations = {
    success: 4000,
    info: 5000,
    warning: 8000,
    danger: 0  // Sticky
  };
  
  // Phoenix-aligned colors and icons
  const config = {
    info: { icon: 'info-circle', header: 'Información' },
    success: { icon: 'check-circle', header: 'Éxito' },
    warning: { icon: 'alert-triangle', header: 'Atención' },
    danger: { icon: 'x-octagon', header: 'Error' }
  };
  
  // Create toast with Phoenix design system
  // Add progress bar for auto-dismiss
  // Implement hover pause functionality
}
```

### CSS Implementation
```css
/* Phoenix-aligned toast styles */
.toast {
  backdrop-filter: blur(10px);
  background: rgba(var(--phoenix-body-bg-rgb), 0.95);
  border: 1px solid rgba(var(--phoenix-border-color-rgb), 0.1);
  box-shadow: var(--phoenix-box-shadow-lg);
  border-radius: var(--phoenix-border-radius);
}

.toast-progress {
  position: absolute;
  bottom: 0;
  left: 0;
  height: 3px;
  background: var(--phoenix-primary);
  border-radius: 0 0 var(--phoenix-border-radius) var(--phoenix-border-radius);
  animation: progress-countdown linear;
}

/* Type-specific colors */
.toast-success { 
  border-left: 3px solid var(--phoenix-success);
}
.toast-danger { 
  border-left: 3px solid var(--phoenix-danger);
}
.toast-warning { 
  border-left: 3px solid var(--phoenix-warning);
}
.toast-info { 
  border-left: 3px solid var(--phoenix-info);
}

/* Animations */
@keyframes slide-in {
  from {
    transform: translateX(100%);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}
```

## Acceptance Criteria
1. ✅ Success/info toasts auto-dismiss with progress bar
2. ✅ Error toasts remain sticky until manually dismissed
3. ✅ Visual design matches Phoenix Admin Template
4. ✅ Smooth animations and transitions
5. ✅ Hover pauses auto-dismiss countdown
6. ✅ Dark mode compatibility
7. ✅ Mobile responsive
8. ✅ No elapsed time counter
9. ✅ All existing toast calls updated (no backward compatibility)

## Implementation Steps
1. Update requirements document with Phoenix alignment
2. Rewrite showToast function in site.js
3. Add Phoenix-aligned styles to linasys.css
4. Remove elapsed time counter logic
5. Implement progress bar with pause on hover
6. Test with all toast types
7. Update all existing showToast calls in the application

## Breaking Changes
- Function signature changed: `showToast(message, type, header)` 
- Removed parameters: `icon`, `delay`
- Auto-dismiss behavior now type-dependent
- No more elapsed time display
- Visual design completely changed

## Dependencies
- Bootstrap 5 Toast component (existing)
- Feather icons library (existing)
- Phoenix Admin Template CSS variables (existing)
- No additional libraries required

## Estimated Effort
- Development: 3-4 hours
- Testing: 1 hour
- Updating existing calls: 1 hour
- Total: 5-6 hours