using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Services.Helpers;

namespace SmartStore.Services.Customers
{
    /// <summary>
    /// Customer report service
    /// </summary>
    public partial class CustomerReportService : ICustomerReportService
    {
        #region Fields

        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        
        #endregion

        #region Ctor
        
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="customerRepository">Customer repository</param>
        /// <param name="orderRepository">Order repository</param>
        /// <param name="customerService">Customer service</param>
        /// <param name="dateTimeHelper">Date time helper</param>
        public CustomerReportService(IRepository<Customer> customerRepository,
            IRepository<Order> orderRepository, ICustomerService customerService,
            IDateTimeHelper dateTimeHelper)
        {
            this._customerRepository = customerRepository;
            this._orderRepository = orderRepository;
            this._customerService = customerService;
            this._dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get best customers
        /// </summary>
        /// <param name="startTime">Order start time; null to load all</param>
        /// <param name="endTime">Order end time; null to load all</param>
        /// <param name="os">Order status; null to load all records</param>
        /// <param name="ps">Order payment status; null to load all records</param>
        /// <param name="ss">Order shippment status; null to load all records</param>
        /// <param name="orderBy">1 - order by order total, 2 - order by number of orders</param>
        /// <returns>Report</returns>
        public virtual IList<BestCustomerReportLine> GetBestCustomersReport(DateTime? startTime,
            DateTime? endTime, OrderStatus? os, PaymentStatus? ps, ShippingStatus? ss, int orderBy)
        {
            int? orderStatusId = null;
            if (os.HasValue)
                orderStatusId = (int)os.Value;

            int? paymentStatusId = null;
            if (ps.HasValue)
                paymentStatusId = (int)ps.Value;

            int? shippingStatusId = null;
            if (ss.HasValue)
                shippingStatusId = (int)ss.Value;
            var query1 = from c in _customerRepository.Table
                         join o in _orderRepository.Table on c.Id equals o.CustomerId
                         where (!startTime.HasValue || startTime.Value <= o.CreatedOnUtc) &&
                         (!endTime.HasValue || endTime.Value >= o.CreatedOnUtc) &&
                         (!orderStatusId.HasValue || orderStatusId == o.OrderStatusId) &&
                         (!paymentStatusId.HasValue || paymentStatusId == o.PaymentStatusId) &&
                         (!shippingStatusId.HasValue || shippingStatusId == o.ShippingStatusId) &&
                         (!o.Deleted) &&
                         (!c.Deleted)
                         select new { c, o };

            var query2 = from co in query1
                         group co by co.c.Id into g
                         select new
                         {
                             CustomerId = g.Key,
                             OrderTotal = g.Sum(x => x.o.OrderTotal),
                             OrderCount = g.Count()
                         };
            switch (orderBy)
            {
                case 1:
                    {
                        query2 = query2.OrderByDescending(x => x.OrderTotal);
                    }
                    break;
                case 2:
                    {
                        query2 = query2.OrderByDescending(x => x.OrderCount);
                    }
                    break;
                default:
                    throw new ArgumentException("Wrong orderBy parameter", "orderBy");
            }

            //load 20 customers
            query2 = query2.Take(20);

            var result = query2.ToList().Select(x =>
            {
                return new BestCustomerReportLine()
                {
                    CustomerId = x.CustomerId,
                    OrderTotal = x.OrderTotal,
                    OrderCount = x.OrderCount
                };
            }).ToList();
            return result;
        }

        /// <summary>
        /// Gets a report of customers registered in the last days
        /// </summary>
        /// <param name="days">Customers registered in the last days</param>
        /// <returns>Number of registered customers</returns>
        public virtual int GetRegisteredCustomersReport(int days)
        {
            DateTime date = _dateTimeHelper.ConvertToUserTime(DateTime.Now).AddDays(-days);

            var registeredCustomerRole = _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered);
            if (registeredCustomerRole == null)
                return 0;

            var query = from c in _customerRepository.Table
                        from cr in c.CustomerRoles
                        where !c.Deleted &&
                        cr.Id == registeredCustomerRole.Id &&
                        c.CreatedOnUtc >= date 
                        //&& c.CreatedOnUtc <= DateTime.UtcNow
                        select c;
            int count = query.Count();
            return count;
        }

        #endregion
    }
}