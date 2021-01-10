using System.ComponentModel.DataAnnotations.Schema;

namespace EPS.Data
{
    public class Product
    {
        public int ID { get; set; }
        public int Title { get; set; }
        public string ProducCode { get; set; }
        [ForeignKey("CardCode")]
        public int CardCodeID { get; set; }
        public CardCode CardCode { get; set; }
    }
}
