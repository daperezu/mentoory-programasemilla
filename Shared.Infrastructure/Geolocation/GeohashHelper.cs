namespace LinaSys.Shared.Infrastructure.Geolocation;

/// <summary>
/// Helper class for encoding and decoding geohash strings.
/// Based on the geohash algorithm by Gustavo Niemeyer.
/// </summary>
public static class GeohashHelper
{
    private const string Base32 = "0123456789bcdefghjkmnpqrstuvwxyz";
    private static readonly Dictionary<char, string[]> Neighbors = new()
    {
        { 'p', new[] { "p0r21436x8zb9dcf5h7kjnmqesgutwvy", "bc01fg45238967deuvhjyznpkmstqrwx" } },
        { 'q', new[] { "14365h7k9dcfesgujnmqp0r2twvyx8zb", "238967debc01fg45kmstqrwxuvhjyznp" } }
    };
    private static readonly Dictionary<char, string[]> Borders = new()
    {
        { 'p', new[] { "prxz", "028b" } },
        { 'q', new[] { "028b", "prxz" } }
    };

    /// <summary>
    /// Encodes latitude and longitude into a geohash string.
    /// </summary>
    /// <returns></returns>
    public static string Encode(double latitude, double longitude, int precision = 12)
    {
        if (precision < 1 || precision > 12)
        {
            throw new ArgumentException("Precision must be between 1 and 12", nameof(precision));
        }

        double[] latRange = { -90.0, 90.0 };
        double[] lonRange = { -180.0, 180.0 };
        string geohash = string.Empty;
        int bits = 0;
        int bit = 0;
        int ch = 0;
        bool even = true;

        while (geohash.Length < precision)
        {
            if (even)
            {
                double mid = (lonRange[0] + lonRange[1]) / 2;
                if (longitude > mid)
                {
                    ch |= 1 << (4 - bit);
                    lonRange[0] = mid;
                }
                else
                {
                    lonRange[1] = mid;
                }
            }
            else
            {
                double mid = (latRange[0] + latRange[1]) / 2;
                if (latitude > mid)
                {
                    ch |= 1 << (4 - bit);
                    latRange[0] = mid;
                }
                else
                {
                    latRange[1] = mid;
                }
            }

            even = !even;

            if (bit < 4)
            {
                bit++;
            }
            else
            {
                geohash += Base32[ch];
                bits++;
                bit = 0;
                ch = 0;
            }
        }

        return geohash;
    }

    /// <summary>
    /// Decodes a geohash string into latitude and longitude bounds.
    /// </summary>
    /// <returns></returns>
    public static (double MinLat, double MaxLat, double MinLon, double MaxLon) DecodeBounds(string geohash)
    {
        double[] latRange = { -90.0, 90.0 };
        double[] lonRange = { -180.0, 180.0 };
        bool even = true;

        foreach (char c in geohash.ToLowerInvariant())
        {
            int idx = Base32.IndexOf(c);
            if (idx == -1)
            {
                throw new ArgumentException($"Invalid geohash character: {c}", nameof(geohash));
            }

            for (int i = 4; i >= 0; i--)
            {
                int bit = (idx >> i) & 1;
                if (even)
                {
                    double mid = (lonRange[0] + lonRange[1]) / 2;
                    if (bit == 1)
                    {
                        lonRange[0] = mid;
                    }
                    else
                    {
                        lonRange[1] = mid;
                    }
                }
                else
                {
                    double mid = (latRange[0] + latRange[1]) / 2;
                    if (bit == 1)
                    {
                        latRange[0] = mid;
                    }
                    else
                    {
                        latRange[1] = mid;
                    }
                }

                even = !even;
            }
        }

        return (latRange[0], latRange[1], lonRange[0], lonRange[1]);
    }

    /// <summary>
    /// Gets the center point of a geohash.
    /// </summary>
    /// <returns></returns>
    public static (double Latitude, double Longitude) DecodeCenter(string geohash)
    {
        var bounds = DecodeBounds(geohash);
        return ((bounds.MinLat + bounds.MaxLat) / 2, (bounds.MinLon + bounds.MaxLon) / 2);
    }

    /// <summary>
    /// Gets all 8 neighboring geohashes for a given geohash.
    /// </summary>
    /// <returns></returns>
    public static string[] GetNeighbors(string geohash)
    {
        if (string.IsNullOrEmpty(geohash))
        {
            throw new ArgumentException("Geohash cannot be null or empty", nameof(geohash));
        }

        string[] neighbors = new string[8];

        // Get the 4 direct neighbors
        neighbors[0] = GetNeighbor(geohash, 'n'); // North
        neighbors[1] = GetNeighbor(geohash, 's'); // South
        neighbors[2] = GetNeighbor(geohash, 'e'); // East
        neighbors[3] = GetNeighbor(geohash, 'w'); // West

        // Get the 4 diagonal neighbors
        neighbors[4] = GetNeighbor(neighbors[0], 'e'); // Northeast
        neighbors[5] = GetNeighbor(neighbors[0], 'w'); // Northwest
        neighbors[6] = GetNeighbor(neighbors[1], 'e'); // Southeast
        neighbors[7] = GetNeighbor(neighbors[1], 'w'); // Southwest

        return neighbors;
    }

    /// <summary>
    /// Gets geohashes covering a bounding box at a specific precision.
    /// </summary>
    /// <returns></returns>
    public static HashSet<string> GetGeohashesInBoundingBox(
        double minLat, double maxLat, double minLon, double maxLon, int precision)
    {
        var geohashes = new HashSet<string>();

        // Get the geohash for each corner
        string bottomLeft = Encode(minLat, minLon, precision);
        string topRight = Encode(maxLat, maxLon, precision);

        // If they're the same, we only need one geohash
        if (bottomLeft == topRight)
        {
            geohashes.Add(bottomLeft);
            return geohashes;
        }

        // Otherwise, we need to fill in the grid
        var bounds1 = DecodeBounds(bottomLeft);
        var bounds2 = DecodeBounds(topRight);

        double latStep = bounds1.MaxLat - bounds1.MinLat;
        double lonStep = bounds1.MaxLon - bounds1.MinLon;

        for (double lat = minLat; lat <= maxLat; lat += latStep)
        {
            for (double lon = minLon; lon <= maxLon; lon += lonStep)
            {
                geohashes.Add(Encode(lat, lon, precision));
            }
        }

        return geohashes;
    }

    /// <summary>
    /// Calculates a bounding box for a given center point and radius.
    /// </summary>
    /// <returns></returns>
    public static (double MinLat, double MaxLat, double MinLon, double MaxLon) GetBoundingBox(
        double latitude, double longitude, double radiusKm)
    {
        const double EarthRadiusKm = 6371.0;

        // Calculate latitude bounds
        double latDelta = (radiusKm / EarthRadiusKm) * (180 / Math.PI);
        double minLat = latitude - latDelta;
        double maxLat = latitude + latDelta;

        // Calculate longitude bounds (accounting for latitude)
        double lonDelta = (radiusKm / (EarthRadiusKm * Math.Cos(latitude * Math.PI / 180))) * (180 / Math.PI);
        double minLon = longitude - lonDelta;
        double maxLon = longitude + lonDelta;

        // Clamp values to valid ranges
        minLat = Math.Max(-90, minLat);
        maxLat = Math.Min(90, maxLat);
        minLon = Math.Max(-180, minLon);
        maxLon = Math.Min(180, maxLon);

        return (minLat, maxLat, minLon, maxLon);
    }

    /// <summary>
    /// Gets a specific neighbor of a geohash.
    /// </summary>
    private static string GetNeighbor(string geohash, char direction)
    {
        geohash = geohash.ToLowerInvariant();
        char lastChar = geohash[^1];
        char type = (geohash.Length % 2 == 0) ? 'q' : 'p';
        string parent = geohash[..^1];

        // Determine the index based on direction
        int idx = direction switch
        {
            'n' => 0,
            's' => 1,
            'e' => (type == 'p') ? 1 : 0,
            'w' => (type == 'p') ? 0 : 1,
            _ => throw new ArgumentException($"Invalid direction: {direction}")
        };

        // Check if we're at a border
        if (!string.IsNullOrEmpty(parent) && Borders[type][idx].Contains(lastChar))
        {
            parent = GetNeighbor(parent, direction);
        }

        // Replace the last character
        int pos = Neighbors[type][idx].IndexOf(lastChar);
        if (pos == -1)
        {
            return geohash; // Return original if character not found
        }

        return parent + Base32[pos];
    }
}