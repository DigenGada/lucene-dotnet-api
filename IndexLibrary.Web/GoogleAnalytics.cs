namespace IndexLibrary.Web
{
    using System;
    using System.Collections.Specialized;
    using System.Security.Cryptography;


    #region Enumerations

    public enum GoogleAnalyticsRequestType
    {
        Page = 0,
        Event = 1,
        Transaction = 2,
        Item = 3,
        CustomVariable = 4
    }

    #endregion Enumerations

    public sealed class CryptoRandomGenerator
    {
        #region Fields

        private RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();

        #endregion Fields

        #region Constructors

        public CryptoRandomGenerator()
        {
        }

        #endregion Constructors

        #region Methods

        public double RandomDouble()
        {
            return RandomDouble(double.MinValue, double.MaxValue);
        }

        public double RandomDouble(double low, double high)
        {
            if (low >= high)
                throw new ArgumentException("low must be less than high");

            byte[] buffer = new byte[8];
            double number;

            provider.GetBytes(buffer);
            number = Math.Abs(BitConverter.ToDouble(buffer, 0));

            number = number - Math.Truncate(number);
            return number * (high - low) + low;
        }

        public int RandomInt()
        {
            return RandomInt(int.MinValue, int.MaxValue);
        }

        public int RandomInt(int low, int high)
        {
            return (int)RandomDouble(low, high);
        }

        public ulong RandomLong()
        {
            return RandomLong(ulong.MinValue, ulong.MaxValue);
        }

        public ulong RandomLong(ulong low, ulong high)
        {
            if (low >= high)
                throw new ArgumentException("low must be less than high");

            byte[] buffer = new byte[8];
            double number;

            provider.GetBytes(buffer);
            number = Math.Abs(BitConverter.ToDouble(buffer, 0));

            number = number - Math.Truncate(number);

            return (ulong)(number * ((double)high - (double)low) + low);
        }

        #endregion Methods
    }

    public sealed class GenericGAPair<T> : IGenericPair
    {
        #region Fields

        private string urchinValue;
        private T value;

        #endregion Fields

        #region Constructors

        public GenericGAPair(string urchinName, T value)
        {
            this.urchinValue = urchinName;
            this.value = value;
        }

        #endregion Constructors

        #region Properties

        public bool HasValue
        {
            get { return this.value != null; }
        }

        public string UrchinName
        {
            get { return this.urchinValue; }
            set { this.urchinValue = value; }
        }

        public T Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        #endregion Properties

        #region Methods

        public static implicit operator T(GenericGAPair<T> obj)
        {
            return obj.value;
        }

        public override string ToString() {
            if (!this.HasValue)
                return null;
            return this.value.ToString();
        }

        #endregion Methods
    }

    public interface IGenericPair {

        bool HasValue { get; }

        string UrchinName { get; }

    }

    public sealed class GoogleAnalyticsPageTrackSubmitter
    {
        #region Fields

        private static string GoogleAnalyticsUrl = @"http://www.google-analytics.com/__utm.gif";

        private GenericGAPair<string> absoluteUrl = new GenericGAPair<string>("utmr", null);
        private GenericGAPair<string> accountString = new GenericGAPair<string>("utmac", null);
        private NameValueCollection additionalValues = new NameValueCollection();
        private GenericGAPair<ulong?> adsenseID = new GenericGAPair<ulong?>("utmhid", null);
        private GenericGAPair<string> clientBrowserCultureCode = new GenericGAPair<string>("utmul", null);
        private GenericGAPair<string> clientBrowserFlashVersion = new GenericGAPair<string>("utmfl", null);
        private GenericGAPair<bool?> clientBrowserJavaEnabled = new GenericGAPair<bool?>("utmje", false);
        private GenericGAPair<string> clientBrowserLanguageEncoding = new GenericGAPair<string>("utmcs", null);
        private GenericGAPair<int?> clientScreenColorDepthInBits = new GenericGAPair<int?>("utmsc", null);
        private GenericGAPair<string> clientScreenResolution = new GenericGAPair<string>("utmsr", null);
        private GenericGAPair<string> cookieValues = new GenericGAPair<string>("utmcc", null);
        private GenericGAPair<string> extensibleParameter = new GenericGAPair<string>("utme", null);
        private GenericGAPair<string> hostName = new GenericGAPair<string>("utmhn", null);
        private GenericGAPair<bool?> newCampaignVisit = new GenericGAPair<bool?>("utmcn", true);
        private GenericGAPair<string> orderAffiliation = new GenericGAPair<string>("utmtst", null);
        private GenericGAPair<string> orderBillingCity = new GenericGAPair<string>("utmtci", null);
        private GenericGAPair<string> orderBillingCountry = new GenericGAPair<string>("utmtco", null);
        private GenericGAPair<string> orderBillingRegion = new GenericGAPair<string>("utmtrg", null);
        private GenericGAPair<string> orderID = new GenericGAPair<string>("utmtid", null);
        private GenericGAPair<double?> orderShippingTotal = new GenericGAPair<double?>("utmtsp", null);
        private GenericGAPair<double?> orderTax = new GenericGAPair<double?>("utmttx", null);
        private GenericGAPair<double?> orderTotal = new GenericGAPair<double?>("utmtto", null);
        private GenericGAPair<string> pageTitle = new GenericGAPair<string>("utmdt", null);
        private GenericGAPair<string> productCode = new GenericGAPair<string>("utmipc", null);
        private GenericGAPair<string> productName = new GenericGAPair<string>("utmipn", null);
        private GenericGAPair<double?> productPrice = new GenericGAPair<double?>("utmipr", null);
        private GenericGAPair<int?> productQuantity = new GenericGAPair<int?>("utmiqt", null);
        private GenericGAPair<string> productVariation = new GenericGAPair<string>("utmiva", null);
        private GenericGAPair<string> relativeUrl = new GenericGAPair<string>("utmp", null);
        private GenericGAPair<ulong?> requestID = new GenericGAPair<ulong?>("utmn", null);
        private GoogleAnalyticsRequestType requestType = GoogleAnalyticsRequestType.Page;
        private GenericGAPair<string> trackingCodeVersion = new GenericGAPair<string>("utmwv", null);

        #endregion Fields

        #region Constructors

        public GoogleAnalyticsPageTrackSubmitter()
            : this(GoogleAnalyticsRequestType.Page)
        {
        }

        public GoogleAnalyticsPageTrackSubmitter(GoogleAnalyticsRequestType requestType)
        {
            this.requestType = requestType;
            CryptoRandomGenerator random = new CryptoRandomGenerator();
            this.adsenseID.Value = random.RandomLong();
            this.requestID.Value = random.RandomLong();
        }

        #endregion Constructors

        #region Properties

        public GenericGAPair<string> AbsoluteUrl
        {
            get { return absoluteUrl; }
            set { absoluteUrl = value; }
        }

        public GenericGAPair<string> AccountString
        {
            get { return accountString; }
            set { accountString = value; }
        }

        public NameValueCollection AdditionalValues
        {
            get { return this.additionalValues; }
        }

        public GenericGAPair<ulong?> AdsenseID
        {
            get { return adsenseID; }
            set { adsenseID = value; }
        }

        public GenericGAPair<string> ClientBrowserCultureCode
        {
            get { return clientBrowserCultureCode; }
            set { clientBrowserCultureCode = value; }
        }

        public GenericGAPair<string> ClientBrowserFlashVersion
        {
            get { return clientBrowserFlashVersion; }
            set { clientBrowserFlashVersion = value; }
        }

        public GenericGAPair<bool?> ClientBrowserJavaEnabled
        {
            get { return clientBrowserJavaEnabled; }
            set { clientBrowserJavaEnabled = value; }
        }

        public GenericGAPair<string> ClientBrowserLanguageEncoding
        {
            get { return clientBrowserLanguageEncoding; }
            set { clientBrowserLanguageEncoding = value; }
        }

        public GenericGAPair<int?> ClientScreenColorDepthInBits
        {
            get { return clientScreenColorDepthInBits; }
            set { clientScreenColorDepthInBits = value; }
        }

        public GenericGAPair<string> ClientScreenResolution
        {
            get { return clientScreenResolution; }
            set { clientScreenResolution = value; }
        }

        public GenericGAPair<string> CookieValues
        {
            get { return cookieValues; }
            set { cookieValues = value; }
        }

        public GenericGAPair<string> ExtensibleParameter
        {
            get { return extensibleParameter; }
            set { extensibleParameter = value; }
        }

        public GenericGAPair<string> HostName
        {
            get { return hostName; }
            set { hostName = value; }
        }

        public GenericGAPair<bool?> NewCampaignVisit
        {
            get { return newCampaignVisit; }
            set { newCampaignVisit = value; }
        }

        public GenericGAPair<string> OrderAffiliation
        {
            get { return orderAffiliation; }
            set { orderAffiliation = value; }
        }

        public GenericGAPair<string> OrderBillingCity
        {
            get { return orderBillingCity; }
            set { orderBillingCity = value; }
        }

        public GenericGAPair<string> OrderBillingCountry
        {
            get { return orderBillingCountry; }
            set { orderBillingCountry = value; }
        }

        public GenericGAPair<string> OrderBillingRegion
        {
            get { return orderBillingRegion; }
            set { orderBillingRegion = value; }
        }

        public GenericGAPair<string> OrderID
        {
            get { return orderID; }
            set { orderID = value; }
        }

        public GenericGAPair<double?> OrderShippingTotal
        {
            get { return orderShippingTotal; }
            set { orderShippingTotal = value; }
        }

        public GenericGAPair<double?> OrderTax
        {
            get { return orderTax; }
            set { orderTax = value; }
        }

        public GenericGAPair<double?> OrderTotal
        {
            get { return orderTotal; }
            set { orderTotal = value; }
        }

        public GenericGAPair<string> PageTitle
        {
            get { return pageTitle; }
            set { pageTitle = value; }
        }

        public GenericGAPair<string> ProductCode
        {
            get { return productCode; }
            set { productCode = value; }
        }

        public GenericGAPair<string> ProductName
        {
            get { return productName; }
            set { productName = value; }
        }

        public GenericGAPair<double?> ProductPrice
        {
            get { return productPrice; }
            set { productPrice = value; }
        }

        public GenericGAPair<int?> ProductQuantity
        {
            get { return productQuantity; }
            set { productQuantity = value; }
        }

        public GenericGAPair<string> ProductVariation
        {
            get { return productVariation; }
            set { productVariation = value; }
        }

        public GenericGAPair<string> RelativeUrl
        {
            get { return relativeUrl; }
            set { relativeUrl = value; }
        }

        public GenericGAPair<ulong?> RequestID
        {
            get { return requestID; }
            set { requestID = value; }
        }

        public GoogleAnalyticsRequestType RequestType
        {
            get { return requestType; }
            set { requestType = value; }
        }

        public GenericGAPair<string> TrackingCodeVersion
        {
            get { return trackingCodeVersion; }
            set { trackingCodeVersion = value; }
        }

        #endregion Properties

        public void Post() {
            PostSubmitter submitter = new PostSubmitter(GoogleAnalyticsUrl);
            submitter.Type = PostTypeEnum.Get;
            Type type = typeof(GoogleAnalyticsPageTrackSubmitter);
            var properties = type.GetProperties();
            int totalProperties = properties.Length;
            for (int i = 0; i < totalProperties; i++) {
                if (properties[i].Name.Equals("AdditionalValues", StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }
                else if (properties[i].Name.Equals("RequestType", StringComparison.OrdinalIgnoreCase)) {
                    //submitter.PostItems.Add("utmt", this.requestType.ToString());
                    continue;
                }
                else if (properties[i].Name.Equals("ClientBrowserJavaEnabled", StringComparison.OrdinalIgnoreCase)) {
                    if (!this.ClientBrowserJavaEnabled.HasValue)
                        continue;

                    submitter.PostItems.Add(this.ClientBrowserJavaEnabled.UrchinName, this.ClientBrowserJavaEnabled.Value.Value ? "1" : "0");
                    continue;
                }

                object value = properties[i].GetValue(this, null);
                if (value == null)
                    continue;
                IGenericPair pair = (IGenericPair)value;
                if (string.IsNullOrEmpty(pair.UrchinName))
                    continue;

                string strValue = value.ToString();
                if (string.IsNullOrEmpty(strValue))
                    continue;

                submitter.PostItems.Add(pair.UrchinName, value.ToString());
            }

            if (this.additionalValues != null && this.additionalValues.Count > 0)
                submitter.PostItems.Add(this.additionalValues);

            submitter.Headers.Add("Accept-Encoding", "gzip, deflate");
            submitter.Headers.Add("Accept-Language", "en-us,en;q=0.5");
            submitter.Headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
            submitter.Headers.Add("Accept", "image/png,image/*;q=0.8,*/*;q=0.5");
            submitter.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:8.0) Gecko/20100101 Firefox/8.0");
            string result = submitter.Post();
        }
    }
}