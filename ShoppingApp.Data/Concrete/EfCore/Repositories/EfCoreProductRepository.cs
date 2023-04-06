using Microsoft.EntityFrameworkCore;
using ShoppingApp.Data.Abstract;
using ShoppingApp.Data.Concrete.EfCore.Contexts;
using ShoppingApp.Entity.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoppingApp.Data.Concrete.EfCore.Repositories
{
    public class EfCoreProductRepository : EfCoreGenericRepository<Product>, IProductRepository
    {
        public EfCoreProductRepository(ShopAppContext context):base(context)
        {

        }
        private ShopAppContext ShopAppContext
        {
            get { return _context as ShopAppContext;  }
        }

        public async Task CreateProductAsync(Product product, int[] selectedCategoryIds)
        {
            //Önce product kaydı yapacağız. Böylece elimizde product Id olacak.
            //Sonra da bu product Id'yi kullanarak, productCategory kaydını/kayıtlarını yapacağız.
            await ShopAppContext.Products.AddAsync(product);
            await ShopAppContext.SaveChangesAsync();
            product.ProductCategories = selectedCategoryIds
                .Select(catId => new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = catId
                }).ToList();
            await ShopAppContext.SaveChangesAsync();
        }

        public async Task<List<Product>> GetHomePageProductsAsync()
        {
            return await ShopAppContext
                .Products
                .Where(p => p.IsHome && p.IsApproved)
                .ToListAsync();
        }

        public Task<Product> GetProductDetailsByUrlAsync(string productUrl)
        {
            return ShopAppContext
                .Products
                .Where(p => p.Url == productUrl)
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(string category)
        {
            var products= ShopAppContext.Products.Where(p => p.IsApproved).AsQueryable(); //onaylılar anasayfada görünsün!
            if (category != null)
            {
                products = products
                    .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                    .Where(p => p.ProductCategories.Any(pc => pc.Category.Url == category));
            }
            return await products.ToListAsync();


        }

        public async Task<List<Product>> GetProductsWithCategories()
        {
            return await ShopAppContext
                .Products
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .ToListAsync();
        }

        public async Task<Product> GetProductWithCategories(int id)
        {
            return await ShopAppContext
                .Products
                .Where(p => p.Id == id)
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateIsHomeAsync(Product product)
        {
            product.IsHome = !product.IsHome;
            ShopAppContext.Update(product);
            await ShopAppContext.SaveChangesAsync();
        }

        public async Task UpdateIsApprovedAsync(Product product)
        {
            product.IsApproved = !product.IsApproved;
            ShopAppContext.Update(product);
            await ShopAppContext.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(Product product, int[] selectedCategoryIds)
        {
            Product newProduct = await ShopAppContext
                .Products
                .Include(p=> p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == product.Id);
            newProduct.ProductCategories = selectedCategoryIds
                .Select(catId => new ProductCategory
                {
                    ProductId = newProduct.Id,
                    CategoryId = catId
                }).ToList();
            ShopAppContext.Update(newProduct);
            await ShopAppContext.SaveChangesAsync();
            
        }

        //public async Task<List<Product>> GetSearchResultsAsync(string searchString, bool isApproved = true)
        //{
        //    searchString = searchString.ToLower();
        //    var result = ShopAppContext.Products.AsQueryable();
        //    if (isApproved)
        //    {
        //        return await result.Where(p => p.IsApproved && (p.Name.ToLower().Contains(searchString) || p.Description.ToLower().Contains(searchString))).ToListAsync(); //isim veya descr aradığım sözcük geçiyorsa VE ONAYLIYSA
        //    }
        //    return await result.Where(p=>p.Name.ToLower().Contains(searchString) || p.Description.ToLower().Contains(searchString)).ToListAsync();

        //}

        public async Task<List<Product>> GetSearchResultsAsync(bool? isApproved, bool? isHome, string searchString)
        {
            var result = ShopAppContext.Products.AsQueryable(); //sorgulanabilir
            if (isApproved != null)
            {
                result = result.Where(p => p.IsApproved == isApproved).AsQueryable();
            }
            if (isHome !=null )
            {
                result = result.Where(p => p.IsHome == isHome).AsQueryable();
            }
            if (!String.IsNullOrEmpty(searchString))
            {
                result = result.Where(p => p.Name.ToLower().Contains(searchString.ToLower()) || p.Description.ToLower().Contains(searchString.ToLower())).AsQueryable();
            }
            return await result.ToListAsync();
        }
    }
}
