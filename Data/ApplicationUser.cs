using Microsoft.AspNetCore.Identity;

namespace FPTBook_v3.Data
{
    public class ApplicationUser: IdentityUser
    {
        public string? User_Name { get; set; }
        public string? User_Img { get; set; }

        public virtual ICollection<ShoppingCart>? ShoppingCarts { get; set; }
        public virtual ICollection<Order>? Orders { get; set; }
    }
}
