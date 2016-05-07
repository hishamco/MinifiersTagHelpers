using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MinifiersTagHelpers.TagHelpers
{
    public static class CssMinifier
    {
        private const string URL_CSS_MINIFIER = "https://cssminifier.com/raw";
        private const string POST_PAREMETER_NAME = "input";

        public static async Task<String> MinifyCss(string inputCss)
        {
            List<KeyValuePair<String, String>> contentData = new List<KeyValuePair<String, String>>
            {
                new KeyValuePair<String, String>(POST_PAREMETER_NAME, inputCss)
            };

            using (HttpClient httpClient = new HttpClient())
            {
                using (FormUrlEncodedContent content = new FormUrlEncodedContent(contentData))
                {
                    using (HttpResponseMessage response = await httpClient.PostAsync(URL_CSS_MINIFIER, content))
                    {
                        response.EnsureSuccessStatusCode();
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
        }
    }
}