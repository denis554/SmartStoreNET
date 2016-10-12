using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Events;
using SmartStore.Core.Plugins;
using SmartStore.Data.Caching;
using SmartStore.Services.Customers;

namespace SmartStore.Services.Directory
{
    public partial class DeliveryTimeService : IDeliveryTimeService
    {
        private readonly IRepository<DeliveryTime> _deliveryTimeRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductVariantAttributeCombination> _attributeCombinationRepository;
        private readonly ICustomerService _customerService;
        private readonly IPluginFinder _pluginFinder;
        private readonly IEventPublisher _eventPublisher;
		private readonly CatalogSettings _catalogSettings;

        public DeliveryTimeService(
            IRepository<DeliveryTime> deliveryTimeRepository,
            IRepository<Product> productRepository,
            IRepository<ProductVariantAttributeCombination> attributeCombinationRepository,
            ICustomerService customerService,
            IPluginFinder pluginFinder,
            IEventPublisher eventPublisher,
			CatalogSettings catalogSettings)
        {
            this._deliveryTimeRepository = deliveryTimeRepository;
            this._customerService = customerService;
            this._pluginFinder = pluginFinder;
            this._eventPublisher = eventPublisher;
            this._productRepository = productRepository;
            this._attributeCombinationRepository = attributeCombinationRepository;
			this._catalogSettings = catalogSettings;
        }

        public virtual void DeleteDeliveryTime(DeliveryTime deliveryTime)
        {
            if (deliveryTime == null)
                throw new ArgumentNullException("deliveryTime");

            if (this.IsAssociated(deliveryTime.Id))
                throw new SmartException("The delivery time cannot be deleted. It has associated product variants");

            _deliveryTimeRepository.Delete(deliveryTime);

            //event notification
            _eventPublisher.EntityDeleted(deliveryTime);
        }

        public virtual bool IsAssociated(int deliveryTimeId)
        {
            if (deliveryTimeId == 0)
                return false;

            var query = 
				from p in _productRepository.Table
				where p.DeliveryTimeId == deliveryTimeId || p.ProductVariantAttributeCombinations.Any(c => c.DeliveryTimeId == deliveryTimeId)
				select p.Id;

            return query.Count() > 0;
        }

        public virtual DeliveryTime GetDeliveryTimeById(int deliveryTimeId)
        {
            if (deliveryTimeId == 0)
                return null;

            return  _deliveryTimeRepository.GetById(deliveryTimeId);
        }

		public virtual DeliveryTime GetDeliveryTime(Product product)
		{
			if (product == null)
				return null;

			if ((product.ManageInventoryMethod == ManageInventoryMethod.ManageStock || product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
				&& _catalogSettings.DeliveryTimeIdForEmptyStock.HasValue && product.StockQuantity <= 0)
			{
				return GetDeliveryTimeById(_catalogSettings.DeliveryTimeIdForEmptyStock.Value);
			}

			return GetDeliveryTimeById(product.DeliveryTimeId ?? 0);
		}

        public virtual IList<DeliveryTime> GetAllDeliveryTimes()
        {
			var query = _deliveryTimeRepository.Table.OrderBy(c => c.DisplayOrder);
			var deliveryTimes = query.ToListCached("db.delivtimes.all");
			return deliveryTimes;
		}

        public virtual void InsertDeliveryTime(DeliveryTime deliveryTime)
        {
            if (deliveryTime == null)
                throw new ArgumentNullException("deliveryTime");

            _deliveryTimeRepository.Insert(deliveryTime);

            //event notification
            _eventPublisher.EntityInserted(deliveryTime);
        }

        public virtual void UpdateDeliveryTime(DeliveryTime deliveryTime)
        {
            if (deliveryTime == null)
                throw new ArgumentNullException("deliveryTime");

            _deliveryTimeRepository.Update(deliveryTime);

            //event notification
            _eventPublisher.EntityUpdated(deliveryTime);
        }
    }
}