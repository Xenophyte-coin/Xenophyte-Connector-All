using System.Collections.Generic;
using System.Net;

namespace Xenophyte_Connector_All.RPC.Token
{
    public class ClassTokenRemoteNode
    {
        public List<IPAddress> remote_node_list = new List<IPAddress>();
        public string seed_node_version = string.Empty;
    }
}
