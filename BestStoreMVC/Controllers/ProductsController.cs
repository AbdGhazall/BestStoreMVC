using System.Linq;
using System.Runtime.Intrinsics.Arm;
using BestStoreMVC.Models;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace BestStoreMVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext context; //TO READ FROM DATABASE
        private readonly IWebHostEnvironment environment; // FOR IMAGE STORING TO DATABASE

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }
        public IActionResult Index() //READ
        {
            var products = context.Products.OrderByDescending(p => p.Id).ToList();
            return View(products);
        }
        [HttpGet]
        public IActionResult Index(string searchQuery) //SEARCH
        {
            var products = context.Products.AsQueryable(); // Start with all products

            if (!string.IsNullOrEmpty(searchQuery))
            {
                products = products.Where(p => p.Name.Contains(searchQuery)); // Filter by product name
            }

            var productList = products.OrderByDescending(p => p.Id).ToList();
            ViewData["SearchQuery"] = searchQuery; // Pass search query back to the view for display

            return View(productList);
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost] //FOR SUBMIT CREATED DATA
        public IActionResult Create(ProductDto productDto) //IN CREATE IMAGE ARE REQUIRED
        {
            if (productDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "The image file is required"); //WE MAKE IT HERE BECAUSE IN UPDATE IMAGE ISN"T REQUIERD
            }
            if (!ModelState.IsValid)
            {
                return View(productDto); //IF MODEL NOT CORRECT 
            }

            // save the image file
            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            newFileName += Path.GetExtension(productDto.ImageFile!.FileName);
            string imageFullPath = environment.WebRootPath + "/products/" + newFileName;
            using (var stream = System.IO.File.Create(imageFullPath))
            {
                productDto.ImageFile.CopyTo(stream);
            }

            // save the new product in the database
            Product product = new Product()
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Category = productDto.Category,
                Price = productDto.Price,
                Descrption = productDto.Descrption,
                ImageFilename = newFileName,
                CreatedAt = DateTime.Now,
            };
            context.Products.Add(product);
            context.SaveChanges();

            return RedirectToAction("Index", "Products"); //IF MODEL CORRECT
        }

        public IActionResult Edit(int id)
        {
            Product product = context.Products.Find(id);

            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }


            // create productDto from product
            var productDto = new ProductDto()
            {

                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Descrption = product.Descrption
            };

            ViewData["ProductId"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFilename;
            ViewData["CreatedAt"] = product.CreatedAt;

            return View(productDto);
        }

        [HttpPost]
        public IActionResult Edit(int id, ProductDto productDto)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }
            if (!ModelState.IsValid)
            {
                ViewData["ProductId"] = product.Id;
                ViewData["ImageFileName"] = product.ImageFilename;
                ViewData["CreatedAt"] = product.CreatedAt;
                return View(productDto);
            }

            // update the image file if we have a new image file
            string newFileName = product.ImageFilename;
            if (productDto.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                newFileName += Path.GetExtension(productDto.ImageFile.FileName);
                string imageFullPath = environment.WebRootPath + "/products/" + newFileName;
                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    productDto.ImageFile.CopyTo(stream);
                }
            }
            // delete the old image
            string oldImageFullPath = environment.WebRootPath + "/products/" + product.ImageFilename;
            System.IO.File.Delete(oldImageFullPath);


            // update the product in the database
            product.Name = productDto.Name;
            product.Brand = productDto.Brand;
            product.Category = productDto.Category;
            product.Price = productDto.Price;
            product.Descrption = productDto.Descrption;
            product.ImageFilename = newFileName;

            context.SaveChanges();

            return RedirectToAction("Index", "Products");

        }

        public IActionResult Delete(int id)
        {

            var product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }
            string imageFullPath = environment.WebRootPath + "/products/" + product.ImageFilename;
            System.IO.File.Delete(imageFullPath);

            context.Products.Remove(product);
            context.SaveChanges(true);

            return RedirectToAction("Index", "Products");

        }
    }
}
