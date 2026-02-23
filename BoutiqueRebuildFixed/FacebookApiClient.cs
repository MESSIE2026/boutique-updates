using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq; // ✅ Installe "Newtonsoft.Json" via NuGet


namespace BoutiqueRebuildFixed
{
    public class FacebookApiClient
    {
        private readonly string _accessToken;
        private readonly string _graphVersion;

        public FacebookApiClient(string accessToken, string graphVersion)
        {
            _accessToken = accessToken ?? "";
            _graphVersion = string.IsNullOrWhiteSpace(graphVersion) ? "v19.0" : graphVersion.Trim();
        }

        // ✅ Test simple : /me/adaccounts
        public async Task<string> GetMyAdAccountsRawAsync()
        {
            // Ici pas besoin de BuildUrl : on compose directement
            string url = $"https://graph.facebook.com/{_graphVersion}/me/adaccounts" +
                         $"?fields=id,name,account_id" +
                         $"&access_token={Uri.EscapeDataString(_accessToken)}";

            return await HttpGetAsync(url);
        }

        // ✅ Insights par CampaignId
        public async Task<FacebookInsights> GetCampaignInsightsAsync(string campaignId, DateTime? since, DateTime? until)
        {
            if (string.IsNullOrWhiteSpace(campaignId))
                throw new ArgumentException("campaignId vide");

            string fields = "impressions,reach,clicks,spend";

            string url = $"https://graph.facebook.com/{_graphVersion}/{Uri.EscapeDataString(campaignId)}/insights" +
                         $"?fields={Uri.EscapeDataString(fields)}" +
                         $"&access_token={Uri.EscapeDataString(_accessToken)}";

            // time_range optionnel
            if (since.HasValue && until.HasValue)
            {
                string timeRangeJson = $"{{\"since\":\"{since.Value:yyyy-MM-dd}\",\"until\":\"{until.Value:yyyy-MM-dd}\"}}";
                url += $"&time_range={Uri.EscapeDataString(timeRangeJson)}";
            }

            string json = await HttpGetAsync(url);

            var jo = JObject.Parse(json);
            var data = jo["data"] as JArray;

            if (data == null || data.Count == 0)
            {
                return new FacebookInsights { Impressions = 0, Reach = 0, Clicks = 0, Spend = 0m };
            }

            var first = (JObject)data[0];

            return new FacebookInsights
            {
                Impressions = ReadInt(first, "impressions"),
                Reach = ReadInt(first, "reach"),
                Clicks = ReadInt(first, "clicks"),
                Spend = ReadDecimal(first, "spend")
            };
        }

        private async Task<string> HttpGetAsync(string url)
        {
            using (var http = new HttpClient())
            {
                http.Timeout = TimeSpan.FromSeconds(30);

                var resp = await http.GetAsync(url).ConfigureAwait(false);
                string body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!resp.IsSuccessStatusCode)
                    throw new Exception("Facebook API Error: " + body);

                return body;
            }
        }

        private static int ReadInt(JObject obj, string key)
        {
            var t = obj[key];
            if (t == null) return 0;

            int v;
            return int.TryParse(t.ToString(), out v) ? v : 0;
        }

        private static decimal ReadDecimal(JObject obj, string key)
        {
            var t = obj[key];
            if (t == null) return 0m;

            decimal v;
            return decimal.TryParse(t.ToString(),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out v) ? v : 0m;
        }
    }

    public class FacebookInsights
    {
        public int Impressions { get; set; }
        public int Reach { get; set; }
        public int Clicks { get; set; }
        public decimal Spend { get; set; }
    }
}