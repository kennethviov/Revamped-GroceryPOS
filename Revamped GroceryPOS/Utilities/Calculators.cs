using Revamped_GroceryPOS.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revamped_GroceryPOS.Utilities
{
    public class Calculators
    {

        public static double CalculateSubtotal(List<CartItem> cartItems)
        {
            double subtotal = 0;

            foreach (var cartItem in cartItems)
            {
                subtotal += (cartItem.Price * cartItem.Quantity);
            }

            return subtotal;
        }

        public static int DetermineDiscountP(double subtotal)
        {
            if (subtotal >= 500)
                return 20;
            else if (subtotal >= 200)
                return 15;
            else if (subtotal >= 100)
                return 10;
            else
                return 0;
        }

        public static double CalculateDiscount(double subtotal, int discountP)
        {
            return subtotal * discountP / 100;
        }

        public static double CalculateTotal(double subtotal, double discount)
        {
            return subtotal - discount;
        }
    }
}
