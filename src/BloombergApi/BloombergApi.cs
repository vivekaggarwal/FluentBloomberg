using Bloomberglp.Blpapi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloombergApi
{
    class BloombergApi
    {
        #region Members / Contructors

        private string _serverHost;
        private int _serverPort;

        private Session _session;
        private Service _refDataService;

        internal BloombergApi(string serverHost = "localhost", int serverPort = 8194)
        {
            _serverHost = serverHost;
            _serverPort = serverPort;
        }

        /// <summary>
        /// Initialise the Session and the Service
        /// </summary>
        internal void Initialise()
        {
            if (_session == null)
            {
                var sessionOptions = new SessionOptions
                {
                    ServerHost = _serverHost,
                    ServerPort = _serverPort
                };

                //Console.WriteLine("Connecting to {0}:{1}", sessionOptions.ServerHost, sessionOptions.ServerPort);

                _session = new Session(sessionOptions);

                if (!_session.Start())
                    throw new Exception("Failed to connect!");

                if (!_session.OpenService("//blp/refdata"))
                {
                    _session.Stop();
                    _session = null;

                    throw new Exception("Failed to open //blp/refdata");
                }

                _refDataService = _session.GetService("//blp/refdata");
            }
        }

        /// <summary>
        /// Dispose the Session and the Service
        /// </summary>
        internal void DisposeSessionAndService()
        {
            _refDataService = null;

            //Stop the session
            if (_session != null)
            {
                _session.Stop();
                _session = null;
            }
        }

        #endregion

        #region Method

        internal Dictionary<string, Dictionary<string, object>> Data(string[] securities, string[] fields, string[] overrideFields, string[] overrideValues)
        {
            Initialise();

            var securitiesFields = new Dictionary<string, Dictionary<string, object>>();

            //Create request
            var referenceDataRequest = _refDataService.CreateRequest("ReferenceDataRequest");

            //Securities
            var securitiesElement = referenceDataRequest.GetElement("securities");
            foreach (var security in securities)
                securitiesElement.AppendValue(security);

            //Fields
            var fieldsElement = referenceDataRequest.GetElement("fields");
            foreach (var field in fields)
                fieldsElement.AppendValue(field);

            if (overrideFields != null &&
                overrideFields.Count() > 0 &&
                overrideFields.Count() == overrideValues.Count())
            {
                // Overrides
                var overridesElement = referenceDataRequest["overrides"];
                var zippedArray = overrideFields.Zip(overrideValues, (t, u) => new { Field = t, Value = u });
                foreach (var arrayitem in zippedArray)
                {
                    var ovrrdeElement = overridesElement.AppendElement();
                    ovrrdeElement.SetElement("fieldId", arrayitem.Field);
                    ovrrdeElement.SetElement("value", arrayitem.Value);
                }

            }

            //   Send off request
            _session.SendRequest(referenceDataRequest, null);

            //   Start with our flag set to False for not done
            var done = false;

            //   Continue as long as we are not done
            while (!done)
            {
                //   Retrieve next event from the server
                var eventObj = _session.NextEvent();

                //   As long as we have a partial or final response, start to process data
                if (eventObj.Type == Event.EventType.RESPONSE ||
                    eventObj.Type == Event.EventType.PARTIAL_RESPONSE)
                {
                    //  Loop through messages
                    foreach (Message msg in eventObj)
                    {
                        //   Error handler in case of problem which throws meaningful exception
                        if (msg.AsElement.HasElement("responseError"))
                            throw new Exception("Response error:  " + msg.GetElement("responseError").GetElement("message"));

                        //   Extract the securityData top layer and the field data
                        //   History comes back on a single security basis so no looping there
                        var securityDataArray = msg.GetElement("securityData");

                        //   Loop through each security
                        for (var i = 0; i < securityDataArray.NumValues; i++)
                        {
                            //   First take out the security object...
                            var security = securityDataArray.GetValueAsElement(i);

                            var securityName = security.GetElementAsString("security");

                            //   ... then extract the fieldData object
                            var fieldData = security.GetElement("fieldData");

                            //If we need to add a new security to the securitiesFields dictionary then do so
                            Dictionary<string, object> results = null;
                            if (!securitiesFields.ContainsKey(securityName))
                                securitiesFields.Add(securityName, new Dictionary<string, object>());

                            //Get the fieldsByDate dictionary from the securitiesFields dictionary
                            results = securitiesFields[securityName];

                            //Extract results and store in results dictionary
                            foreach (var dataElement in fieldData.Elements)
                            {
                                var dataElementName = dataElement.Name.ToString();

                                //Not using this at present - just demonstrating that we can
                                switch (dataElement.Datatype)
                                {
                                    //Special handling to co-erce bloomberg datetimes back to standard .NET datetimes
                                    case Schema.Datatype.DATE:
                                        results.Add(dataElementName, dataElement.GetValueAsDate().ToSystemDateTime());
                                        break;
                                    case Schema.Datatype.DATETIME:
                                        results.Add(dataElementName, dataElement.GetValueAsDatetime().ToSystemDateTime());
                                        break;
                                    case Schema.Datatype.TIME:
                                        results.Add(dataElementName, dataElement.GetValueAsDatetime().ToSystemDateTime());
                                        break;
                                    case Schema.Datatype.SEQUENCE:
                                        results.Add(dataElementName, ProcessBulkData(dataElementName, dataElement));
                                        break;
                                    //Standard handling
                                    default:
                                        results.Add(dataElementName, dataElement.GetValue());
                                        break;
                                }
                            }
                        }
                    }

                    //   Once we have a response we are done
                    if (eventObj.Type == Event.EventType.RESPONSE) done = true;
                }
            }

            return securitiesFields;
        }

        private object ProcessBulkData(string security, Element data)
        {
            IList<object> bulkDataItems = new List<object>();
            for (int i = 0; i < data.NumValues; i++)
            {
                List<string> array = new List<string>();
                foreach (Element el in data.GetValueAsElement(i).Elements)
                {
                    array.Add(el.GetValueAsString());
                }
                bulkDataItems.Add(array);
            }
            return bulkDataItems;
        }

        #endregion
    }
}
