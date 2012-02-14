namespace IndexLibrary.Web
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Web;

    public class PostSubmitter
    {
        #region Fields

        private string contentType = "application/x-www-form-urlencoded";
        private NameValueCollection headerValues = new System.Collections.Specialized.NameValueCollection();
        private PostTypeEnum postType = PostTypeEnum.Post;
        private string url = string.Empty;
        private NameValueCollection values = new System.Collections.Specialized.NameValueCollection();

        #endregion Fields

        #region Constructors

        public PostSubmitter(string url)
        {
            this.url = url;
        }

        #endregion Constructors

        #region Properties

        public string ContentType
        {
            get { return this.contentType; }
            set { this.contentType = value; }
        }

        public System.Collections.Specialized.NameValueCollection Headers
        {
            get { return this.headerValues; }
        }

        public System.Collections.Specialized.NameValueCollection PostItems
        {
            get { return this.values; }
        }

        public PostTypeEnum Type
        {
            get { return this.postType; }
            set { this.postType = value; }
        }

        public string URL
        {
            get { return this.url; }
            set { this.url = value; }
        }

        #endregion Properties

        #region Methods

        public string Post()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            for (int i = 0; i < values.Count; i++) EncodeAndAddItem(ref builder, values.GetKey(i), values[i]);
            return PostData(this.url, builder.ToString());
        }

        private void EncodeAndAddItem(ref System.Text.StringBuilder builder, string key, string value)
        {
            if (builder == null) builder = new System.Text.StringBuilder();
            if (builder.Length != 0) builder.Append("&");
            builder.Append(key);
            builder.Append("=");
            builder.Append(HttpUtility.UrlEncode(value));
        }

        private string PostData(string url, string postData)
        {
            System.Net.HttpWebRequest request = null;
            if (this.postType == PostTypeEnum.Post) {
                Uri uri = new Uri(url);
                request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(uri);
                request.Method = "POST";
                request.ContentType = string.IsNullOrEmpty(this.contentType) ? "application/x-www-form-urlencoded" : this.contentType;
                request.ContentLength = postData.Length;
                if (this.headerValues.Count > 0) {
                    foreach (var key in this.headerValues.AllKeys) {
                        if (request.Headers.AllKeys.Contains(key)) continue;

                        switch (key.ToLower()) {
                            case "host":
                                continue;
                            case "user-agent":
                                request.UserAgent = this.headerValues[key];
                                continue;
                            case "accept":
                                request.Accept = this.headerValues[key];
                                continue;
                            case "referer":
                                request.Referer = this.headerValues[key];
                                continue;
                            case "content-length":
                                request.ContentLength = long.Parse(this.headerValues[key]);
                                continue;
                        }

                        request.Headers.Add(key, this.headerValues[key]);
                    }
                }
                using (System.IO.Stream writeStream = request.GetRequestStream()) {
                    System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                    byte[] bytes = encoding.GetBytes(postData);
                    writeStream.Write(bytes, 0, bytes.Length);
                }
            }
            else {
                string strUri = url + (url.Contains('?') ? '&' : '?') + postData;
                Uri uri = new Uri(strUri);
                request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(uri);
                request.Method = "GET";
                if (this.headerValues.Count > 0) {
                    foreach (var key in this.headerValues.AllKeys) {
                        if (request.Headers.AllKeys.Contains(key)) continue;
                        if (key.Equals("User-Agent", StringComparison.OrdinalIgnoreCase)) {
                            request.UserAgent = this.headerValues[key];
                            continue;
                        }
                        else if (key.Equals("Referer", StringComparison.OrdinalIgnoreCase)) {
                            request.Referer = this.headerValues[key];
                            continue;
                        }
                        else if (key.Equals("Accept", StringComparison.OrdinalIgnoreCase)) {
                            request.Accept = this.headerValues[key];
                            continue;
                        }
                        else if (key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)) {
                            request.ContentType = this.headerValues[key];
                            continue;
                        }
                        request.Headers.Add(key, this.headerValues[key]);
                    }
                }
            }

            string result = string.Empty;
            using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse()) {
                using (System.IO.Stream responseStream = response.GetResponseStream()) {
                    using (System.IO.StreamReader readStream = new System.IO.StreamReader(responseStream, System.Text.Encoding.UTF8)) {
                        result = readStream.ReadToEnd();
                    }
                }
            }

            return result;
        }

        #endregion Methods
    }
}