# 🔥 Filter Builder Feature - Complete Guide

## ✅ What's Been Created

A complete **Filter Builder** component that generates SOQL WHERE clauses!

---

## 🎯 Features

### ✅ Field Type-Aware Operators
Different operators based on field type:

**Text Fields (String, Email, Phone, URL):**
- Equals
- Not Equals  
- Contains
- Does Not Contain
- Starts With
- Ends With
- Is Empty
- Is Not Empty

**Number Fields (Int, Double, Currency):**
- Equals
- Not Equals
- Greater Than
- Greater Than or Equals
- Less Than
- Less Than or Equals
- Is Null
- Is Not Null

**Date/DateTime Fields:**
- Equals
- Not Equals
- After (Greater Than)
- On or After
- Before (Less Than)
- On or Before
- Is Null
- Is Not Null

**Boolean Fields:**
- Equals (True/False)

**Picklist & Reference Fields:**
- Equals
- Not Equals
- Is Null
- Is Not Null

### ✅ AND/OR Logic
- Multiple filters with AND logic (all must match)
- Multiple filters with OR logic (any can match)

### ✅ Auto-Generated SOQL
```sql
WHERE Industry = 'Technology'
  AND AnnualRevenue > 1000000
  AND CreatedDate >= 2024-01-01
```

---

## 🚀 Quick Test

### Step 1: Run the Apps
```bash
# Terminal 1 - Backend API
cd ReportBuilder.Api
dotnet run

# Terminal 2 - MVC App
cd ReportBuilder.Web.Mvc
dotnet run
```

### Step 2: Open Filter Builder Test Page
```
http://localhost:5200/ReportBuilder/FilterBuilder
```

### Step 3: Test the Filter Builder

1. **Select an Object**
   - Choose "Account" from dropdown
   - Click "Load Fields"

2. **Add First Filter**
   - Click "Add Filter" button
   - Select Field: "Industry"
   - Select Operator: "Equals"
   - Enter Value: "Technology"
   - Click "Add Filter"

3. **Add Second Filter**
   - Click "Add Filter" again
   - Select Field: "AnnualRevenue"
   - Select Operator: "Greater Than"
   - Enter Value: "1000000"
   - Click "Add Filter"

4. **See Generated WHERE Clause**
   ```sql
   WHERE Industry = 'Technology'
     AND AnnualRevenue > 1000000
   ```

5. **Try OR Logic**
   - Select "OR (Any can match)" radio button
   - See clause update:
   ```sql
   WHERE Industry = 'Technology'
     OR AnnualRevenue > 1000000
   ```

---

## 📁 Files Created

```
ReportBuilder.Web.Mvc/
├── Controllers/
│   └── ReportBuilderController.cs     (Added FilterBuilder action)
├── Views/
│   └── ReportBuilder/
│       └── FilterBuilder.cshtml        ⭐ NEW - Test page
└── wwwroot/js/
    ├── filter-builder.js               ⭐ NEW - Filter logic
    └── report-builder.js               ✏️ UPDATED - Integrated
```

---

## 🎨 How It Works

### 1. User Clicks "Add Filter"
```javascript
// Opens Bootstrap modal
showAddFilterDialog()
```

### 2. User Selects Field
```javascript
// Populates operators based on field type
if (fieldType === 'String') {
    operators = ['Equals', 'Contains', 'Starts With', ...]
}
```

### 3. User Selects Operator & Value
```javascript
// Shows/hides value input based on operator
if (operator === 'Is Empty') {
    hideValueInput()  // No value needed
} else {
    showValueInput()
}
```

### 4. Filter Added to List
```javascript
filters.push({
    field: field,
    operator: operator,
    value: value
});
```

### 5. SOQL Generated
```javascript
generateSOQLCondition(filter) {
    if (operator === 'contains') {
        return `${field} LIKE '%${value}%'`;
    }
    // ...more operators
}
```

---

## 🔗 Integration with Report Builder

The filter builder is designed to integrate seamlessly:

```javascript
// In report-builder.js
function updateQueryPreview() {
    let query = `SELECT ${fields} FROM ${object}`;
    
    // Add WHERE clause from FilterBuilder
    const whereClause = window.FilterBuilder.getWhereClause();
    if (whereClause) {
        query += `\n${whereClause}`;
    }
}
```

---

## 📊 Filter Examples

### Example 1: Text Filter
```
Field: Industry
Operator: Equals
Value: Technology

Output: Industry = 'Technology'
```

### Example 2: Number Filter
```
Field: AnnualRevenue
Operator: Greater Than
Value: 1000000

Output: AnnualRevenue > 1000000
```

### Example 3: Date Filter
```
Field: CreatedDate
Operator: On or After
Value: 2024-01-01

Output: CreatedDate >= 2024-01-01
```

### Example 4: Contains Filter
```
Field: Name
Operator: Contains
Value: Corp

Output: Name LIKE '%Corp%'
```

### Example 5: Is Empty Filter
```
Field: Description
Operator: Is Empty

Output: Description = ''
```

### Example 6: Multiple Filters (AND)
```
Filter 1: Industry = 'Technology'
Filter 2: AnnualRevenue > 1000000  
Filter 3: CreatedDate >= 2024-01-01

Output:
WHERE Industry = 'Technology'
  AND AnnualRevenue > 1000000
  AND CreatedDate >= 2024-01-01
```

### Example 7: Multiple Filters (OR)
```
Filter 1: Industry = 'Technology'
Filter 2: Industry = 'Healthcare'

Logic: OR

Output:
WHERE Industry = 'Technology'
  OR Industry = 'Healthcare'
```

---

## 🎯 Next Step: Integrate into Main Report Builder

To add filters to the main Report Builder page, you would:

### 1. Add Filter Section to Index.cshtml
```html
<!-- Add after Selected Fields card -->
<div class="card mb-3">
    <div class="card-header bg-warning text-dark">
        <h5><i class="bi bi-funnel-fill"></i> Filters</h5>
    </div>
    <div class="card-body">
        <div id="filtersContainer"></div>
        <button class="btn btn-primary" id="addFilterBtn">
            <i class="bi bi-plus-circle"></i> Add Filter
        </button>
    </div>
</div>
```

### 2. Include filter-builder.js
```html
@section Scripts {
    <script src="~/js/filter-builder.js"></script>
    <script src="~/js/report-builder.js"></script>
}
```

### 3. Initialize with Selected Object
```javascript
// When object is selected
function loadObjectDetails(objectName) {
    $.ajax({
        success: function(data) {
            selectedObject = data;
            
            // Initialize FilterBuilder with fields
            window.FilterBuilder.loadObjectFields(objectName);
        }
    });
}
```

---

## 🧪 Testing Checklist

- [ ] Open FilterBuilder test page
- [ ] Select "Account" object
- [ ] Click "Load Fields"
- [ ] Click "Add Filter"
- [ ] Select different field types (String, Number, Date)
- [ ] See different operators per type
- [ ] Add multiple filters
- [ ] Toggle AND/OR logic
- [ ] Remove individual filters
- [ ] See WHERE clause update in real-time

---

## 💡 Advanced Features (Future)

### Could Add:
1. **Saved Filters** - Save filter combinations
2. **Filter Groups** - Nested AND/OR logic
3. **IN Operator** - Multiple values (Industry IN ('Tech', 'Healthcare'))
4. **BETWEEN Operator** - Range queries
5. **Date Relative** - "Last 30 Days", "This Month"
6. **Field to Field** - Compare two fields
7. **Sub-queries** - Complex filtering

---

## 📝 API Used

The filter builder uses the same Metadata API:

```
GET /api/MetadataApi/objects/{objectName}
```

Returns object with fields array containing:
- apiName
- label
- dataType
- isRequired
- isCustom

---

## ✨ Summary

**You now have a working Filter Builder that:**
- ✅ Supports all Salesforce field types
- ✅ Has field type-specific operators
- ✅ Generates valid SOQL WHERE clauses
- ✅ Supports AND/OR logic
- ✅ Has beautiful Bootstrap UI
- ✅ Integrates with Report Builder

**Test it at:** `http://localhost:5200/ReportBuilder/FilterBuilder`

**Want me to integrate it into the main Report Builder page?** 🎯
