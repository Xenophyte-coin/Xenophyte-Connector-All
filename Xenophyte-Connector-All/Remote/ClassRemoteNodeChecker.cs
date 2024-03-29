﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xenophyte_Connector.Remote;
using Xenophyte_Connector_All.Setting;
using Xenophyte_Connector_All.Utils;

namespace Xenophyte_Connector_All.Remote
{
    public class ClassRemoteNodeChecker
    {
        public static List<Tuple<IPAddress, int>> ListRemoteNodeChecked = new List<Tuple<IPAddress, int>>();
        public static bool InCheckingNewRemoteNode;


        /// <summary>
        ///     Permit to check new remote node host.
        /// </summary>
        /// <param name="ip"></param>
        public static async Task<string> CheckNewRemoteNodeHostAsync(IPAddress ip)
        {
            InCheckingNewRemoteNode = true;
            var statusPing = CheckPing.CheckPingHost(ip);
            if (statusPing != -1)
            {
#if DEBUG
                Console.WriteLine("From Xenophyte-Connector-All: ping status: " + ip + " " + statusPing + " ms.");
#endif
                var statusTcp = await CheckTcp.CheckTcpClientAsync(ip, ClassConnectorSetting.RemoteNodePort);
                if (statusTcp)
                {
                    if (ListRemoteNodeChecked.Count > 0)
                    {
                        var exist = false;
                        for (var i = 0; i < ListRemoteNodeChecked.Count - 1; i++)
                            if (ListRemoteNodeChecked[i].Item1 == ip)
                                exist = true;
                        if (!exist)
                        {
                            ListRemoteNodeChecked.Add(new Tuple<IPAddress, int>(ip, statusPing));
                            return ClassRemoteNodeStatus.StatusNew;
                        }

                        return ClassRemoteNodeStatus.StatusAlive;
                    }

                    ListRemoteNodeChecked.Add(new Tuple<IPAddress, int>(ip, statusPing));
                    return ClassRemoteNodeStatus.StatusNew;
                }
            }

            InCheckingNewRemoteNode = false;
            return ClassRemoteNodeStatus.StatusDead;
        }

        /// <summary>
        ///     Check each remote node of the list.
        /// </summary>
        public static async System.Threading.Tasks.Task CheckListRemoteNodeHostAsync()
        {
            for (var i = 0; i < ListRemoteNodeChecked.Count - 1; i++)
            {
                var nodeStatus = await CheckNewRemoteNodeHostAsync(ListRemoteNodeChecked[i].Item1);
#if DEBUG
                Console.WriteLine("Status Remote Node Host: " + ListRemoteNodeChecked[i] + " is " +
                                ClassRemoteNodeStatus.StatusDead);
#endif
                if (nodeStatus == ClassRemoteNodeStatus.StatusDead)
                    ListRemoteNodeChecked.Remove(ListRemoteNodeChecked[i]);
            }
        }

        /// <summary>
        ///     Check if remote node host already exist in the checked list.
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool CheckRemoteNodeHostExist(IPAddress ip)
        {
            for (var i = 0; i < ListRemoteNodeChecked.Count - 1; i++)
                if (ListRemoteNodeChecked[i].Item1 == ip)
                    return true;
            return false;
        }

        /// <summary>
        ///     Clean up the list of remote node.
        /// </summary>
        public static void CleanUpRemoteNodeHost()
        {
            ListRemoteNodeChecked.Clear();
        }
    }
}