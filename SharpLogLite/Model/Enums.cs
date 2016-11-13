using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * User: duketwo - https://github.com/duketwo/
 * Date: 10.11.2016
 */

namespace SharpLogLite.Model
{
    public enum MessageType
    {
        CONNECTION_MESSAGE,
        SIMPLE_MESSAGE,
        LARGE_MESSAGE,
        CONTINUATION_MESSAGE,
        CONTINUATION_END_MESSAGE,
    };

    public enum LogSeverity
    {
        SEVERITY_INFO,
        SEVERITY_NOTICE,
        SEVERITY_WARN,
        SEVERITY_ERR,
        SEVERITY_COUNT,
    };

}
