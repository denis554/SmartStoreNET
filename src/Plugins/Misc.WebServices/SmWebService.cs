﻿//Contributor: Nicolas Muniere


using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel.Activation;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;
using SmartStore.Plugin.Misc.WebServices.Security;
using SmartStore.Services.Authentication;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Security;
using SmartStore.Services.Configuration;

namespace SmartStore.Plugin.Misc.WebServices
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class SmWebService : IWebService
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly CustomerSettings _customerSettings;
        private readonly IPermissionService _permissionSettings;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IWorkContext _workContext;
        private readonly IPluginFinder _pluginFinder;
		private readonly IStoreContext _storeContext;
		private readonly ISettingService _settingService;	// codehint: sm-add
        
        #endregion 

        #region Ctor

        public SmWebService()
        {
            _addressService = EngineContext.Current.Resolve<IAddressService>();
            _countryService = EngineContext.Current.Resolve<ICountryService>();
            _stateProvinceService = EngineContext.Current.Resolve<IStateProvinceService>();
            _customerService = EngineContext.Current.Resolve<ICustomerService>();
            _customerRegistrationService = EngineContext.Current.Resolve<ICustomerRegistrationService>();
            _customerSettings = EngineContext.Current.Resolve<CustomerSettings>();
            _permissionSettings = EngineContext.Current.Resolve<IPermissionService>();
            _orderProcessingService = EngineContext.Current.Resolve<IOrderProcessingService>();
            _orderService = EngineContext.Current.Resolve<IOrderService>();
            _authenticationService = EngineContext.Current.Resolve<IAuthenticationService>();
            _workContext = EngineContext.Current.Resolve<IWorkContext>();
            _pluginFinder = EngineContext.Current.Resolve<IPluginFinder>();
			_storeContext = EngineContext.Current.Resolve<IStoreContext>();
			_settingService = EngineContext.Current.Resolve<ISettingService>();
        }

        #endregion 

        #region Utilities

        protected void CheckAccess(string usernameOrEmail, string userPassword)
        {
            //check whether web service plugin is installed
            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("Misc.WebServices");
            if (pluginDescriptor == null)
                throw new ApplicationException("Web services plugin cannot be loaded");
			if (!(_storeContext.CurrentStore.Id == 0 ||
				_settingService.GetSettingByKey<string>(pluginDescriptor.GetSettingKey("LimitedToStores")).ToIntArrayContains(_storeContext.CurrentStore.Id, true)))
				throw new ApplicationException("Web services plugin is not available in this store");

            if (!_customerRegistrationService.ValidateCustomer(usernameOrEmail, userPassword))
                    throw new ApplicationException("Not allowed");
            
            var customer = _customerSettings.UsernamesEnabled ? _customerService.GetCustomerByUsername(usernameOrEmail) : _customerService.GetCustomerByEmail(usernameOrEmail);

            _workContext.CurrentCustomer = customer;
            _authenticationService.SignIn(customer, true);

            //valdiate whether we can access this web service
            if (!_permissionSettings.Authorize(WebServicePermissionProvider.AccessWebService))
                throw new ApplicationException("Not allowed to access web service");
        }

        protected List<Order> GetOrderCollection(int[] ordersId)
        {
            var orders = new List<Order>();
            foreach (int id in ordersId)
            {
                orders.Add(_orderService.GetOrderById(id));
            }
            return orders;
        }

        #endregion

        #region Orders

        public DataSet GetPaymentMethod(string usernameOrEmail, string userPassword)
        {
            CheckAccess(usernameOrEmail, userPassword);
            if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
                throw new ApplicationException("Not allowed to manage orders");

            var plugins = from p in _pluginFinder.GetPlugins<IPaymentMethod>()
                          select p;

            var dataset = new DataSet();
            var datatable = dataset.Tables.Add("PaymentMethod");
            datatable.Columns.Add("SystemName");
            datatable.Columns.Add("Name");
            foreach (var plugin in plugins)
            {
                datatable.LoadDataRow(new object[] { plugin.PluginDescriptor.SystemName, plugin.PluginDescriptor.FriendlyName }, true);
            }
            return dataset;
        }

        public DataSet ExecuteDataSet(string[] sqlStatements, string usernameOrEmail, string userPassword)
        {
            //uncomment lines below in order to allow execute any SQL
            //CheckAccess(usernameOrEmail, userPassword);

            //if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
            //    throw new ApplicationException("Not allowed to manage orders");

            //var dataset = new DataSet();

            //var dataSettingsManager = new DataSettingsManager();
            //var dataProviderSettings = dataSettingsManager.LoadSettings();
            //using (SqlConnection connection = new SqlConnection(dataProviderSettings.DataConnectionString))
            //{
            //    foreach (var sqlStatement in sqlStatements)
            //    {
            //        DataTable dt = dataset.Tables.Add();
            //        SqlDataAdapter adapter = new SqlDataAdapter();
            //        adapter.SelectCommand = new SqlCommand(sqlStatement, connection);
            //        adapter.Fill(dt);

            //    }
            //}

            //return dataset;




            return null;
        }
        
        public Object ExecuteScalar(string sqlStatement, string usernameOrEmail, string userPassword)
        {
            //uncomment lines below in order to allow execute any SQL
            //CheckAccess(usernameOrEmail, userPassword);
            //if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
            //    throw new ApplicationException("Not allowed to manage orders");

            //Object result;
            //var dataSettingsManager = new DataSettingsManager();
            //var dataProviderSettings = dataSettingsManager.LoadSettings();
            //using (SqlConnection connection = new SqlConnection(dataProviderSettings.DataConnectionString))
            //{
            //    SqlCommand cmd = new SqlCommand(sqlStatement, connection);
            //    connection.Open();
            //    result = cmd.ExecuteScalar();

            //}

            //return result;



            return null;
        }
        
        public void ExecuteNonQuery(string sqlStatement, string usernameOrEmail, string userPassword)
        {
            //uncomment lines below in order to allow execute any SQL
            //CheckAccess(usernameOrEmail, userPassword);
            //if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
            //    throw new ApplicationException("Not allowed to manage orders");

            //var dataSettingsManager = new DataSettingsManager();
            //var dataProviderSettings = dataSettingsManager.LoadSettings();
            //using (SqlConnection connection = new SqlConnection(dataProviderSettings.DataConnectionString))
            //{
            //    SqlCommand cmd = new SqlCommand(sqlStatement, connection);
            //    connection.Open();
            //    cmd.ExecuteScalar();
            //}
        }

        public List<OrderError> DeleteOrders(int[] ordersId, string usernameOrEmail, string userPassword)
        {
            CheckAccess(usernameOrEmail, userPassword);
            if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
                throw new ApplicationException("Not allowed to manage orders");

            var errors = new List<OrderError>();
            foreach (var order in GetOrderCollection(ordersId))
            {
                try
                {
                    _orderProcessingService.DeleteOrder(order);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(ex.Message);
                }
            }
            return errors;
        }

        public void AddOrderNote(int orderId, string note, bool displayToCustomer, string usernameOrEmail, string userPassword)
        {
            CheckAccess(usernameOrEmail, userPassword);
            if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
                throw new ApplicationException("Not allowed to manage orders");

            try
            {
                var order = _orderService.GetOrderById(orderId);
                order.OrderNotes.Add(new OrderNote()
                {
                    Note = note,
                    DisplayToCustomer = displayToCustomer,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }
        }
        
        public void UpdateOrderBillingInfo(int orderId, string firstName, string lastName, string phone, string email, string fax, string company, string address1, string address2,
            string city, string stateProvinceAbbreviation, string countryThreeLetterIsoCode, string postalCode, string usernameOrEmail, string userPassword)
        {
            CheckAccess(usernameOrEmail, userPassword);
            if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
                throw new ApplicationException("Not allowed to manage orders");

            try
            {
                var order = _orderService.GetOrderById(orderId);
                var a = order.BillingAddress;
                a.FirstName = firstName;
                a.LastName = lastName;
                a.PhoneNumber = phone;
                a.Email = email;
                a.FaxNumber = fax;
                a.Company = company;
                a.Address1 = address1;
                a.Address2 = address2;
                a.City = city;
                StateProvince stateProvince = null;
                if (!String.IsNullOrEmpty(stateProvinceAbbreviation))
                    stateProvince = _stateProvinceService.GetStateProvinceByAbbreviation(stateProvinceAbbreviation);
                a.StateProvince = stateProvince;
                Country country = null;
                if (!String.IsNullOrEmpty(countryThreeLetterIsoCode))
                    country = _countryService.GetCountryByThreeLetterIsoCode(countryThreeLetterIsoCode);
                a.Country = country;
                a.ZipPostalCode = postalCode;

                _addressService.UpdateAddress(a);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }
        }
        
        public void UpdateOrderShippingInfo(int orderId, string firstName, string lastName, string phone, string email, string fax, string company, string address1, string address2,
            string city, string stateProvinceAbbreviation, string countryThreeLetterIsoCode, string postalCode, string usernameOrEmail, string userPassword)
        {
            CheckAccess(usernameOrEmail, userPassword);
            if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
                throw new ApplicationException("Not allowed to manage orders");

            try
            {
                var order = _orderService.GetOrderById(orderId);
                var a = order.ShippingAddress;
                a.FirstName = firstName;
                a.LastName = lastName;
                a.PhoneNumber = phone;
                a.Email = email;
                a.FaxNumber = fax;
                a.Company = company;
                a.Address1 = address1;
                a.Address2 = address2;
                a.City = city;
                StateProvince stateProvince = null;
                if (!String.IsNullOrEmpty(stateProvinceAbbreviation))
                    stateProvince = _stateProvinceService.GetStateProvinceByAbbreviation(stateProvinceAbbreviation);
                a.StateProvince = stateProvince;
                Country country = null;
                if (!String.IsNullOrEmpty(countryThreeLetterIsoCode))
                    country = _countryService.GetCountryByThreeLetterIsoCode(countryThreeLetterIsoCode);
                a.Country = country;
                a.ZipPostalCode = postalCode;

                _addressService.UpdateAddress(a);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }

        }
        
        public void SetOrderPaymentPending(int orderId, string usernameOrEmail, string userPassword)
        {
            CheckAccess(usernameOrEmail, userPassword);
            if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
                throw new ApplicationException("Not allowed to manage orders");

            try
            {
                Order order = _orderService.GetOrderById(orderId);
                order.PaymentStatus = PaymentStatus.Pending;
                _orderService.UpdateOrder(order);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }

        }
        
        public void SetOrderPaymentPaid(int orderId, string usernameOrEmail, string userPassword)
        {
            CheckAccess(usernameOrEmail, userPassword);
            if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
                throw new ApplicationException("Not allowed to manage orders");

            try
            {
                var order = _orderService.GetOrderById(orderId);
                _orderProcessingService.MarkOrderAsPaid(order);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }

        }
        
        public void SetOrderPaymentPaidWithMethod(int orderId, string paymentMethodName, string usernameOrEmail, string userPassword)
        {
            CheckAccess(usernameOrEmail, userPassword);
            if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
                throw new ApplicationException("Not allowed to manage orders");

            try
            {
                var order = _orderService.GetOrderById(orderId);
                order.PaymentMethodSystemName = paymentMethodName;
                _orderService.UpdateOrder(order);
                _orderProcessingService.MarkOrderAsPaid(order);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }

        }
        
        public void SetOrderPaymentRefund(int orderId, bool offline, string usernameOrEmail, string userPassword)
        {
            CheckAccess(usernameOrEmail, userPassword);
            if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
                throw new ApplicationException("Not allowed to manage orders");

            try
            {
                var order = _orderService.GetOrderById(orderId);
                if (offline)
                {
                    _orderProcessingService.RefundOffline(order);
                }
                else
                {
                    var errors = _orderProcessingService.Refund(order);
                    if (errors.Count > 0)
                    {
                        throw new ApplicationException(errors[0]);
                    }

                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }

        }

        
        public List<OrderError> SetOrdersStatusCanceled(int[] ordersId, string usernameOrEmail, string userPassword)
        {
            CheckAccess(usernameOrEmail, userPassword);
            if (!_permissionSettings.Authorize(StandardPermissionProvider.ManageOrders))
                throw new ApplicationException("Not allowed to manage orders");

            var errors = new List<OrderError>();
            foreach (var order in GetOrderCollection(ordersId))
            {
                try
                {
                    _orderProcessingService.CancelOrder(order, true);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(ex.Message);
                }
            }
            return errors;
        }
        
        #endregion
    }
}
