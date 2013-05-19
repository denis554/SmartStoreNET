using System.Net;

namespace SmartStore.Services.Directory
{
    /// <summary>
    /// Country lookup helper
    /// </summary>
    public partial interface IGeoCountryLookup
    {
        string LookupCountryCode(string str);

        string LookupCountryCode(IPAddress addr);

        string LookupCountryName(string str);

        string LookupCountryName(IPAddress addr);
    }
}