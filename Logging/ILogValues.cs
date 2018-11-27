using System.Collections.Generic;

namespace Expedien.ERP.Common.Logging
{
    public interface ILogValues
    {
        IEnumerable<KeyValuePair<string, object>> GetValues();
    }
}