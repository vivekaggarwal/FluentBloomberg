using BloombergApi;
using System.Collections.Generic;
using System.Linq;

namespace FluentBloomberg
{
    public class FluentBloomberg : IFluentBloomberg
    {
        private List<string> _securities;
        private List<string> _fields;
        private Dictionary<string, string> _overrides;

        public IBloombergService Service { get; set; }

        #region Constructors

        public FluentBloomberg(string serverHost = "localhost", int serverPort = 8194) : this(new BloombergService(serverHost, serverPort))
        {

        }

        public FluentBloomberg(IBloombergService _service)
        {
            this.Service = _service;
            this._fields = new List<string>();
            this._securities = new List<string>();
            this._overrides = new Dictionary<string, string>();
        }
        #endregion

        public IFluentBloomberg Security(string security)
        {
            this._securities.Add(security);
            return this;
        }

        public IFluentBloomberg Securities(string securities)
        {
            var splitValues = securities.Split(',').ToList();
            if (splitValues.Count > 0)
            {
                this._securities.AddRange(splitValues);
            }
            return this;
        }

        public IFluentBloomberg Field(string field)
        {
            this._fields.Add(field);
            return this;
        }

        public IFluentBloomberg Fields(string fields)
        {
            var splitValues = fields.Split(',').ToList();
            if (splitValues.Count > 0)
            {
                this._fields.AddRange(splitValues);
            }
            return this;
        }

        public IFluentBloomberg Override(string overrideField, string overrideValue)
        {
            _overrides.Add(overrideField, overrideValue);
            return this;
        }

        public Dictionary<string, Dictionary<string, object>> Lookup()
        {
            if (_overrides.Count > 0)
            {
                return Service.Lookup(_securities.ToArray(), _fields.ToArray(), _overrides.Keys.ToArray(), _overrides.Values.ToArray());
            }
            else 
            {
                return Service.Lookup(_securities.ToArray(), _fields.ToArray());
            }
        }

        public void Dispose()
        {
            Service.Dispose();
        }
    }
}
