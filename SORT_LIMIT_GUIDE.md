# 🎯 Sort & Limit Features - Complete Guide

## ✅ What's Been Added

**ORDER BY (Sorting)** and **LIMIT** functionality are now integrated into the Report Builder!

---

## 🚀 Quick Start

### Step 1: Run the Apps
```bash
# Terminal 1 - API
cd ReportBuilder.Api
dotnet run

# Terminal 2 - MVC
cd ReportBuilder.Web.Mvc
dotnet run
```

### Step 2: Open Report Builder
```
http://localhost:5200/ReportBuilder
```

### Step 3: Build a Complete Query

#### 1. **Select Object & Fields**
- Object: Account
- Fields: Name, Industry, AnnualRevenue

#### 2. **Add Filters**
- Industry = 'Technology'
- AnnualRevenue > 1000000

#### 3. **Add Sorting** ⭐ NEW!
- Click "Add Sort Field"
- Field: AnnualRevenue
- Direction: Descending (Z-A, 9-0)
- Click "Add Sort"

#### 4. **Set Limit** ⭐ NEW!
- Click "Top 10" quick button
- Or enter custom number (e.g., 25)

#### 5. **See Complete Query**
```sql
SELECT
  Name,
  Industry,
  AnnualRevenue
FROM Account
WHERE Industry = 'Technology'
  AND AnnualRevenue > 1000000
ORDER BY AnnualRevenue DESC
LIMIT 10
```

---

## 🎨 Sort Features

### ✨ Add Sort Fields
1. Click "Add Sort Field" button
2. Select field from dropdown
3. Choose direction:
   - **Ascending** (A-Z, 0-9, smallest to largest)
   - **Descending** (Z-A, 9-0, largest to smallest)
4. Click "Add Sort"

### 📊 Sort Order Matters
Sort fields are applied in order:
```sql
ORDER BY AnnualRevenue DESC, Name ASC
```
- First sorts by AnnualRevenue (highest first)
- Then sorts by Name within each revenue level

### 🔄 Reorder Sort Fields
- Use ⬆️ up/down ⬇️ arrows to change priority
- Drag grip icon (future: drag & drop)
- Primary sort = top of list

### ❌ Remove Sort Fields
- Click trash icon on any sort field
- Reset button clears all

---

## 🔢 Limit Features

### ✨ Quick Limit Buttons
- **Top 10** - Most common, fast queries
- **Top 25** - Good for reports
- **Top 50** - Larger datasets
- **Top 100** - Comprehensive view

### ⌨️ Custom Limit
- Enter any number 1-2000
- Leave blank for no limit

### 🔄 Clear Limit
- Click X button
- Or use Reset

---

## 📊 Complete Examples

### Example 1: Top 10 High-Value Tech Companies
```
Object: Account
Fields: Name, Industry, AnnualRevenue, Website

Filters:
- Industry = 'Technology'
- AnnualRevenue > 5000000

Sort:
- AnnualRevenue DESC

Limit: 10

Result:
SELECT Name, Industry, AnnualRevenue, Website
FROM Account
WHERE Industry = 'Technology'
  AND AnnualRevenue > 5000000
ORDER BY AnnualRevenue DESC
LIMIT 10
```

### Example 2: Recent Opportunities by Amount
```
Object: Opportunity
Fields: Name, Amount, CloseDate, StageName

Filters:
- CreatedDate >= 2024-01-01
- StageName != 'Closed Lost'

Sort:
- Amount DESC
- CloseDate ASC

Limit: 25

Result:
SELECT Name, Amount, CloseDate, StageName
FROM Opportunity
WHERE CreatedDate >= 2024-01-01
  AND StageName != 'Closed Lost'
ORDER BY Amount DESC, CloseDate ASC
LIMIT 25
```

### Example 3: Alphabetical Contact List
```
Object: Contact
Fields: FirstName, LastName, Email, Phone

Filters:
- Email != ''

Sort:
- LastName ASC
- FirstName ASC

Limit: 50

Result:
SELECT FirstName, LastName, Email, Phone
FROM Contact
WHERE Email != ''
ORDER BY LastName ASC, FirstName ASC
LIMIT 50
```

### Example 4: Latest Cases
```
Object: Case
Fields: CaseNumber, Subject, CreatedDate, Status

Sort:
- CreatedDate DESC

Limit: 100

Result:
SELECT CaseNumber, Subject, CreatedDate, Status
FROM Case
ORDER BY CreatedDate DESC
LIMIT 100
```

---

## 🎯 UI Layout

```
┌──────────────────────────────────────────────────────────┐
│                 Sort & Limit Card                         │
├────────────────────┬─────────────────────────────────────┤
│ Sort Order         │ Limit Results                       │
│ [Add Sort Field]   │                                     │
│                    │ [___________]  [X]                  │
│ AnnualRevenue DESC │                                     │
│   [↑] [↓] [🗑️]    │ Maximum records (1-2000)            │
│                    │                                     │
│ Name ASC           │ [Top 10]                            │
│   [↑] [↓] [🗑️]    │ [Top 25]                            │
│                    │ [Top 50]                            │
│ Sort matters!      │ [Top 100]                           │
└────────────────────┴─────────────────────────────────────┘
```

---

## 🔧 Technical Details

### Integration Points:

**1. When Object Selected:**
```javascript
// report-builder.js
loadObjectDetails(objectName) {
    // Pass fields to SortLimit
    window.SortLimit.setAvailableFields(data.fields);
    $('#addSortBtn').prop('disabled', false);
}
```

**2. When Sort Added/Changed:**
```javascript
// sort-limit.js
addSortField() {
    sortFields.push(sortField);
    renderSortFields();
    // Trigger query update
    window.updateQueryPreviewFromSortLimit();
}
```

**3. When Limit Changed:**
```javascript
// sort-limit.js
$('#limitInput').on('input', function() {
    limitValue = parseInt($(this).val());
    updateQueryPreview();
});
```

**4. Query Generation:**
```javascript
// report-builder.js
updateQueryPreview() {
    let query = `SELECT ${fields} FROM ${object}`;
    
    // Add WHERE
    query += `\n${whereClause}`;
    
    // Add ORDER BY
    const orderByClause = window.SortLimit.getOrderByClause();
    if (orderByClause) {
        query += `\n${orderByClause}`;
    }
    
    // Add LIMIT
    const limitClause = window.SortLimit.getLimitClause();
    if (limitClause) {
        query += `\n${limitClause}`;
    }
}
```

---

## 📁 Files Created/Modified

```
ReportBuilder.Web.Mvc/
├── Views/ReportBuilder/
│   └── Index.cshtml                ✏️ UPDATED - Added Sort & Limit card
└── wwwroot/js/
    ├── sort-limit.js               ⭐ NEW - 350+ lines!
    └── report-builder.js           ✏️ UPDATED - Integration
```

---

## ✨ Features

### ✅ Sort Features:
- Add multiple sort fields
- Ascending/Descending per field
- Reorder sort priority (up/down arrows)
- Remove individual sort fields
- Visual indicators (icons)

### ✅ Limit Features:
- Quick limit buttons (10, 25, 50, 100)
- Custom limit input (1-2000)
- Clear limit button
- Validation (min 1, max 2000)

### ✅ Integration:
- Context-aware (disabled until object selected)
- Real-time query updates
- Works with filters seamlessly
- Clears on reset

---

## 🧪 Testing Checklist

- [ ] Open Report Builder
- [ ] Select object → Sort & Limit buttons enable
- [ ] Click "Add Sort Field" → Modal opens
- [ ] Add sort with field and direction
- [ ] Sort appears in list
- [ ] Add second sort field
- [ ] Use up/down arrows to reorder
- [ ] Remove a sort field
- [ ] Click "Top 10" quick button → Limit set
- [ ] Enter custom limit (e.g., 25)
- [ ] Clear limit with X button
- [ ] See complete query with ORDER BY and LIMIT
- [ ] Reset → Everything clears

---

## 💡 Best Practices

### When to Use Sort:
- **Always** when using LIMIT (to get consistent results)
- Rankings (top 10, best performers)
- Chronological lists (newest first, oldest first)
- Alphabetical lists (contacts, accounts)

### When to Use Limit:
- Large datasets (performance)
- Top N queries (top 10 opportunities)
- Pagination (get first 50, then next 50)
- Testing queries (limit 5 to test quickly)

### Sort + Limit Best Practices:
```sql
-- ✅ GOOD: Sort before limit
SELECT Name, Amount FROM Opportunity
ORDER BY Amount DESC
LIMIT 10

-- ❌ BAD: Limit without sort (random results)
SELECT Name, Amount FROM Opportunity
LIMIT 10
```

---

## 🎉 Complete Features Now!

Your Report Builder now has ALL essential query building features:

1. ✅ **SELECT** - Field selection
2. ✅ **FROM** - Object selection
3. ✅ **WHERE** - Filters with AND/OR
4. ✅ **ORDER BY** - Sorting ⭐ NEW!
5. ✅ **LIMIT** - Result limits ⭐ NEW!

**You can now build complete, production-ready SOQL queries!** 🚀

---

## 📊 Full Query Example

```sql
SELECT
  Name,
  Industry,
  AnnualRevenue,
  Website,
  CreatedDate
FROM Account
WHERE Industry = 'Technology'
  AND AnnualRevenue > 1000000
  AND CreatedDate >= 2024-01-01
ORDER BY AnnualRevenue DESC, Name ASC
LIMIT 25
```

**This query:**
- Selects 5 fields
- From Account object
- Filters tech companies with >$1M revenue created in 2024
- Sorts by revenue (high to low), then name (A-Z)
- Returns top 25 results

**Perfect for finding your best tech customers!** 🎯
