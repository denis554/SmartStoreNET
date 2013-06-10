
namespace SmartStore.Core.Domain.Customers
{
    public static partial class SystemCustomerAttributeNames
    {
        //Form fields
        public static string FirstName { get { return "FirstName"; } }
        public static string LastName { get { return "LastName"; } }
        public static string Gender { get { return "Gender"; } }
        public static string DateOfBirth { get { return "DateOfBirth"; } }
        public static string Company { get { return "Company"; } }
        public static string StreetAddress { get { return "StreetAddress"; } }
        public static string StreetAddress2 { get { return "StreetAddress2"; } }
        public static string ZipPostalCode { get { return "ZipPostalCode"; } }
        public static string City { get { return "City"; } }
        public static string CountryId { get { return "CountryId"; } }
        public static string StateProvinceId { get { return "StateProvinceId"; } }
        public static string Phone { get { return "Phone"; } }
        public static string Fax { get { return "Fax"; } }

        //Other attributes
        public static string AvatarPictureId { get { return "AvatarPictureId"; } }
        public static string ForumPostCount { get { return "ForumPostCount"; } }
        public static string Signature { get { return "Signature"; } }
        public static string PasswordRecoveryToken { get { return "PasswordRecoveryToken"; } }
        public static string AccountActivationToken { get { return "AccountActivationToken"; } }
        public static string LastVisitedPage { get { return "LastVisitedPage"; } }
        public static string ImpersonatedCustomerId { get { return "ImpersonatedCustomerId"; } }

		//depends on store
		public static string CurrencyId { get { return "CurrencyId"; } }
		public static string LanguageId { get { return "LanguageId"; } }
		public static string SelectedPaymentMethod { get { return "SelectedPaymentMethod"; } }
		public static string SelectedShippingOption { get { return "SelectedShippingOption"; } }
		public static string OfferedShippingOptions { get { return "OfferedShippingOptions"; } }
		public static string LastContinueShoppingPage { get { return "LastContinueShoppingPage"; } }
		public static string NotifiedAboutNewPrivateMessages { get { return "NotifiedAboutNewPrivateMessages"; } }
		public static string WorkingDesktopThemeName { get { return "WorkingDesktopThemeName"; } }
        public static string DontUseMobileVersion { get { return "DontUseMobileVersion"; } }
		public static string TaxDisplayTypeId { get { return "TaxDisplayTypeId"; } }
		public static string UseRewardPointsDuringCheckout { get { return "UseRewardPointsDuringCheckout"; } }
    }
}