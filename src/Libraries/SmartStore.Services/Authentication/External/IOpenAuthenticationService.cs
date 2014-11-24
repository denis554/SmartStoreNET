//Contributor:  Nicholas Mayne

using System.Collections.Generic;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Authentication.External
{
    public partial interface IOpenAuthenticationService
    {
        /// <summary>
        /// Load active external authentication methods
        /// </summary>
		/// <param name="storeId">Load records allows only in specified store; pass 0 to load all records</param>
        /// <returns>Payment methods</returns>
		IEnumerable<Provider<IExternalAuthenticationMethod>> LoadActiveExternalAuthenticationMethods(int storeId = 0);

        /// <summary>
        /// Load external authentication method by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found external authentication method</returns>
		Provider<IExternalAuthenticationMethod> LoadExternalAuthenticationMethodBySystemName(string systemName, int storeId = 0);

        /// <summary>
        /// Load all external authentication methods
        /// </summary>
		/// <param name="storeId">Load records allows only in specified store; pass 0 to load all records</param>
        /// <returns>External authentication methods</returns>
		IEnumerable<Provider<IExternalAuthenticationMethod>> LoadAllExternalAuthenticationMethods(int storeId = 0);


        bool AccountExists(OpenAuthenticationParameters parameters);

        void AssociateExternalAccountWithUser(Customer customer, OpenAuthenticationParameters parameters);

        Customer GetUser(OpenAuthenticationParameters parameters);

        IList<ExternalAuthenticationRecord> GetExternalIdentifiersFor(Customer customer);

        void RemoveAssociation(OpenAuthenticationParameters parameters);
    }
}