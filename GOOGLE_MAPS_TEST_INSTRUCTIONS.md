# 🧪 Testing Google Maps in Development

## Quick Setup for Local Development

### 1. Configure Your API Key

Open `Aspire.AppHost/appsettings.Development.json` and replace the placeholder:

```json
{
  "GoogleMaps": {
    "ApiKey": "YOUR_ACTUAL_API_KEY_HERE"  // <-- Replace this with your real key
  }
}
```

### 2. Build and Run

Using Aspire (recommended):
```bash
dotnet build
dotnet run --project Aspire.AppHost
```

Or run the Web project directly:
```bash
dotnet run --project Web
```

### 3. Test the Map

1. Navigate to the Aspire dashboard (usually http://localhost:15038)
2. Click on the **lina-web** service link
3. Go to a project details page with coordinates:
   - Example: https://localhost:7032/Public/Projects/Details/a8c3328a-1a51-4cf2-b0ce-4c99ede086c0
   - Or any project that has latitude and longitude values

### 4. Verify Map Features

When the map loads successfully, you should see:

✅ **Interactive Google Map** with the project location marked
✅ **Custom purple marker** at the project coordinates
✅ **Info window** when clicking the marker (shows project name and description)
✅ **"Lugares cercanos"** button (top-left) for searching nearby places
✅ **"Centrar"** button to reset view to project location
✅ **"Cómo llegar"** link in the info window (opens Google Maps directions)

### 5. Check Browser Console

Open Developer Tools (F12) and check the Console tab:

**Success indicators:**
- `Google Maps API loaded and ready`
- No red error messages about API keys

**Common errors and solutions:**

| Error | Solution |
|-------|----------|
| `InvalidKeyMapError` | Your API key is invalid or incorrectly typed |
| `RefererNotAllowedMapError` | Add `localhost:*` to allowed referrers in Google Cloud Console |
| `ApiNotActivatedMapError` | Enable Maps JavaScript API and Places API in Google Cloud Console |
| `Google Maps API key is not configured` | The placeholder wasn't replaced in appsettings.Development.json |

## Troubleshooting

### Map Not Showing?

1. **Check Configuration File**
   ```bash
   # Verify your key is set
   cat Aspire.AppHost/appsettings.Development.json | grep GoogleMaps -A 2
   ```
   Should NOT show `YOUR_DEVELOPMENT_GOOGLE_MAPS_API_KEY`

2. **Verify API Key in Google Cloud Console**
   - Go to: https://console.cloud.google.com/apis/credentials
   - Check your API key settings:
     - ✅ Maps JavaScript API enabled
     - ✅ Places API enabled
     - ✅ HTTP referrers include `localhost:*` and `https://localhost:*`

3. **Clear Browser Cache**
   - Press Ctrl+Shift+R (hard refresh)
   - Or open in Incognito/Private window

4. **Check Project Has Coordinates**
   ```sql
   -- Run this in SQL Server to find projects with coordinates
   SELECT ExternalId, Name, Latitude, Longitude
   FROM businessincubators.Projects
   WHERE Latitude IS NOT NULL AND Longitude IS NOT NULL
   ```

### Testing Without Real API Key?

If you don't have a Google Maps API key yet:

1. The map section will show a placeholder message: "Mapa no disponible - Configuración de Google Maps pendiente"
2. The rest of the page will work normally
3. You can still see the location name and coordinates in text format

## Testing Checklist

- [ ] API key added to `appsettings.Development.json`
- [ ] Application builds without errors: `dotnet build`
- [ ] Aspire.AppHost runs successfully
- [ ] Can navigate to project details page
- [ ] Map loads and shows project location
- [ ] Marker is clickable and shows info window
- [ ] "Lugares cercanos" button opens search modal
- [ ] "Centrar" button resets map view
- [ ] No errors in browser console
- [ ] Map is responsive on different screen sizes

## Environment Variables (Alternative Method)

You can also set the API key as an environment variable:

**Windows (PowerShell):**
```powershell
$env:GoogleMaps__ApiKey = "YOUR_API_KEY"
dotnet run --project Aspire.AppHost
```

**Linux/Mac:**
```bash
export GoogleMaps__ApiKey="YOUR_API_KEY"
dotnet run --project Aspire.AppHost
```

## Next Steps

Once testing is successful:
1. Commit your changes (but NOT the API key!)
2. For production deployment, configure the key in Azure as a secret parameter
3. Consider implementing additional features like clustering for multiple markers

---

**Note:** Never commit your actual API key to version control. Always use placeholders in committed files.