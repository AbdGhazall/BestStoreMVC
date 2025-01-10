using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BestStoreMVC.Models
{
    public class Product
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; } = "";
        [MaxLength(100)]
        public string Brand { get; set; } = "";
        [MaxLength(100)]
        public string Category { get; set; } = "";

        [Precision(16,2)]
        public decimal Price { get; set; }
        [MaxLength(100)]
        public string Descrption { get; set; } = "";
        [MaxLength(100)]
        public string ImageFilename { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
