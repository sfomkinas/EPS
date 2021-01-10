using System;
using System.Collections.Generic;

namespace EPS.Data
{
    public class CardCode
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Used { get; set; }

        public List<Product> Products { get; set; }
    }
}
