using BloombergApi;
using System;
using System.Collections.Generic;

namespace FluentBloomberg
{
    public interface IFluentBloomberg : IDisposable
    {
        IBloombergService Service { get; set; }
        IFluentBloomberg Security(string security);
        IFluentBloomberg Securities(string securities);
        IFluentBloomberg Field(string field);
        IFluentBloomberg Fields(string fields);
        IFluentBloomberg Override(string overrideField, string overrideValue);
        Dictionary<string, Dictionary<string, object>> Lookup();
    }
}
