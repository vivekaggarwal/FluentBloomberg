using System;
using System.Collections.Generic;

namespace BloombergApi
{
    public class BloombergService : IBloombergService
    {
        private BloombergApi _api;

        public BloombergService(string serverHost = "localhost", int serverPort = 8194)
        {
            _api = new BloombergApi(serverHost, serverPort);
        }

        public Dictionary<string, Dictionary<string, object>> Lookup(string security, string field)
        {
            return Lookup(new[] { security }, new[] { field });
        }

        public Dictionary<string, Dictionary<string, object>> Lookup(string[] securities, string[] fields)
        {
            return Lookup(securities, fields, null, null);
        }

        public Dictionary<string, Dictionary<string, object>> Lookup(string[] securities, string[] fields, string[] overrideFields, string[] overrideValues)
        {
            return _api.Data(securities, fields, overrideFields, overrideValues);
        }

        public void Dispose()
        {
            if (_api != null)
            {
                _api.DisposeSessionAndService();
            }
        }
    }
}
