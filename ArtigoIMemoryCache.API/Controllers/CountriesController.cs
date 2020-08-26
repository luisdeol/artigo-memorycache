using ArtigoIMemoryCache.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArtigoIMemoryCache.API.Controllers
{
    [Route("api/[controller]")]
    public class CountriesController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private const string COUNTRIES_KEY = "Countries";

        public CountriesController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        [HttpGet]
        public async Task<IActionResult> GetCountries()
        {

            if (_memoryCache.TryGetValue(COUNTRIES_KEY, out object countriesObject))
            {
                return Ok(countriesObject);
            } else
            {
                const string restCountriesUrl = "https://restcountries.eu/rest/v2/all";

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(restCountriesUrl);

                    var responseData = await response.Content.ReadAsStringAsync();

                    var countries = JsonConvert.DeserializeObject<List<Country>>(responseData);

                    var memoryCacheEntryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3600),
                        SlidingExpiration = TimeSpan.FromSeconds(1200)
                    };

                    _memoryCache.Set(COUNTRIES_KEY, countries, memoryCacheEntryOptions);

                    return Ok(countries);
                }
            }
            
        }
    }
}
