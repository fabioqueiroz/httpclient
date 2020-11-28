using Movies.API.InternalModels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Movies.Client.Services
{
    public class CancellationService : IIntegrationService
    {
        private static HttpClient _httpClient = new HttpClient(
            new HttpClientHandler()
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip
            });
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public CancellationService()
        {
            _httpClient.BaseAddress = new Uri("http://localhost:57863");
            _httpClient.Timeout = new TimeSpan(0, 0, 30);
            _httpClient.DefaultRequestHeaders.Clear();           
        }
        public async Task Run()
        {
            _cancellationTokenSource.CancelAfter(2000);
            await GetTrailerAndCancel(_cancellationTokenSource);
        }

        public async Task GetTrailerAndCancel(CancellationTokenSource cancellationTokenSource)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b/trailers/{Guid.NewGuid()}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            //var cancellationTokenSource = new CancellationTokenSource();
            //cancellationTokenSource.CancelAfter(2000);

            try
            {
                using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationTokenSource.Token))
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    response.EnsureSuccessStatusCode();

                    var trailer = stream.ReadAndDeserializeFromJson<Trailer>();
                }
            }
            catch (OperationCanceledException ocEx)
            {

                throw new Exception(ocEx.Message);
            }
        }
    }
}
