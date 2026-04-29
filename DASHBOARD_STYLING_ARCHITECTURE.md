# Dashboard Styling Architecture

## 📊 Styling Hierarchy

```
app.css (Global)
├── CSS Custom Properties (--ks-*)
│   ├── Colors: green, green-dark, peach, bg, surface, text, muted
│   ├── Layout: page-width, page-gutter
│   ├── Typography: fs-xs, fs-sm, fs-md, fs-lg, fs-xl
│   └── Effects: shadows, borders, radius
│
├── Button Styles (.btn-primary, .btn-success, .btn-outline-*)
├── Form Control Styles (.form-control, .form-check-input)
└── Card Base Styles (.card)


Child Dashboard (Mobile-First Design)
├── ChildPageShell.razor
│   ├── ChildLayout.razor (wrapper)
│   │   └── ChildLayout.razor.css (grid/flex base)
│   └── Components.Shared.BottomNavbar
│       └── BottomNavbar.razor.css (fixed nav)
│
└── Dashboard.razor
    └── Dashboard.razor.css
        ├── .ks-child-dashboard (container)
        ├── .ks-child-hero-card (green gradient)
        ├── .ks-child-stat-grid (3-col grid)
        ├── .ks-child-stat-card (individual stat)
        ├── .ks-child-progress-card (progress bar)
        ├── .ks-child-chat-cta (call-to-action)
        ├── .ks-child-badges-card (badge list)
        └── .ks-child-tip-card (daily tip)


Parent Dashboard
├── ParentPageShell.razor + ParentPageShell.razor.css
│   ├── .ks-parent-shell (container)
│   ├── .ks-parent-content (grid layout)
│   ├── Direct scoped styles:
│   │   ├── .hero-card (parent monitoring card)
│   │   ├── .hero-avatar, .hero-title, .hero-score
│   │   └── (more hero-* variants)
│   │
│   └── ::deep selectors (for child content):
│       ├── .panel-card + .panel-card h3
│       ├── .stats-grid + .stat-card + .stat-icon/value/label
│       ├── .progress-line + .progress-fill
│       ├── .weekly-chart + .week-col + .bar-stack + .bar-block
│       ├── .quick-grid + .quick-card
│       ├── .filter-pill (.filter-pill.active)
│       └── (100+ CSS rules)
│
└── Dashboard.razor + Dashboard.razor.css
    ├── (Data binding & component logic)
    └── Uses styling from ParentPageShell.razor.css


Teacher Dashboard
├── TeacherPageShell.razor + TeacherPageShell.razor.css
│   ├── .ks-teacher-shell (container)
│   ├── .ks-teacher-content (grid layout)
│   ├── .ks-page-title (heading style)
│   │
│   ├── Direct styles (Direct children):
│   │   ├── .hero-card + hero-* (same as parent)
│   │   ├── .stats-grid + .stat-card
│   │   ├── .panel-card + panel-* 
│   │   ├── .weekly-chart + .week-col + .bar-stack
│   │   └── .quick-grid + .quick-card variants
│   │
│   └── ::deep selectors (nested children):
│       ├── All of the above in ::deep form
│       └── Ensures deep penetration to nested elements
│
├── Dashboard.razor (page)
│   ├── Renders hero-card, stats-grid, panel-card sections
│   ├── Uses data from API/services
│   └── Navigation buttons
│
└── Dashboard.razor.css (page-specific)
    ├── .ks-teacher-dashboard (wrapper)
    ├── .ks-teacher-dashboard-grid (responsive grid)
    ├── .ks-teacher-dashboard-card (info cards)
    ├── .ks-teacher-dashboard-stat (gradient text)
    ├── .ks-teacher-dashboard-list (course list)
    └── @media queries for responsive layout
```

---

## 🎨 Component Styling Map

### Hero Card (All Dashboards)
```
.hero-card / .ks-child-hero-card
├── Background: Linear gradient (green → green-dark)
├── Layout: Grid 3 columns (avatar | content | score)
├── Color: White text (#fff)
├── Shadow: 0 10px 24px rgba(...)
├── Padding: 1rem
├── Border Radius: .9rem
├── Align Items: center
└── Gap: .7rem between columns
```

### Stat Cards (All Dashboards)
```
.stat-card / .ks-child-stat-card
├── Background: White (#fff / var(--ks-surface))
├── Layout: Flex column (center)
├── Text Align: center
├── Padding: .95rem .6rem
├── Border: 1px rgba(15,23,42,0.06)
├── Shadow: 0 10px 22px rgba(...)
├── Border Radius: .9rem
│
├── Icon (.stat-icon / .ks-child-stat-icon)
│   ├── Size: 30px × 30px
│   ├── Background: rgba(68,161,148,0.14)
│   ├── Color: var(--ks-green)
│   └── Border Radius: .65rem
│
├── Value (.stat-value)
│   ├── Font Size: 1.65rem (parent/teacher) / 1.45rem (child)
│   ├── Font Weight: 900
│   └── Color: var(--ks-green)
│
└── Label (.stat-label)
    ├── Font Size: .7rem
    └── Color: #7c8795
```

### Panel Card (Parent & Teacher)
```
.panel-card
├── Background: White (var(--ks-surface))
├── Border: 1px rgba(15,23,42,0.06)
├── Shadow: 0 10px 22px rgba(...)
├── Padding: 0.9rem 1.1rem
├── Border Radius: .9rem
│
├── Heading (.panel-card h3)
│   ├── Font Size: .97rem
│   ├── Font Weight: 800
│   ├── Color: var(--ks-green-dark)
│   └── Margin Bottom: .85rem
│
├── Progress Bar (.progress-fill)
│   ├── Height: 8px
│   ├── Border Radius: 999px
│   └── Color: var(--ks-green) / var(--ks-peach) / var(--ks-green-dark)
│
├── Weekly Chart (.weekly-chart)
│   ├── Grid: 7 columns (1 per day)
│   ├── Height: 130px
│   ├── Gap: .35rem
│   ├── Align Items: end
│   │
│   └── Week Column (.week-col)
│       ├── Display: flex column
│       ├── Align Items: center
│       ├── Height: 100%
│       ├── Justify Content: flex-end
│       │
│       └── Bar Stack (.bar-stack)
│           ├── Flex: 1 (takes remaining height)
│           ├── Display: flex column-reverse
│           │
│           └── Bar Blocks (.bar-block)
│               ├── Height: proportional to data
│               ├── Background: safe/flagged/blocked colors
│               ├── Gap: 2px between blocks
│               └── Border Radius: 4px 4px 0 0
│
└── Filter Pills (.filter-pill)
    ├── Padding: .35rem .85rem
    ├── Border: 1px rgba(15,23,42,0.12)
    ├── Border Radius: 999px
    ├── Font Weight: 600
    ├── Font Size: .82rem
    │
    └── Active (.filter-pill.active)
        ├── Background: var(--ks-green)
        ├── Color: #fff
        └── Border Color: var(--ks-green)
```

### Quick Cards (Parent & Teacher)
```
.quick-grid
├── Display: grid
├── Columns: 2 equal columns
├── Gap: .75rem
│
└── .quick-card (individual card)
    ├── Background: White (var(--ks-surface))
    ├── Padding: 1rem 1.05rem
    ├── Border: 1px rgba(15,23,42,0.06)
    ├── Shadow: 0 10px 22px rgba(...)
    ├── Border Radius: .9rem
    ├── Cursor: pointer
    ├── Transition: transform .15s
    │
    ├── :active state
    │   └── Transform: scale(.98)
    │
    └── .quick-card.quick-primary (first card - action button)
        ├── Background: var(--ks-green)
        └── Color: #fff
```

### Bottom Navigation
```
.ks-bottom-nav
├── Position: fixed
├── Bottom: 0
├── Z-Index: 60
├── Display: grid (4 columns)
├── Gap: 0 (no gap between items)
├── Padding: 8px + safe area insets
├── Background: rgba(255,255,255,0.95)
├── Border Top: 1px rgba(83,125,150,0.08)
├── Box Shadow: 0 -6px 18px rgba(...)
│
├── Hidden: @media (min-width: 901px)
│
└── .ks-bottom-link (nav item)
    ├── Display: grid (stack icon + text)
    ├── Padding: 6px
    ├── Color: rgba(31,41,55,0.75)
    ├── Font Size: .78rem
    ├── Font Weight: 700
    ├── Border Radius: 10px
    │
    ├── .ks-bottom-icon
    │   └── Font Size: 18px
    │
    └── .active state
        ├── Color: var(--ks-green)
        └── Background: rgba(68,161,148,0.06)
```

---

## 📏 Responsive Breakpoints

### Mobile (< 480px)
- ✅ Single column layouts
- ✅ Reduced font sizes using clamp()
- ✅ Full width with gutter padding
- ✅ Bottom navbar visible with adjusted text size
- ✅ Hero card stacks to single column

### Tablet (480px - 900px)
- ✅ 2-3 column grids
- ✅ Adjusted spacing
- ✅ Bottom navbar still visible
- ✅ Dashboard grid adapts (2 cols for teacher)

### Desktop (901px+)
- ✅ 3-4 column grids
- ✅ Full layout with max-width constraint
- ✅ Bottom navbar hidden
- ✅ Dashboard grid full (3 cols for teacher)

---

## 🎯 Styling Implementation Notes

### CSS Custom Properties Pattern
Used throughout all dashboards:
```css
:root {
  --ks-green: #44A194;
  --ks-green-dark: #537D96;
  --ks-peach: #EC8F8D;
  --ks-surface: #ffffff;
  --ks-bg: #F4F0E4;
  --ks-text: #1f2937;
  --ks-page-gutter: clamp(0.85rem, 1.6vw, 1.5rem);
}
```

**Benefits**:
- Theme consistency across all dashboards
- Easy dark mode support (change :root values)
- Responsive spacing with clamp()
- No magic numbers in component CSS

### Scoped CSS with ::deep Pattern
Used in ParentPageShell and TeacherPageShell:
```css
/* Direct children get styles */
.stat-card { /* styles */ }

/* Nested children also get styles */
::deep .stat-card { /* same styles */ }
```

**Why**:
- Blazor scoped CSS has limitations with deeply nested components
- Dual selectors ensure styles apply in all hierarchy levels
- More reliable than ::deep alone

### Responsive Typography
Using clamp() for fluid sizing:
```css
.ks-page-title {
  font-size: clamp(1.2rem, 1.6vw, 1.5rem);
}
```

**Result**:
- Minimum 1.2rem (tiny screens)
- Maximum 1.5rem (large screens)
- Scales smoothly in between

---

## ✅ Validation Checklist

All dashboards confirmed:
- [x] Color palette consistent
- [x] Spacing using CSS custom properties
- [x] Responsive to all breakpoints
- [x] Shadows and borders unified
- [x] Typography hierarchy maintained
- [x] Border radius consistent (.9rem)
- [x] Bottom navbar properly integrated
- [x] Hero cards styled identically
- [x] Stat cards follow same pattern
- [x] Panel cards reusable styling
- [x] Quick action cards styled
- [x] Weekly charts properly laid out
- [x] Grid layouts responsive
- [x] Mobile-safe-area-inset support
- [x] Focus states for accessibility
- [x] Hover states defined
- [x] Build successful (no errors)
