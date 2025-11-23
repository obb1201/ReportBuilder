# 🎉 Blazor UI - Setup and Run Guide

## ✅ What's Been Created

I've generated a complete Blazor Server UI with MudBlazor for your Report Builder!

### 📦 New Project Structure:

```
ReportBuilder.Web/
├── Components/
│   ├── ObjectSelector.razor          # Select Salesforce objects
│   ├── FieldPanel.razor               # Browse and select fields
│   ├── SelectedFieldsDisplay.razor    # Show selected fields
│   └── QueryPreview.razor             # Display generated SOQL
├── Pages/
│   ├── Index.razor                    # Home page
│   └── ReportBuilder.razor            # Main report builder page
├── Services/
│   └── MetadataApiClient.cs           # API communication service
├── Shared/
│   ├── MainLayout.razor               # App layout
│   └── NavMenu.razor                  # Navigation menu
├── wwwroot/css/
│   └── app.css                        # Custom styles
├── Program.cs                          # App configuration
├── appsettings.json                   # Configuration
└── ReportBuilder.Web.csproj           # Project file
```

---

## 🚀 Quick Start (5 minutes)

### Step 1: Build the Project

```bash
cd C:\Users\obb12\Downloads\ReportBuilder-Complete

# Restore packages
dotnet restore

# Build everything
dotnet build
```

### Step 2: Make Sure API is Running

**In Terminal 1:**
```bash
cd ReportBuilder.Api
dotnet run
```

Should see:
```
Now listening on: http://localhost:5000
Now listening on: https://localhost:5001
```

### Step 3: Start the Blazor App

**In Terminal 2 (new terminal):**
```bash
cd ReportBuilder.Web
dotnet run
```

Should see:
```
Now listening on: http://localhost:5100
Now listening on: https://localhost:5101
```

### Step 4: Open in Browser

```
http://localhost:5100
```

Or:
```
https://localhost:5101
```

---

## 🎨 What You'll See

### Home Page (`/`)
- Welcome screen with feature overview
- Cards for Report Builder, Metadata Browser
- Feature list

### Report Builder (`/report-builder`)
```
┌─────────────────────────────────────────────────────────┐
│  Report Builder                                         │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌──────────────────┐  ┌────────────────────────────┐  │
│  │ Select Object    │  │ Selected Fields            │  │
│  │ [Account ▼]      │  │ ☑ Name              [X]    │  │
│  └──────────────────┘  │ ☑ Industry          [X]    │  │
│                        │ ☑ AnnualRevenue     [X]    │  │
│  ┌──────────────────┐  └────────────────────────────┘  │
│  │ Available Fields │                                   │
│  │ [Search...]      │  ┌────────────────────────────┐  │
│  │                  │  │ SOQL Query Preview         │  │
│  │ ☐ Name           │  │                            │  │
│  │ ☐ Industry       │  │ SELECT Name, Industry,     │  │
│  │ ☐ AnnualRevenue  │  │   AnnualRevenue            │  │
│  │ ☐ Phone          │  │ FROM Account               │  │
│  │ ... more         │  │                            │  │
│  └──────────────────┘  └────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

---

## ✨ Features Working Now

### ✅ Object Selector
- Autocomplete search through 946+ objects
- Shows object label and API name
- Displays custom objects with badge
- Icons for common objects (Account, Contact, etc.)

### ✅ Field Panel
- Lists all fields for selected object
- Search/filter fields by name
- Shows field type (String, Number, Currency, etc.)
- Color-coded by data type
- Shows Required and Custom badges
- Click to add to report

### ✅ Selected Fields Display
- Shows all selected fields
- Remove individual fields
- Clear all fields button
- Drag indicator (drag-and-drop coming soon)

### ✅ Query Preview
- Auto-generates SOQL query
- Updates in real-time as fields change
- Copy to clipboard button
- Syntax-highlighted display

---

## 🎯 How to Use

### 1. Select an Object
1. Click on the "Select Object" dropdown
2. Start typing (e.g., "Account")
3. Select from the list

### 2. Add Fields
1. Fields appear in "Available Fields" panel
2. Use search box to filter fields
3. Click on any field to add it
4. Field appears in "Selected Fields"

### 3. View Query
1. Query auto-generates in "Query Preview"
2. Click copy icon to copy query
3. Paste into Salesforce Developer Console

### 4. Remove Fields
1. Click [X] button on any selected field
2. Or click "Clear all" to start over

---

## 🔧 Configuration

### Change API URL

Edit `ReportBuilder.Web/appsettings.json`:
```json
{
  "ApiBaseUrl": "http://localhost:5000"
}
```

### Change Ports

Edit `ReportBuilder.Web/Properties/launchSettings.json`:
```json
{
  "applicationUrl": "https://localhost:5101;http://localhost:5100"
}
```

---

## 🐛 Troubleshooting

### Issue: "Failed to load objects"

**Cause:** API not running or wrong URL

**Fix:**
1. Check API is running: `http://localhost:5000/api/health`
2. Verify `appsettings.json` has correct `ApiBaseUrl`

---

### Issue: No objects appear in dropdown

**Cause:** Metadata not synced to database

**Fix:**
1. Go to API Swagger: `http://localhost:5000/swagger`
2. Use `POST /api/metadata/sync/from-wsdl` to sync metadata

---

### Issue: CORS errors in browser console

**Cause:** API CORS not configured

**Fix:** API already has CORS enabled. Make sure API is running on port 5000.

---

### Issue: Build errors

**Fix:**
```bash
cd ReportBuilder.Web
dotnet clean
dotnet restore
dotnet build
```

---

## 🎨 Customization

### Change Theme Colors

Edit `MainLayout.razor`:
```razor
<MudThemeProvider Theme="@_theme" />

@code {
    private MudTheme _theme = new MudTheme()
    {
        Palette = new PaletteLight()
        {
            Primary = "#1976d2",
            Secondary = "#424242",
            // ... more colors
        }
    };
}
```

### Add Your Logo

1. Put image in `wwwroot/images/logo.png`
2. Update `MainLayout.razor`:
```razor
<MudAppBar>
    <img src="images/logo.png" height="40" />
    <MudText>Report Builder</MudText>
</MudAppBar>
```

---

## 📊 What's Next (Coming Soon)

### Features to Add:
- [ ] Filters (WHERE clause builder)
- [ ] Sorting (ORDER BY)
- [ ] Limit/Offset (pagination)
- [ ] Grouping (GROUP BY)
- [ ] Aggregates (COUNT, SUM, AVG)
- [ ] Relationships (JOIN queries)
- [ ] Save report templates
- [ ] Execute queries via Salesforce API
- [ ] Export results to Excel/CSV
- [ ] Drag-and-drop field reordering

---

## ✅ Success Checklist

After starting the app, verify:

- [ ] API running on port 5000
- [ ] Blazor app running on port 5100/5101
- [ ] Home page loads
- [ ] Navigate to Report Builder page
- [ ] Object dropdown shows objects
- [ ] Select an object (e.g., Account)
- [ ] Fields appear in Available Fields panel
- [ ] Click a field - it appears in Selected Fields
- [ ] Query generates in Query Preview
- [ ] Copy query button works
- [ ] Remove field button works

---

## 🎉 You're Done!

You now have:
- ✅ Working API with metadata
- ✅ Beautiful Blazor UI
- ✅ Object selector
- ✅ Field browser
- ✅ SOQL query generator
- ✅ Real-time preview

**Next step:** Start using it to build reports! Then we can add filters, sorting, and more advanced features.

---

## 📸 Screenshots

Take screenshots of your working app and show off your progress! 🚀

**Have fun building reports!** 🎊
