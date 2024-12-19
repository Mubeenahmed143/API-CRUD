using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using simple.Models;
using System.Net;
using System.Text.Json.Serialization;

namespace simple.Controllers
{
    public class Fetch : Controller
    {

        //Fetch section
        public IActionResult Index()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            

            string url = "https://localhost:44354/api/Products";

            using (WebClient web = new WebClient())
            {
                string jsonstr = web.DownloadString(url);

                List<Products> result = JsonConvert.DeserializeObject<List<Products>>(jsonstr);

                return View(result);
            }

           
        }

        //Create section

        [HttpGet]
        public IActionResult Create() {

            return View();
        }

        [HttpPost]
        public IActionResult Create(Products products, IFormFile Image)
        {
            if (Image != null && Image.Length > 0)
            {
                var imageName = Path.GetFileName(Image.FileName);
                string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/rrr/");

                // Ensure the directory exists
                if (!Directory.Exists(imagePath))
                {
                    Directory.CreateDirectory(imagePath);
                }

                string imageValue = Path.Combine(imagePath, imageName);
                using (var stream = new FileStream(imageValue, FileMode.Create))
                {
                    Image.CopyTo(stream);
                }

                // Save the relative path to the database
                products.Image = Path.Combine("/rrr/", imageName);
            }

            using (var webClient = new WebClient())
            {
                string json = JsonConvert.SerializeObject(products);

                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";

                string response = webClient.UploadString("https://localhost:44354/api/Products", "POST", json);

                return RedirectToAction("Create");
            }
        }

        //delete section
        public IActionResult Delete(int Id) {

            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                    webClient.Headers[HttpRequestHeader.Accept] = "application/json";

                    var response = webClient.UploadData($"{"https://localhost:44354/api/Products"}/{Id}", "DELETE", new byte[0]);

                    if(response.Length == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return View("Error");
                    }
                }
            }
            catch(WebException ex)
            {
                return View("Error");
            }
        }

        //update section

        [HttpGet]
        public IActionResult Edit(int Id)
        {
            using (var webClient = new WebClient())
            {
                var json = webClient.DownloadString($"{"https://localhost:44354/api/Products"}/{Id}");

                var product = JsonConvert.DeserializeObject<Products>(json);

                return View(product);
            }
        }

        [HttpPost]

        public IActionResult Edit(Products product, IFormFile image)
        {
            // Handle the image upload
            if (image != null && image.Length > 0)
            {
                // Define the image directory path
                string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/rrr/");

                // Ensure the directory exists
                if (!Directory.Exists(imagePath))
                {
                    Directory.CreateDirectory(imagePath);
                }

                // Create a unique filename for the image
                string uniqueImageName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                string fullImagePath = Path.Combine(imagePath, uniqueImageName);

                // Save the image to the server
                using (var stream = new FileStream(fullImagePath, FileMode.Create))
                {
                    image.CopyTo(stream);
                }

                // Update the image path in the product object
                product.Image = Path.Combine("/image/", uniqueImageName);
            }

            using (var webClient = new WebClient())
            {
                string json = JsonConvert.SerializeObject(product);

                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";

                string response = webClient.UploadString("https://localhost:44354/api/Products" + $"/{product.Id}", "PUT", json);

                return RedirectToAction("Index");

            }
        }
    }
}
