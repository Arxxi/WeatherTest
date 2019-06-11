using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WeatherMVC_TEST.Models;

namespace WeatherMVC_TEST.Controllers
{
    public class HomeController : Controller
    {
        HttpClient client = new HttpClient();

        [HttpGet]
        public ActionResult Index()
        {
            if(HttpContext.Request.Cookies["Last_city"] != null)
            {
                string cityCookie = HttpContext.Request.Cookies["Last_city"].Value;
                ViewData["Last_city"] = cityCookie;
            }
            
            return View();
        }

        
        [HttpPost]
        public ActionResult searchCity()
        { 
            string cityName = HttpContext.Request.Form["cityName"];
            string url = string.Format("http://api.openweathermap.org/data/2.5/weather?q={0}&APPID={1}", cityName, "08bd2b65af4f2c9ff3fc2789493d8944");
            WeatherInfo weatherinfo = null;

            using (WebClient web = new WebClient())
            {
               
                try
                {
                    string data = web.DownloadString(url);
                    weatherinfo = (new JavaScriptSerializer()).Deserialize<WeatherInfo>(data);
                    weatherinfo.main.temp_min = Math.Round(KelvinCensius(weatherinfo.main.temp_min, true), 0);
                    weatherinfo.main.temp_max = Math.Round(KelvinCensius(weatherinfo.main.temp_max, true), 0);

                }
                catch (WebException ex)
                {
                    HttpWebResponse errorResponse = (HttpWebResponse)ex.Response;
                    if (errorResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        return View("Error");
                    }
                }
            }

            DateTime now = DateTime.Now;
            HttpCookie lastCity = new HttpCookie("Last_city", cityName);
            lastCity.Expires = now.AddDays(14);
            HttpContext.Response.SetCookie(lastCity);

            ViewData["Last_city"] = cityName;
            if(HttpContext.Request.Cookies["Warning_message_required"] == null)
            {
                foreach (var item in weatherinfo.weather)
                {
                    if (item.main == "Rain")
                    {
                        HttpCookie warningMessageRequired = new HttpCookie("Warning_message_required");
                        warningMessageRequired.Expires = now.AddDays(1);
                        HttpContext.Response.SetCookie(warningMessageRequired);
                        ViewData["Warning_message_required"] = "Today is rainy :(";
                    }
                }
            }

            return View(weatherinfo);
        }

        private static double KelvinCensius(double d, bool kelvin = false)
        {
            return kelvin ? (d - 273.15) : (d + 273.15);
        }
    }
}