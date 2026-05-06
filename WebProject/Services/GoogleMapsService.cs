using System.Text.Json;

namespace WebProject.Services
{
    public class GoogleMapsService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public GoogleMapsService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task<double?> GetDistanceInKmAsync(
            double originLat,
            double originLng,
            double destinationLat,
            double destinationLng)
        {
            var apiKey = _configuration["GoogleMaps:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_GOOGLE_MAPS_API_KEY")
            {
                return null;
            }

            var url =
                $"https://maps.googleapis.com/maps/api/distancematrix/json" +
                $"?origins={originLat},{originLng}" +
                $"&destinations={destinationLat},{destinationLng}" +
                $"&units=metric" +
                $"&key={apiKey}";

            var response = await _httpClient.GetStringAsync(url);

            using var document = JsonDocument.Parse(response);

            var root = document.RootElement;

            var status = root.GetProperty("status").GetString();

            if (status != "OK")
            {
                return null;
            }

            var element = root
                .GetProperty("rows")[0]
                .GetProperty("elements")[0];

            var elementStatus = element.GetProperty("status").GetString();

            if (elementStatus != "OK")
            {
                return null;
            }

            var meters = element
                .GetProperty("distance")
                .GetProperty("value")
                .GetDouble();

            return meters / 1000.0;
        }
    }
}