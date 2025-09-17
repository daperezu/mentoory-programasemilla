# Google Maps Integration Setup Instructions

## 🗺️ Overview
This document provides instructions for setting up Google Maps integration in the LinaSys project's public project details page. The integration displays an interactive map showing the project's location with additional features like nearby places search and custom styling.

## 📋 Prerequisites

1. **Google Cloud Platform Account**: You need a Google Cloud Platform account to create and manage your API key.
2. **Billing Account**: Google Maps API requires a billing account (though Google provides $200 free monthly credit).

## 🔑 Step 1: Obtain Google Maps API Key

1. **Go to Google Cloud Console**
   - Visit: https://console.cloud.google.com/

2. **Create a New Project (or select existing)**
   - Click on the project dropdown at the top
   - Click "New Project"
   - Name it (e.g., "LinaSys Maps")
   - Click "Create"

3. **Enable Required APIs**
   - Go to "APIs & Services" → "Library"
   - Search for and enable these APIs:
     - **Maps JavaScript API** (Required)
     - **Places API** (Required for nearby places search)
     - **Geocoding API** (Optional, for future features)

4. **Create API Credentials**
   - Go to "APIs & Services" → "Credentials"
   - Click "+ CREATE CREDENTIALS" → "API Key"
   - Your API key will be created and displayed

5. **Secure Your API Key** (IMPORTANT!)
   - Click on the API key you just created
   - Under "Application restrictions":
     - Select "HTTP referrers (websites)"
     - Add your allowed referrers:
       ```
       https://localhost:7032/*
       https://yourdomain.com/*
       ```
   - Under "API restrictions":
     - Select "Restrict key"
     - Select only the APIs you enabled:
       - Maps JavaScript API
       - Places API
   - Click "Save"

## 🔧 Step 2: Configure the Application

The Google Maps API key is configured following the same pattern as other external services (like Mailgun) in the LinaSys application, using .NET Aspire's parameter management system.

### For Local Development

1. **Open** `Aspire.AppHost/appsettings.Development.json`

2. **Replace the placeholder** with your actual Google Maps API key:
   ```json
   {
     "GoogleMaps": {
       "ApiKey": "YOUR_DEVELOPMENT_GOOGLE_MAPS_API_KEY"
     }
   }
   ```

   Change `YOUR_DEVELOPMENT_GOOGLE_MAPS_API_KEY` to your actual API key obtained from Google Cloud Console.

### For Production (Azure Deployment)

The application is configured to read the Google Maps API key from Azure parameters, following the same pattern as Mailgun:

1. **In Azure Portal**:
   - Navigate to your Container App or App Service
   - Go to Configuration → Application settings
   - Add a new setting:
     - Name: `googlemaps-apikey`
     - Value: Your production Google Maps API key
   - Mark it as a secret

2. **The Aspire.AppHost** will automatically inject this as an environment variable:
   ```csharp
   // This is already configured in Aspire.AppHost/Program.cs
   googleMapsApiKey = builder.AddParameter("googlemaps-apikey", secret: true);

   webProject.WithEnvironment("GoogleMaps__ApiKey", googleMapsApiKey);
   ```

### Configuration Options

The application supports additional Google Maps configuration through the `GoogleMapsOptions` class:

```json
{
  "GoogleMaps": {
    "ApiKey": "YOUR_API_KEY",
    "Language": "es",              // Default: Spanish
    "Region": "CR",                 // Default: Costa Rica
    "EnablePlacesLibrary": true,   // Enable nearby places search
    "DefaultZoom": 15               // Default zoom level
  }
}
```

### How It Works

1. **Aspire.AppHost** manages the configuration:
   - For local development: reads from `appsettings.Development.json`
   - For production: reads from Azure parameters

2. **GoogleMapsOptions** class (`Web/Options/GoogleMapsOptions.cs`):
   - Binds to the configuration
   - Validates if API key is configured
   - Provides default values

3. **Details.cshtml** uses dependency injection:
   ```csharp
   @inject IOptions<GoogleMapsOptions> GoogleMapsOptions
   ```
   - Automatically checks if API key is configured
   - Falls back gracefully if not configured

## 🚀 Step 3: Test the Integration

1. **Build the project**:
   ```bash
   dotnet build
   ```

2. **Run the application**:
   ```bash
   dotnet run --project Web
   ```
   Or with Aspire:
   ```bash
   dotnet run --project Aspire.AppHost
   ```

3. **Navigate to a project with coordinates**:
   - Go to: https://localhost:7032/Public/Projects/Details/a8c3328a-1a51-4cf2-b0ce-4c99ede086c0
   - Or any project that has latitude and longitude values

4. **Verify the map loads**:
   - You should see an interactive Google Map
   - The project location should be marked with a custom purple marker
   - Clicking the marker shows project information
   - "Lugares cercanos" button allows searching for nearby places
   - "Centrar" button resets the view to the project location

## 🎨 Features Included

### Interactive Map
- Custom styled map matching Phoenix theme colors
- Project location marked with custom SVG marker
- Info window with project details
- "Cómo llegar" (Get directions) link

### Controls
- **Lugares cercanos**: Search for nearby restaurants, banks, parking, transit, cafes
- **Centrar**: Reset map view to project location
- Standard Google Maps controls (zoom, street view, fullscreen)

### Responsive Design
- Desktop: 400px height
- Mobile: 300px height
- Fully responsive controls and info windows

### Error Handling
- Graceful fallback if coordinates are missing
- Loading spinner while map initializes
- Error message if Google Maps fails to load

## 🔍 Troubleshooting

### Map Not Loading

1. **Check Browser Console** (F12):
   - Look for "Google Maps API error" messages
   - Common errors:
     - `InvalidKeyMapError`: API key is invalid
     - `RefererNotAllowedMapError`: Add your domain to allowed referrers
     - `ApiNotActivatedMapError`: Enable the Maps JavaScript API

2. **Verify API Key**:
   - Ensure the API key is correctly set in configuration
   - Check that billing is enabled on your Google Cloud account
   - Verify the key has the correct restrictions

3. **Check Network Tab**:
   - Ensure the Google Maps script is loading (should see request to maps.googleapis.com)
   - Look for any 403 or 401 errors

### Nearby Places Not Working

- Ensure the **Places API** is enabled in Google Cloud Console
- Check that your API key has permission for Places API

### Custom Styling Not Applied

- Clear browser cache (Ctrl+Shift+R)
- Verify the CSS is loading in the Network tab

## 📊 Usage Monitoring

Monitor your API usage in Google Cloud Console:
1. Go to "APIs & Services" → "Metrics"
2. Select your API (Maps JavaScript API)
3. View usage charts and quotas

**Free Tier Limits** (as of 2024):
- $200 monthly credit
- Maps loads: ~28,000 free per month
- Places searches: ~5,000 free per month

## 🔒 Security Best Practices

1. **Never commit API keys** to version control
2. **Use environment-specific keys** (dev, staging, production)
3. **Restrict API keys** by referrer and API
4. **Monitor usage** for unusual activity
5. **Rotate keys periodically**
6. **Use HTTPS** for all production deployments

## 📝 Additional Configuration Options

### Change Map Language
In line 418 of Details.cshtml, modify `language` parameter:
```javascript
&language=es  // Spanish (current)
&language=en  // English
&language=pt  // Portuguese
```

### Change Map Region
Modify `region` parameter to bias results:
```javascript
&region=CR  // Costa Rica (current)
&region=US  // United States
&region=MX  // Mexico
```

### Adjust Map Zoom Level
In `project-details-map.js`, line 32:
```javascript
zoom: 15,  // Current zoom level (street level)
zoom: 10,  // City level
zoom: 18,  // Building level
```

## 🆘 Support

- **Google Maps Documentation**: https://developers.google.com/maps/documentation/javascript
- **Google Cloud Support**: https://cloud.google.com/support
- **Project Issues**: Create an issue in the LinaSys repository

## ✅ Checklist

Before going to production, ensure:

- [ ] API key is obtained and configured
- [ ] API key is restricted by domain
- [ ] API key is restricted to required APIs only
- [ ] Environment-specific configuration is set up
- [ ] Map loads correctly on all target pages
- [ ] Nearby places search works
- [ ] Error handling is tested (invalid coordinates, API failures)
- [ ] Mobile responsiveness is verified
- [ ] Usage monitoring is set up
- [ ] Billing alerts are configured in Google Cloud Console

## 📄 Files Modified

The following files were created or modified for this integration:

1. **Created**:
   - `/Web/wwwroot/js/public/project-details-map.js` - Main map functionality
   - `/Web/Options/GoogleMapsOptions.cs` - Configuration options class
   - `/GOOGLE_MAPS_SETUP.md` - This setup guide

2. **Modified**:
   - `/Aspire.AppHost/Program.cs` - Added Google Maps parameter configuration
   - `/Aspire.AppHost/appsettings.json` - Added GoogleMaps configuration section
   - `/Aspire.AppHost/appsettings.Development.json` - Added GoogleMaps with placeholder key
   - `/Web/Program.cs` - Registered GoogleMapsOptions configuration
   - `/Web/Areas/Public/Views/Projects/Details.cshtml` - Added map container, DI, and initialization
   - Styles section: Added CSS for map styling
   - Scripts section: Added Google Maps API loading with configuration

## 🎯 Next Steps

After basic setup, consider:

1. **Add clustering** for multiple project markers on overview pages
2. **Implement geocoding** to convert addresses to coordinates
3. **Add route planning** between multiple projects
4. **Create heatmaps** showing project density
5. **Add Street View** integration for project locations
6. **Implement geofencing** for location-based notifications

---

**Last Updated**: January 2025
**Version**: 1.0.0