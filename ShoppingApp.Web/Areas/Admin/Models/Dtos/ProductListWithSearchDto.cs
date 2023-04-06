using Microsoft.AspNetCore.Mvc.Rendering;

namespace ShoppingApp.Web.Areas.Admin.Models.Dtos
{
    public class ProductListWithSearchDto
    {
        public List<ProductListDto> productListDtos { get; set; }
        public SearchQueryDto SearchQueryDto { get; set; }
        public List<SelectListItem> IsHomeList { get; set; }
        public List<SelectListItem> IsApprovedList { get; set; }

    }
}
