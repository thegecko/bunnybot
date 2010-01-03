using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;

namespace Q42.Wheels.Api.Nabaztag
{
    public class NabaztagApiExtension : NabaztagApi
    {
        private readonly static string theBurrowStreamUrl = "http://api.nabaztag.com/vl/FR/api_stream.jsp";

        public NabaztagApiExtension(string oSerial, string oToken) : base(oSerial, oToken)
        {
        }

        public string SendStreamAction(NabaztagAction action)
        {
            Dictionary<String, String> parameters = GetParameterDictionary();
            action.ToStringList(ref parameters);
            XmlDocument responseXml = SendStreamAction(parameters);
            return responseXml.DocumentElement.SelectSingleNode("message").InnerText;
        }

        protected XmlDocument SendStreamAction(Dictionary<String, String> parameters)
        {
            if (!(parameters.ContainsKey("sn") && parameters.ContainsKey("token")))
                throw new ArgumentOutOfRangeException("sn and token are required parameters");

            //all actions paramters are now in the parameter list, build querystring of it (htmlencode values)
            String RequestString = "?";
            foreach (KeyValuePair<String, String> kvp in parameters)
                RequestString += String.Format("{0}={1}&", kvp.Key, HttpUtility.HtmlEncode(kvp.Value));

            //send action to API
            HttpWebResponse response = null;
            XmlDocument responseXml = new XmlDocument();
            try
            {
                // Create the web request  
                HttpWebRequest request = WebRequest.Create(theBurrowStreamUrl + RequestString) as HttpWebRequest;

                // Get response  
                response = request.GetResponse() as HttpWebResponse;
                if (request.HaveResponse == true && response != null)
                {
                    // Get the response stream & read response to XML
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    responseXml.LoadXml(reader.ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                    using (HttpWebResponse errorResponse = (HttpWebResponse)ex.Response)
                    {
                        throw new NabaztagResponseException(String.Format("Response from burrow: '{0}' with the status code '{1}' while accessing url {2}", errorResponse.StatusDescription, errorResponse.StatusCode, theBurrowStreamUrl + RequestString), ex);
                    }
                else
                    throw new NabaztagResponseException(String.Format("Response from burrow: '{0}' while accessing url {2}", ex.Message, theBurrowStreamUrl + RequestString), ex);
            }
            finally
            {
                if (response != null)
                    response.Close();
            }

            //check for faulty response messages from the service
            raisePossibleResultException(responseXml);

            return responseXml;
        }
    }
}
