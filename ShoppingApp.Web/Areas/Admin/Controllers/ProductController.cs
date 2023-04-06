using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShoppingApp.Business.Abstract;
using ShoppingApp.Business.Concrete;
using ShoppingApp.Core;
using ShoppingApp.Entity.Concrete;
using ShoppingApp.Web.Areas.Admin.Models.Dtos;
using ShoppingApp.Web.Models.Dtos;

namespace ShoppingApp.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }
        [NonAction]
        private List<SelectListItem> FillIsHomeList()
        {
            List<SelectListItem> isHomeList = new List<SelectListItem>();
            isHomeList.Add(new SelectListItem
            {
                Text = "Tümü",
                Value = "null"
            });
            isHomeList.Add(new SelectListItem
            {
                Text = "Anasayfa Ürünleri",
                Value = "true"
            });
            isHomeList.Add(new SelectListItem
            {
                Text = "Anasayfa Ürünü Olmayanlar",
                Value = "false"
            });
            return isHomeList;
        }
        [NonAction]
        private List<SelectListItem> FillIsApprovedList()
        {
            List<SelectListItem> isApprovedList = new List<SelectListItem>();
            isApprovedList.Add(new SelectListItem
            {
                Text = "Tümü",
                Value = "null"
            });
            isApprovedList.Add(new SelectListItem
            {
                Text = "Onaylı Ürünler",
                Value = "true"
            });
            isApprovedList.Add(new SelectListItem
            {
                Text = "Onaysız Ürünler",
                Value = "false"
            });
            return isApprovedList;
        }
        public async Task<IActionResult> Index(SearchQueryDto searchQueryDto = null)
        {
            var products = searchQueryDto != null ? await 
                _productService.GetSearchResultsAsync(searchQueryDto.IsApproved, searchQueryDto.IsHome, searchQueryDto.SearchString) : await 
                _productService.GetProductsWithCategories();
            var productListDtos = products
                .Select(p => new ProductListDto
                {
                    Product = p
                }).ToList();
            var isHomeList = FillIsHomeList();
            var isApprovedList = FillIsApprovedList();
            ViewBag.SelectedMenu = "Product";
            ViewBag.Title = "Ürünler";
            ProductListWithSearchDto productListWithSearchDto = new ProductListWithSearchDto
            {
                productListDtos = productListDtos,
                SearchQueryDto = searchQueryDto,
                IsHomeList = isHomeList,
                IsApprovedList = isApprovedList
            };
            return View(productListWithSearchDto);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryService.GetAllAsync();
            var productAddDto = new ProductAddDto
            {
                Categories = categories
            };
            return View(productAddDto);
        }
        [HttpPost]
        public async Task<IActionResult> Create(ProductAddDto productAddDto)
        {
            if (ModelState.IsValid)
            {
                var url = Jobs.InitUrl(productAddDto.Name);
                var product = new Product
                {
                    Name = productAddDto.Name,
                    Price = productAddDto.Price,
                    Description = productAddDto.Description,
                    Url = url,
                    IsApproved = productAddDto.IsApproved,
                    IsHome = productAddDto.IsHome,
                    ImageUrl = Jobs.UploadImage(productAddDto.ImageFile)
                };
                await _productService.CreateProductAsync(product, productAddDto.SelectedCategoryIds);
                return RedirectToAction("Index");
            }
            var categories = await _categoryService.GetAllAsync();
            productAddDto.Categories = categories;
            productAddDto.ImageUrl = productAddDto.ImageUrl;
            return View(productAddDto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            
            var product = await _productService.GetProductWithCategories(id);
            ProductUpdateDto productUpdateDto = new ProductUpdateDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                IsApproved = product.IsApproved,
                IsHome = product.IsHome,
                ImageUrl = product.ImageUrl,
                Categories = await _categoryService.GetAllAsync(),
                SelectedCategoryIds = product.ProductCategories.Select(pc => pc.CategoryId).ToArray()
            };
            return View(productUpdateDto);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(ProductUpdateDto productUpdateDto, int[] selectedCategoryIds)
        {
            if (ModelState.IsValid)
            {
                var product = await _productService.GetByIdAsync(productUpdateDto.Id);
                if (product==null)
                {
                    return NotFound();
                }
                var url= Jobs.InitUrl(productUpdateDto.Name);
                var imageUrl = productUpdateDto.ImageFile != null ? Jobs.UploadImage(productUpdateDto.ImageFile) : product.ImageUrl;
                product.Name= productUpdateDto.Name;
                product.Price= productUpdateDto.Price;
                product.Description= productUpdateDto.Description;
                product.IsApproved=productUpdateDto.IsApproved;
                product.IsHome = productUpdateDto.IsHome;
                product.ImageUrl = imageUrl;
                product.Url = url;
                await _productService.UpdateProductAsync(product, selectedCategoryIds);
                return RedirectToAction("Index");
            }
            var categories = await _categoryService.GetAllAsync();
            productUpdateDto.Categories = categories;
            
            return View(productUpdateDto);
        }
        public async Task<IActionResult> UpdateIsHome(int id, SearchQueryDto searchQueryDto)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product==null) { return NotFound();}
            await _productService.UpdateIsHomeAsync(product);
            return RedirectToAction("Index", searchQueryDto);
        }
        public async Task<IActionResult> UpdateIsApproved(int id, SearchQueryDto searchQueryDto)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) { return NotFound(); }
            await _productService.UpdateIsApprovedAsync(product);
            return RedirectToAction("Index", searchQueryDto);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product==null) { return NotFound();}
            _productService.Delete(product);
            return RedirectToAction("Index");
        }
        //public async Task<IActionResult> SearchProduct(SearchQueryDto seachQueryDto)
        //{
        //    List<Product> searchResults = await _productService.GetSearchResultsAsync(p=>p.IsApproved);
        //    List<ProductListDto> productDtos = new List<ProductListDto>();
        //    foreach (var product in searchResults)
        //    {
        //        productDtos.Add(new ProductListDto
        //        {
        //            Product = product
        //        });
        //    }
        //    ViewBag.SelectedMenu = "Product";
        //    ViewBag.Title = "Ürün Arama Sonuçları";
        //    return View("Index", productDtos);
        //}
    }
}
