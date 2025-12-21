# 🏗️ MVC Architecture - All API Calls Through Controllers

## ✅ Architecture Pattern

**All jQuery AJAX calls now go through MVC controllers - proper separation of concerns!**

---

## 📊 Request Flow Diagram

```
┌─────────────┐
│   Browser   │
│  (jQuery)   │
└──────┬──────┘
       │
       │ AJAX: /api/MetadataApi/objects
       ▼
┌─────────────────────────────────────┐
│     MVC Web Application             │
│                                     │
│  ┌──────────────────────────────┐  │
│  │  MetadataApiController       │  │
│  │  (API Endpoints)             │  │
│  └────────┬─────────────────────┘  │
│           │                         │
│           │ Calls                   │
│           ▼                         │
│  ┌──────────────────────────────┐  │
│  │  MetadataApiService          │  │
│  │  (Business Logic)            │  │
│  └────────┬─────────────────────┘  │
│           │                         │
│           │ HTTP Call               │
└───────────┼─────────────────────────┘
            │
            │ http://localhost:5000/api/metadata/objects
            ▼
┌─────────────────────────────────────┐
│   Backend API (ReportBuilder.Api)   │
│                                     │
│  ┌──────────────────────────────┐  │
│  │  MetadataController          │  │
│  └────────┬─────────────────────┘  │
│           │                         │
│           ▼                         │
│  ┌──────────────────────────────┐  │
│  │  MetadataRepository          │  │
│  └────────┬─────────────────────┘  │
│           │                         │
│           ▼                         │
│  ┌──────────────────────────────┐  │
│  │     SQL Server Database      │  │
│  └──────────────────────────────┘  │
└─────────────────────────────────────┘
```

---

## 🔄 Request Flow Example

### Example: Loading Salesforce Objects

**1. User Action (Browser)**
```javascript
// jQuery in report-builder.js
function loadObjects() {
    $.ajax({
        url: '/api/MetadataApi/objects',  // ← MVC Controller endpoint
        method: 'GET',
        success: function(data) {
            displayObjects(data);
        }
    });
}
```

**2. MVC API Controller**
```csharp
// Controllers/MetadataApiController.cs
[HttpGet("objects")]
public async Task<IActionResult> GetObjects()
{
    var objects = await _metadataService.GetObjectsAsync();
    return Ok(objects);  // ← Returns JSON to jQuery
}
```

**3. Service Layer**
```csharp
// Services/MetadataApiService.cs
public async Task<List<MetadataObject>> GetObjectsAsync()
{
    var client = _httpClientFactory.CreateClient("MetadataApi");
    var response = await client.GetAsync("/api/metadata/objects");
    // ↑ Calls backend API
    
    var json = await response.Content.ReadAsStringAsync();
    return JsonSerializer.Deserialize<List<MetadataObject>>(json);
}
```

**4. Backend API**
```csharp
// ReportBuilder.Api/Controllers/MetadataController.cs
[HttpGet("objects")]
public async Task<ActionResult<List<MetadataObject>>> GetObjects()
{
    var objects = await _repository.GetAllObjectsAsync();
    return Ok(objects);  // ← Returns data from database
}
```

---

## 📁 Project Structure

```
ReportBuilder.Web.Mvc/
├── Controllers/
│   ├── HomeController.cs              (Views)
│   ├── ReportBuilderController.cs     (Views)
│   └── MetadataApiController.cs       (API) ⭐ NEW
│
├── Services/
│   └── MetadataApiService.cs          ⭐ NEW
│
├── Views/
│   └── ReportBuilder/Index.cshtml
│
└── wwwroot/js/
    └── report-builder.js              (Calls MVC API)
```

---

## 🎯 Benefits of This Architecture

### ✅ Separation of Concerns
- **jQuery** → Only handles UI/UX
- **MVC Controller** → Orchestrates requests
- **Service Layer** → Business logic & API calls
- **Backend API** → Data access

### ✅ Security
- Backend API URL hidden from client
- Can add authentication/authorization in controller
- Can validate/sanitize requests before forwarding
- CORS issues eliminated

### ✅ Flexibility
- Easy to add caching in service layer
- Can transform data before sending to client
- Can aggregate multiple API calls
- Easy to mock for testing

### ✅ Maintainability
- Clear separation of responsibilities
- Changes to backend API don't affect jQuery
- Single place to manage API calls
- Easy to add logging/error handling

---

## 🔐 Security Enhancements (Optional)

### Add Authorization
```csharp
[Authorize]  // Require authentication
[HttpGet("objects")]
public async Task<IActionResult> GetObjects()
{
    // Only authenticated users can access
}
```

### Add Rate Limiting
```csharp
[RateLimit(10, 60)]  // 10 requests per minute
[HttpGet("objects")]
public async Task<IActionResult> GetObjects()
{
    // Prevents abuse
}
```

### Add Request Validation
```csharp
[HttpGet("objects/{apiName}")]
public async Task<IActionResult> GetObject(string apiName)
{
    if (!IsValidObjectName(apiName))
    {
        return BadRequest("Invalid object name");
    }
    // ...
}
```

---

## 🚀 API Endpoints in MVC

All jQuery calls now use these MVC endpoints:

| jQuery Call | MVC Endpoint | Backend API |
|------------|--------------|-------------|
| `GET /api/MetadataApi/objects` | `MetadataApiController.GetObjects()` | `GET http://localhost:5000/api/metadata/objects` |
| `GET /api/MetadataApi/objects/{name}` | `MetadataApiController.GetObject()` | `GET http://localhost:5000/api/metadata/objects/{name}` |
| `GET /api/MetadataApi/objects/{name}/fields` | `MetadataApiController.GetFields()` | `GET http://localhost:5000/api/metadata/objects/{name}/fields` |
| `GET /api/MetadataApi/search?query=X` | `MetadataApiController.SearchObjects()` | `GET http://localhost:5000/api/metadata/search?query=X` |

---

## 🔧 Configuration

### appsettings.json
```json
{
  "ApiBaseUrl": "http://localhost:5000"
}
```

### Program.cs
```csharp
// Register HttpClient
builder.Services.AddHttpClient("MetadataApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]);
});

// Register Service
builder.Services.AddScoped<MetadataApiService>();
```

---

## 🎯 How It Works Now

### Old Way (Direct API Calls) ❌
```javascript
// jQuery called backend API directly
$.ajax({
    url: 'http://localhost:5000/api/metadata/objects',  // ← Hardcoded!
    // ...
});
```

**Problems:**
- Backend URL exposed to client
- CORS issues
- No server-side validation
- Can't add caching
- Hard to secure

### New Way (Through MVC Controller) ✅
```javascript
// jQuery calls MVC controller
$.ajax({
    url: '/api/MetadataApi/objects',  // ← Relative URL!
    // ...
});
```

**Benefits:**
- Backend URL hidden
- No CORS issues (same origin)
- Can add validation/caching
- Easy to secure
- Clean architecture

---

## 📝 Testing the New Architecture

### 1. Start Backend API
```bash
cd ReportBuilder.Api
dotnet run
```
✅ Runs on port 5000

### 2. Start MVC App
```bash
cd ReportBuilder.Web.Mvc
dotnet run
```
✅ Runs on port 5200

### 3. Open Browser
```
http://localhost:5200/ReportBuilder
```

### 4. Check Network Tab
In browser DevTools → Network:
- You'll see AJAX calls to `/api/MetadataApi/objects`
- NOT to `http://localhost:5000...`
- Server-side logs show calls from MVC → Backend API

---

## 🎉 Summary

**Architecture Pattern:**
```
Browser (jQuery) 
  → MVC Controller (API endpoint)
    → Service Layer (Business logic)
      → Backend API (Data access)
        → Database
```

**All API calls are now properly abstracted through MVC controllers!**

This is production-ready, secure, and maintainable architecture. 🚀
