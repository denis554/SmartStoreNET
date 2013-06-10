using System;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Data;

namespace SmartStore.Services.Common
{
    public static class GenericAttributeExtentions
    {
        /// <summary>
        /// Get an attribute of an entity
        /// </summary>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="key">Key</param>
		/// <param name="storeId">Load a value specific for a certain store; pass 0 to load a value shared for all stores</param>
        /// <returns>Attribute</returns>
		public static TPropType GetAttribute<TPropType>(this BaseEntity entity, string key, int storeId = 0)
        {
            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            return GetAttribute<TPropType>(entity, key, genericAttributeService, storeId);
        }
        /// <summary>
        /// Get an attribute of an entity
        /// </summary>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="key">Key</param>
        /// <param name="genericAttributeService">GenericAttributeService</param>
		/// <param name="storeId">Load a value specific for a certain store; pass 0 to load a value shared for all stores</param>
        /// <returns>Attribute</returns>
        public static TPropType GetAttribute<TPropType>(this BaseEntity entity,
			string key, IGenericAttributeService genericAttributeService, int storeId = 0)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            string keyGroup = entity.GetUnproxiedEntityType().Name;

			var props = genericAttributeService.GetAttributesForEntity(entity.Id, keyGroup)
				.Where(x => x.StoreId == storeId)
				.ToList();
			if (props.Count == 0)
                return default(TPropType);

            var prop = props.FirstOrDefault(ga =>
                ga.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)); //should be culture invariant

            if (prop == null || string.IsNullOrEmpty(prop.Value))
                return default(TPropType);

            return CommonHelper.To<TPropType>(prop.Value);
        }
    }
}
