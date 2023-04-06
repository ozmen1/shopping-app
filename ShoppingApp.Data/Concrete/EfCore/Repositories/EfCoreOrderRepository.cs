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
    public class EfCoreOrderRepository : EfCoreGenericRepository<Order>, IOrderRepository
    {
        public EfCoreOrderRepository(ShopAppContext context) : base(context)
        {

        }
        private ShopAppContext ShopAppContext
        {
            get { return _context as ShopAppContext; }
        }
        public async Task<List<Order>> GetOrders(string userId = null)
        {
            #region userid-null-kontrolu-yapilmadan
            //var orders = ShopAppContext.Orders
            //    .Where(o => o.UserId == userId)
            //    .Include(o => o.OrderItems)
            //    .ThenInclude(od => od.Product)
            //    .ToList();
            #endregion

            #region userid-null-kontrolu-yaparak
            var orders = ShopAppContext.Orders
                .Include(o => o.OrderItems) //order.cs'deki orderitems'ı doldur getir
                .ThenInclude(oi => oi.Product) //orderitems'dan product'a geçip ürün bilgilerini getir.
                .AsQueryable(); //sorguyu şuan çalıştırma dursun, kontrolden sonra çalıştır.

            if (!String.IsNullOrEmpty(userId))
            {
                orders = orders.Where(o => o.UserId == userId);
            }
            return orders.ToList();
            #endregion

        }
    }
}
