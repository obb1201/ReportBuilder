# 🎉 ASP.NET Core MVC Project - jQuery & Bootstrap 5

## ✅ What's Been Created

A complete ASP.NET Core MVC project with modern jQuery and Bootstrap 5!

### 📦 Project Structure:

```
ReportBuilder.Web.Mvc/
├── Controllers/
│   ├── HomeController.cs
│   └── ReportBuilderController.cs
├── Models/
│   └── ErrorViewModel.cs
├── Views/
│   ├── Home/
│   │   └── Index.cshtml
│   ├── ReportBuilder/
│   │   └── Index.cshtml
│   ├── Shared/
│   │   └── _Layout.cshtml
│   ├── _ViewStart.cshtml
│   └── _ViewImports.cshtml
├── wwwroot/
│   ├── css/
│   │   └── site.css
│   └── js/
│       ├── site.js
│       └── report-builder.js
├── Program.cs
├── appsettings.json
└── ReportBuilder.Web.Mvc.csproj
```

---

## 🚀 Technologies Used

- **ASP.NET Core 9.0 MVC**
- **jQuery 3.7.1** (Latest)
- **Bootstrap 5.3.2** (Latest)
- **Bootstrap Icons 1.11.2**
- **Razor Pages** with Runtime Compilation

---

## 🎨 Features Implemented

### ✅ Home Page
- Beautiful landing page with Bootstrap cards
- Feature highlights
- Navigation to Report Builder

### ✅ Report Builder Page
- **Object Selector** with search
- **Available Fields** panel with filtering
- **Selected Fields** display with remove buttons
- **SOQL Query Preview** with copy-to-clipboard
- **Real-time updates** using jQuery
- **Toast notifications** for user feedback
- **Responsive design** (mobile-friendly)

---

## 🏃 Quick Start

### Step 1: Build
```bash
cd C:\Work\Projects\ReportBuilder
dotnet build
```

### Step 2: Run API (Terminal 1)
```bash
cd ReportBuilder.Api
dotnet run
```
✅ Wait for: `Now listening on: http://localhost:5000`

### Step 3: Run MVC App (Terminal 2)
```bash
cd ReportBuilder.Web.Mvc
dotnet run
```
✅ Wait for: `Now listening on: http://localhost:5200`

### Step 4: Open Browser
```
http://localhost:5200
```

---

## 🎯 How It Works

### 1. **Home Page** (`/`)
- Shows welcome message
- Three feature cards
- Links to Report Builder

### 2. **Report Builder** (`/ReportBuilder`)

**Left Side:**
- Search and select Salesforce objects
- Filter and browse available fields
- Click fields to add to report

**Right Side:**
- View selected fields
- See generated SOQL query
- Copy query to clipboard
- Remove individual fields or clear all

**Features:**
- ✅ Real-time search (objects and fields)
- ✅ Live SOQL query generation
- ✅ Copy to clipboard functionality
- ✅ Toast notifications
- ✅ Field type icons and badges
- ✅ Required/Custom field indicators

---

## 📝 jQuery Features

The `report-builder.js` file includes:

```javascript
// AJAX calls to API
$.ajax({
    url: `${API_BASE_URL}/api/metadata/objects`,
    method: 'GET',
    success: function(data) { ... }
});

// Event handlers
$('#objectSelect').on('change', function() { ... });
$('#fieldSearch').on('input', function() { ... });

// DOM manipulation
$('#fieldsList').append($fieldItem);
$('#queryPreview code').text(query);

// Toast notifications
function showToast(message, type) { ... }
```

---

## 🎨 Bootstrap 5 Components Used

- **Cards** - for panels
- **Forms** - selects, inputs, buttons
- **Badges** - for field types and counts
- **Icons** - Bootstrap Icons throughout
- **Grid System** - responsive layout
- **Toasts** - notifications
- **Utilities** - spacing, colors, flex

---

## 🔧 Customization

### Change API URL

Edit `appsettings.json`:
```json
{
  "ApiBaseUrl": "http://localhost:5000"
}
```

### Change Port

Edit `Properties/launchSettings.json` (if exists) or use:
```bash
dotnet run --urls "http://localhost:YOUR_PORT"
```

### Customize Styles

Edit `wwwroot/css/site.css`:
```css
/* Add your custom styles */
.my-custom-class {
    color: blue;
}
```

### Add More Features

Edit `wwwroot/js/report-builder.js`:
```javascript
// Add your custom JavaScript
function myNewFeature() {
    // Your code here
}
```

---

## 🎯 Testing Checklist

After starting the app:

- [ ] Home page loads
- [ ] Navigation works
- [ ] Click "Build Report" → goes to Report Builder
- [ ] Objects load in dropdown
- [ ] Search objects works
- [ ] Select an object → fields appear
- [ ] Search fields works
- [ ] Click a field → appears in Selected Fields
- [ ] SOQL query generates correctly
- [ ] Copy button works
- [ ] Remove field works
- [ ] Clear all works
- [ ] Reset button works
- [ ] Toast notifications appear

---

## 📊 Key Files Explained

### `report-builder.js` - Main jQuery Logic
- **loadObjects()** - Fetches all objects from API
- **loadObjectDetails()** - Gets fields for selected object
- **addFieldToReport()** - Adds field to selected list
- **updateQueryPreview()** - Generates SOQL query
- **copyQueryToClipboard()** - Clipboard functionality
- **showToast()** - Bootstrap toast notifications

### `Index.cshtml` - Report Builder View
- Bootstrap grid layout
- Form controls (select, input)
- Cards for organization
- Data attributes for jQuery

### `site.css` - Custom Styles
- Field item hover effects
- Query preview styling
- Custom scrollbars
- Toast positioning

---

## 🚀 Next Steps

### Add More Features:
1. **Filters** - WHERE clause builder
2. **Sorting** - ORDER BY selector
3. **Limits** - LIMIT/OFFSET controls
4. **Save Reports** - Local storage or database
5. **Execute Queries** - Run against Salesforce
6. **Export Results** - CSV/Excel download

---

## 💡 Advantages of MVC + jQuery

### ✅ Pros:
- Familiar technology stack
- No complex build process
- Works in all browsers
- Easy to debug
- Server-side rendering
- SEO-friendly
- Simple deployment

### vs Blazor:
- Faster page loads (no WebAssembly)
- No SignalR connection needed
- Traditional request/response model
- More control over HTML
- Easier to integrate with existing systems

---

## 🆘 Troubleshooting

### Issue: API calls fail

**Check:**
```javascript
// In browser console
console.log(API_BASE_URL);
// Should show: http://localhost:5000
```

**Fix:** Make sure API is running on port 5000

---

### Issue: Objects don't load

**Check API:**
```bash
curl http://localhost:5000/api/metadata/objects
```

**Should return JSON array of objects**

---

### Issue: jQuery not working

**Check browser console:**
- Open F12 Developer Tools
- Look for JavaScript errors
- Verify jQuery is loaded

**Check _Layout.cshtml:**
```html
<!-- Should have these -->
<script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>
<script src="~/js/report-builder.js"></script>
```

---

## ✨ You're Ready!

Your MVC project is complete with:
- ✅ Modern jQuery 3.7.1
- ✅ Bootstrap 5.3.2
- ✅ Working Report Builder
- ✅ Full API integration
- ✅ Beautiful responsive UI

**Start the app and build some reports!** 🚀

---

**Questions? Want to add features?** Let me know! 🎯
