using System;
using System.Collections.Generic;
using System.Text;
using Fiddler;

namespace AutoResponderExt
{
    class ResponderRuleExt
    {
        public byte[] arrResponseBytes;
        public bool bEnabled = true;
        public int iLatencyMS;
        public HTTPResponseHeaders oResponseHeaders;
        public string strDescription;
        public string strMatch;

    }
}
