using System;
using System.Collections.Generic;

namespace Q42.Wheels.Api.Nabaztag
{
    class UrlListAction : NabaztagAction
    {
        /// <summary>
        /// Urls the bunny will play
        /// </summary>
        private List<string> urls;
        /// <summary>
        /// Urls the bunny will play
        /// </summary>
        public List<string> Urls {
            get {
                return this.urls;
            }
            protected set {
                this.urls = value;
            }
        }

        /// <summary>
        /// Send a url to play to the Nabaztag
        /// </summary>
        /// <param name="message"></param>
        /// <param name="voice">defaults to UK-Penelope</param>
        public UrlListAction(string url)
        {
            if (url.Equals(string.Empty))
                throw new ArgumentOutOfRangeException("No empty url allowed");

            Urls = new List<string> { url };
        }

        /// <summary>
        /// Send a list of urls to play to the Nabaztag
        /// </summary>
        /// <param name="message"></param>
        /// <param name="voice">defaults to UK-Penelope</param>
        public UrlListAction(List<string> urls)
        {
            if (urls == null)
                throw new ArgumentOutOfRangeException("No empty url list allowed");

            Urls = urls;
        }

        /// <summary>
        /// Return all parameters for this action in the given parameters dictionary
        /// </summary>
        /// <param name="parameters"></param>
        public override void ToStringList(ref Dictionary<String, String> parameters)
        {
            string urlList = string.Empty;
            foreach (string url in Urls)
            {
                urlList += "|" + url;
            }

            parameters.Add("urlList", urlList.Substring(1));
        }
    }
}

