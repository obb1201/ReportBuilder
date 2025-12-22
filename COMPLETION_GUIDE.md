# 🚀 COMPLETION GUIDE - Final Steps

## ✅ What's Done (Backend 100%)

1. ✅ DynamicTableService
2. ✅ DataGeneratorService  
3. ✅ SoqlToSqlConverter
4. ✅ QueryExecutionService
5. ✅ Data API Controller
6. ✅ DataApiService (MVC)
7. ✅ DataApiController (MVC proxy)
8. ✅ All services registered

**Backend is 100% complete and ready to use!**

---

## 📋 What's Left (Frontend - 1 hour)

### Step 1: Run Database Migration (5 min)

```bash
# In SQL Server Management Studio or sqlcmd
USE ReportBuilderDb;
GO

# Run the migration file
# File: ReportBuilder.Infrastructure/Data/Migrations/002_DynamicDataSchema.sql
```

**Or use sqlcmd:**
```bash
cd ReportBuilder.Infrastructure/Data/Migrations
sqlcmd -S (localdb)\MSSQLLocalDB -d ReportBuilderDb -i 002_DynamicDataSchema.sql
```

---

### Step 2: Add JavaScript Functions (30 min)

Add these functions to `/wwwroot/js/report-builder-v2.js`:

#### A. Add at top with other variables:
```javascript
let selectedObjectHasData = false;
```

#### B. Add to `attachEventHandlers()` function:
```javascript
// Run Query button (already exists in HTML)
$('#runQueryBtn').on('click', executeQuery);
```

#### C. Add after `loadObjectDetails()` function:
```javascript
// Check if object has data
async function checkObjectHasData(objectName) {
    try {
        const response = await $.ajax({
            url: `/api/DataApi/check/${objectName}`,
            method: 'GET'
        });
        
        selectedObjectHasData = response.hasData;
        updateRunQueryButton();
        
        if (response.hasData) {
            showToast(`${objectName} has sample data ready`, 'success');
        } else {
            showToast(`${objectName} needs sample data - use Setup Data`, 'warning');
        }
    } catch (error) {
        console.error('Error checking object data:', error);
        selectedObjectHasData = false;
    }
}

// Setup sample data for object
async function setupObjectData() {
    if (!selectedObject) {
        showToast('Please select an object first', 'warning');
        return;
    }
    
    if (confirm(`Generate 500 sample records for ${selectedObject.label}?\n\nThis may take 10-30 seconds.`)) {
        const $btn = $('#setupDataBtn');
        const originalHtml = $btn.html();
        $btn.html('<span class="spinner-border spinner-border-sm me-1"></span>Generating...').prop('disabled', true);
        
        try {
            const response = await $.ajax({
                url: `/api/DataApi/setup/${selectedObject.apiName}?recordCount=500`,
                method: 'POST',
                timeout: 300000 // 5 minutes
            });
            
            if (response.success) {
                showToast(response.message, 'success');
                selectedObjectHasData = true;
                updateRunQueryButton();
            } else {
                showToast('Failed to generate data', 'danger');
            }
        } catch (error) {
            console.error('Error setting up data:', error);
            showToast('Error generating data: ' + (error.responseJSON?.error || error.statusText), 'danger');
        } finally {
            $btn.html(originalHtml).prop('disabled', false);
        }
    }
}

// Execute SOQL query
async function executeQuery() {
    if (!selectedObject || reportColumns.length === 0) {
        showToast('Please select an object and add columns', 'warning');
        return;
    }
    
    if (!selectedObjectHasData) {
        showToast('Please setup sample data first', 'warning');
        return;
    }
    
    // Generate SOQL query
    const soqlQuery = generateSoqlQuery();
    
    const $btn = $('#runQueryBtn');
    const originalHtml = $btn.html();
    $btn.html('<span class="spinner-border spinner-border-sm me-1"></span>Running...').prop('disabled', true);
    
    try {
        const response = await $.ajax({
            url: '/api/DataApi/execute',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ soqlQuery: soqlQuery }),
            timeout: 60000 // 1 minute
        });
        
        if (response.success) {
            displayQueryResults(response);
            showToast(`Query executed in ${response.executionTimeMs}ms - ${response.recordCount} records`, 'success');
        } else {
            showToast('Query failed: ' + response.error, 'danger');
        }
    } catch (error) {
        console.error('Error executing query:', error);
        showToast('Error executing query: ' + (error.responseJSON?.error || error.statusText), 'danger');
    } finally {
        $btn.html(originalHtml).prop('disabled', !selectedObjectHasData);
    }
}

// Generate SOQL query from current state
function generateSoqlQuery() {
    const fields = reportColumns.map(c => c.apiName).join(', ');
    let query = `SELECT ${fields} FROM ${selectedObject.apiName}`;
    
    // Add WHERE clause from FilterBuilder if available
    if (window.FilterBuilder) {
        const whereClause = window.FilterBuilder.getWhereClause();
        if (whereClause) {
            query += `\n${whereClause}`;
        }
    }
    
    // Add ORDER BY from sorting
    const sortedColumns = reportColumns.filter(c => c.sortDirection);
    if (sortedColumns.length > 0) {
        const orderBy = sortedColumns.map(c => `${c.apiName} ${c.sortDirection}`).join(', ');
        query += `\nORDER BY ${orderBy}`;
    }
    
    // Add LIMIT from page size
    query += `\nLIMIT ${pageSize}`;
    
    return query;
}

// Display query results in grid
function displayQueryResults(response) {
    // Remove existing results modal if any
    $('#queryResultsModal').remove();
    
    // Create results modal
    const modal = `
        <div class="modal fade" id="queryResultsModal" tabindex="-1">
            <div class="modal-dialog modal-xl modal-fullscreen-lg-down">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">
                            <i class="bi bi-table"></i> Query Results 
                            <span class="badge bg-primary ms-2">${response.recordCount} records</span>
                            <span class="badge bg-secondary ms-1">${response.executionTimeMs}ms</span>
                        </h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <div class="mb-3 d-flex justify-content-between align-items-center">
                            <div>
                                <button class="btn btn-sm btn-success" id="exportCsvBtn">
                                    <i class="bi bi-download"></i> Export CSV
                                </button>
                            </div>
                            <div>
                                <small class="text-muted">
                                    <i class="bi bi-info-circle"></i> 
                                    Showing up to ${pageSize} records
                                </small>
                            </div>
                        </div>
                        <div class="table-responsive" style="max-height: 60vh; overflow-y: auto;">
                            <table class="table table-striped table-hover table-sm" id="resultsTable">
                                <thead class="sticky-top bg-light">
                                    <tr>
                                        ${response.columns.map(c => `<th>${c.name}</th>`).join('')}
                                    </tr>
                                </thead>
                                <tbody>
                                    ${response.data.map(row => `
                                        <tr>
                                            ${response.columns.map(c => `
                                                <td>${formatValue(row[c.name])}</td>
                                            `).join('')}
                                        </tr>
                                    `).join('')}
                                </tbody>
                            </table>
                        </div>
                        <div class="mt-3">
                            <strong>SQL Query:</strong>
                            <pre class="bg-light p-2 rounded mt-2"><code>${escapeHtml(response.sqlQuery)}</code></pre>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    $('body').append(modal);
    const modalInstance = new bootstrap.Modal(document.getElementById('queryResultsModal'));
    modalInstance.show();
    
    // CSV Export handler
    $('#exportCsvBtn').on('click', function() {
        exportToCsv(response.columns, response.data);
    });
}

// Format value for display
function formatValue(value) {
    if (value === null || value === undefined) {
        return '<span class="text-muted">null</span>';
    }
    if (typeof value === 'boolean') {
        return value ? '<span class="badge bg-success">true</span>' : '<span class="badge bg-secondary">false</span>';
    }
    if (typeof value === 'number') {
        return value.toLocaleString();
    }
    return escapeHtml(String(value));
}

// Escape HTML
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Export to CSV
function exportToCsv(columns, data) {
    let csv = columns.map(c => c.name).join(',') + '\n';
    
    data.forEach(row => {
        const values = columns.map(c => {
            let value = row[c.name];
            if (value === null || value === undefined) {
                return '';
            }
            value = String(value);
            if (value.includes(',') || value.includes('"') || value.includes('\n')) {
                value = '"' + value.replace(/"/g, '""') + '"';
            }
            return value;
        });
        csv += values.join(',') + '\n';
    });
    
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${selectedObject.apiName}_${new Date().toISOString().split('T')[0]}.csv`;
    a.click();
    window.URL.revokeObjectURL(url);
}

// Update Run Query button state
function updateRunQueryButton() {
    const canRun = selectedObject && reportColumns.length > 0 && selectedObjectHasData;
    $('#runQueryBtn').prop('disabled', !canRun);
}
```

#### D. Update `selectObject()` function - Add after metadata loads:
```javascript
// After: displayColumns(metadata.fields);
// Add:
await checkObjectHasData(objectName);
```

#### E. Update when columns change - Add to column add/remove:
```javascript
updateRunQueryButton();
```

---

### Step 3: Add Setup Data Button to HTML (5 min)

In `/Views/ReportBuilder/Index.cshtml`, find the Objects panel header and add button:

```html
<!-- Find this section around line 33-36 -->
<div class="panel-header">
    <span><i class="bi bi-box"></i> Objects</span>
    <button class="btn btn-sm btn-light" id="setupDataBtn" disabled title="Generate sample data">
        <i class="bi bi-gear-fill"></i> Setup
    </button>
</div>
```

Add event handler in JavaScript:
```javascript
$('#setupDataBtn').on('click', setupObjectData);
```

Enable/disable button when object selected:
```javascript
// In selectObject() after metadata loads:
$('#setupDataBtn').prop('disabled', false);

// In reset:
$('#setupDataBtn').prop('disabled', true);
```

---

### Step 4: Add CSS for Results Modal (5 min)

Add to `/wwwroot/css/site.css`:

```css
/* Query Results Modal */
#queryResultsModal .table {
    font-size: 0.875rem;
}

#queryResultsModal thead th {
    background: #f8f9fa;
    position: sticky;
    top: 0;
    z-index: 10;
    border-bottom: 2px solid #dee2e6;
}

#queryResultsModal tbody tr:hover {
    background-color: #f1f3f5;
}

#queryResultsModal pre {
    font-size: 0.8rem;
    max-height: 200px;
    overflow-y: auto;
}
```

---

## 🚀 Testing Steps

### 1. Start Both Applications
```bash
# Terminal 1 - API
cd ReportBuilder.Api
dotnet run

# Terminal 2 - MVC
cd ReportBuilder.Web.Mvc
dotnet run
```

### 2. Test Workflow

**A. Setup Data:**
1. Open `http://localhost:5200/ReportBuilder`
2. Select "Account" object
3. Click "Setup" button (top-right of Objects panel)
4. Wait 10-30 seconds
5. See success message: "Successfully created Account table with 500 records"

**B. Build Query:**
1. Click columns: Name, Industry, AnnualRevenue
2. They appear in Report Template
3. Drag to reorder (optional)
4. Click column name to sort (optional)

**C. Add Filter:**
1. Click "+ Filter" in Filters panel
2. Field: Industry
3. Operator: Equals  
4. Value: Technology
5. Add filter

**D. Run Query:**
1. Click "Run" button (top-right)
2. Wait 1-2 seconds
3. Modal opens with results
4. See data table with Technology companies
5. Click "Export CSV" to download

### 3. Try Other Objects
- Contact (people)
- Opportunity (sales)
- Case (support tickets)
- Lead (prospects)

Each needs "Setup" first!

---

## 📊 Expected Results

### Account Query Example:
```
SELECT Name, Industry, AnnualRevenue
FROM Account
WHERE Industry = 'Technology'
ORDER BY AnnualRevenue DESC
LIMIT 25

Results (10 records in 42ms):
┌──────────────────┬────────────┬───────────────┐
│ Name             │ Industry   │ AnnualRevenue │
├──────────────────┼────────────┼───────────────┤
│ Acme Corporation │ Technology │    8456234.50 │
│ TechVision Inc   │ Technology │    5234122.75 │
│ Digital Systems  │ Technology │    3876543.25 │
...
```

---

## 🎉 You're Done!

After adding these JavaScript functions and HTML updates:

✅ Setup button generates data
✅ Run button executes queries
✅ Results display in modal
✅ Export to CSV works
✅ Full SOQL query execution!

**Total time: ~1 hour**

---

## 🐛 Troubleshooting

### "Failed to setup object"
- Check API is running on port 5000
- Check database connection string
- Check migration ran successfully

### "Query failed"
- Object might not have data - click Setup first
- Check SOQL syntax in browser console
- Check API logs for errors

### "Timeout"
- Data generation takes time (10-30 sec for 500 records)
- Query execution should be <1 second
- Check network tab in browser dev tools

---

## 📚 What You Built

A complete report builder that:
1. ✅ Creates database tables dynamically
2. ✅ Generates realistic sample data
3. ✅ Converts SOQL to SQL
4. ✅ Executes queries
5. ✅ Displays results
6. ✅ Exports to CSV

**All without connecting to Salesforce!** 🚀

---

## 🎯 Quick Reference

### API Endpoints:
- `POST /api/data/setup/{objectName}` - Generate data
- `POST /api/data/execute` - Run query
- `GET /api/data/check/{objectName}` - Check if has data
- `GET /api/data/objects` - List populated objects

### MVC Proxy Endpoints:
- `POST /api/DataApi/setup/{objectName}`
- `POST /api/DataApi/execute`
- `GET /api/DataApi/check/{objectName}`

### JavaScript Functions:
- `setupObjectData()` - Generate sample data
- `executeQuery()` - Run SOQL query
- `displayQueryResults()` - Show results modal
- `exportToCsv()` - Download CSV

Ready to finish? Just add the JavaScript functions and HTML button! 🎯
