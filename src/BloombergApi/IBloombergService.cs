using System;
using System.Collections.Generic;

namespace BloombergApi
{
    public interface IBloombergService : IDisposable
    {
        Dictionary<string, Dictionary<string, object>> Lookup(string security, string field);
        Dictionary<string, Dictionary<string, object>> Lookup(string[] securities, string[] fields);
        Dictionary<string, Dictionary<string, object>> Lookup(string[] securities, string[] fields, string[] overrideFields, string[] overrideValues);
    }
}
