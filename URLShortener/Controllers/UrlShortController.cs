using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using URLShortener.Models;
using User = URLShortener.Models.User;

namespace URLShortener.Controllers
{
    public class UrlShortController : Controller
    {
        private const string CosmosDbConnectionString = "AccountEndpoint=https://short-url-jehoshua.documents.azure.com:443/;AccountKey=fvOAh5PGrJACvgAfUGrKwy0TTO0a1CbbAo5qL2aLZT14RzgSXTyzCuaVFereGSuRVeKinVL22CYyACDbnEiUSQ==;";
        private const string DatabaseId = "ShortUrl";
        private const string ContainerId_Link = "short-url-main";
        private const string ContainerId_Customer = "short-customer";

        private readonly CosmosDbRepository<LinkModel> _repositoryLinks;
        private readonly CosmosDbRepository<User> _repositoryCustomer;

        public UrlShortController()
        {
            _repositoryLinks = new CosmosDbRepository<LinkModel>(CosmosDbConnectionString, DatabaseId, ContainerId_Link);
            _repositoryCustomer = new CosmosDbRepository<User>(CosmosDbConnectionString, DatabaseId, ContainerId_Customer);
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.UserId = CommonVar.UserId;
            ViewBag.Profile = CommonVar.UserName != null ? CommonVar.UserName.ToUpper() : null;
            ViewBag.Message = CommonVar.Message != null ? CommonVar.Message : "";
            CommonVar.Message = null;
            ViewBag.ShortenedUrl = CommonVar.ShortenLink != null ? CommonVar.ShortenLink : "";
            CommonVar.ShortenLink = null;
            List<List<string>> dataHistory = new List<List<string>>();
            if (CommonVar.UserName != null)
            {
                var shortUrlExists = await _repositoryLinks.GetItemsAsync($"SELECT * FROM c where c.usedId = {CommonVar.UserId}");
                foreach(var url in shortUrlExists)
                {
                    List<string> existedURL = new List<string>
                    {
                        url.ShortKey,
                        url.OriginalUrl,
                        url.ExpiryDate == null ? "inf" : url.ExpiryDate.ToString(),
                        url.HitCount == null ? "0" : url.HitCount.ToString()
                    };
                    dataHistory.Add(existedURL);
                }
                ViewBag.DataHistory = dataHistory;
            }
            else
            {
                ViewBag.DataHistory = dataHistory;
            }
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ShortenLink(string originalUrl, string customUrl, DateTime? expiryDate)
        {
            int? userId = null;
            if(CommonVar.UserId != null)
            {
                userId = CommonVar.UserId;
            }

            if (!string.IsNullOrWhiteSpace(originalUrl))
            {
                string shortKey = Guid.NewGuid().ToString("N").Substring(0, 8);
                string shortUrl = $"{shortKey}";

                if(customUrl != "" && customUrl != null)
                {
                    shortUrl = customUrl;
                }

                var shortUrlExists = await _repositoryLinks.GetItemsAsync($"SELECT * FROM c where c.shortKey = '{shortUrl}'");
                if(shortUrlExists.Count() != 0)
                {
                    CommonVar.Message = "URL is Existed!";
                    return RedirectToAction("Index");
                }

                if (expiryDate == null)
                {
                    expiryDate = DateTime.MaxValue;
                }

                var links = await _repositoryLinks.GetItemsAsync($"SELECT * FROM c");

                var linkItem = new LinkModel
                {
                    Id = (links.Count() + 1).ToString(),
                    ShortKey = shortUrl,
                    OriginalUrl = originalUrl,
                    ExpiryDate = expiryDate,
                    UserId = userId,
                    HitCount = 0
                };

                await _repositoryLinks.CreateItemAsync(linkItem);

                CommonVar.ShortenLink = shortUrl;
            }

            return RedirectToAction("Index");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password)
        {
            if (!string.IsNullOrWhiteSpace(username))
            {
                var cstm = await _repositoryCustomer.GetItemsAsync($"SELECT * FROM c WHERE c.userName ='{username}'");
                if(cstm.Count() != 0)
                {
                    ViewBag.Message = "Account is Existed!";
                    return View();
                }
                else
                {
                    cstm = await _repositoryCustomer.GetItemsAsync($"SELECT * FROM c");
                    var newCustomer = new User
                    {
                        Id = (cstm.Count() + 1).ToString(),
                        UserName = username,
                        Password = password
                    };

                    await _repositoryCustomer.CreateItemAsync(newCustomer);
                }

                ViewBag.Message = "Success";
                return RedirectToAction("Login");
            }

            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (!string.IsNullOrWhiteSpace(username))
            {
                var cstm = await _repositoryCustomer.GetItemsAsync($"SELECT * FROM c WHERE c.userName ='{username}'");
                if (cstm.Count() == 0)
                {
                    ViewBag.Message = "Account is not Exist!";
                    return View();
                }
                else
                {
                    if(cstm.First().Password == password)
                    {
                        ViewBag.Message = "Success";
                        CommonVar.UserId = int.Parse(cstm.First().Id);
                        CommonVar.UserName = cstm.First().UserName;
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewBag.Message = "Wrong Password!";
                        return View();
                    }
                }
            }

            return View();
        }

        public IActionResult Logout()
        {
            CommonVar.UserId = null;
            CommonVar.UserName = null;
            return RedirectToAction("Login");
        }

        [Route("/{shortKey}")]
        public async Task<IActionResult> RedirectToOriginalUrl(string shortKey)
        {
            var links = await _repositoryLinks.GetItemsAsync($"SELECT * FROM c WHERE c.shortKey='{shortKey}'");
            var linkItem = links.FirstOrDefault();

            if (linkItem != null && (!linkItem.ExpiryDate.HasValue || linkItem.ExpiryDate >= DateTime.UtcNow))
            {
                linkItem.HitCount = linkItem.HitCount + 1;
                var res = _repositoryLinks.UpdateItemAsync(linkItem);
                return Redirect(linkItem.OriginalUrl);
            }

            return NotFound();
        }
    }
}

