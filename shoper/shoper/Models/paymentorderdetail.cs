namespace shoper.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class paymentorderdetail
    {
        [Key]
        public int OrderDetailID { get; set; }

        public int? OrderID { get; set; }

        public int? ProductID { get; set; }

        public int? Quantity { get; set; }

        public decimal? UnitPrice { get; set; }

        public int? CustomerID { get; set; }

        public virtual Product Product { get; set; }
    }
}
