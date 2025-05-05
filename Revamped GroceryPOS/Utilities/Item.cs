using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Revamped_GroceryPOS.Utilities
{
    class Item
    {
        public Image? Image { get; set; }

        public required string Name { get; set; }

        public required double Price { get; set; }

        public string? Category { get; set; }

        public string? Description { get; set; }

        public string? SoldBy { get; set; }

        public required int Stock { get; set; }
    }
}
