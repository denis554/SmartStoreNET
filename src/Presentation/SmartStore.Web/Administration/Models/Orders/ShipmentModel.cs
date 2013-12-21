﻿using System.Collections.Generic;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Orders
{
    public class ShipmentModel : EntityModelBase
    {
        public ShipmentModel()
        {
            this.Products = new List<ShipmentOrderProductVariantModel>();
        }
        [SmartResourceDisplayName("Admin.Orders.Shipments.ID")]
        public override int Id { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Shipments.OrderID")]
        public int OrderId { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Shipments.TotalWeight")]
        public string TotalWeight { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Shipments.TrackingNumber")]
        public string TrackingNumber { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Shipments.ShippedDate")]
        public string ShippedDate { get; set; }
        public bool CanShip { get; set; }
        [SmartResourceDisplayName("Admin.Orders.Shipments.DeliveryDate")]
        public string DeliveryDate { get; set; }
        public bool CanDeliver { get; set; }

        public List<ShipmentOrderProductVariantModel> Products { get; set; }

        public bool DisplayPdfPackagingSlip { get; set; }

        #region Nested classes

        public class ShipmentOrderProductVariantModel : EntityModelBase
        {
            public int OrderProductVariantId { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public string Sku { get; set; }
            public string AttributeInfo { get; set; }

            //weight of one item (product)
            public string ItemWeight { get; set; }
            public string ItemDimensions { get; set; }

            public int QuantityToAdd { get; set; }
            public int QuantityOrdered { get; set; }
            public int QuantityInThisShipment { get; set; }
            public int QuantityInAllShipments { get; set; }
        }
        #endregion
    }
}