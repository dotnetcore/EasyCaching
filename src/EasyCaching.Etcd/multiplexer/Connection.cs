// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Etcdserverpb;

using V3Lockpb;

namespace dotnet_etcd.multiplexer
{
    internal class Connection
    {
        internal KV.KVClient _kvClient;

        internal Watch.WatchClient _watchClient;

        internal Lease.LeaseClient _leaseClient;

        internal Lock.LockClient _lockClient;

        internal Cluster.ClusterClient _clusterClient;

        internal Maintenance.MaintenanceClient _maintenanceClient;

        internal Auth.AuthClient _authClient;
    }
}
