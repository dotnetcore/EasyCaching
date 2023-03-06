// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;

using Etcdserverpb;

using Grpc.Core;

namespace dotnet_etcd
{
    public partial class EtcdClient
    {
        /// <summary>
        ///  Alarm activates, deactivates, and queries alarms regarding cluster health
        /// </summary>
        /// <param name="request">Alarm request</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>Alarm Response</returns>
        public AlarmResponse Alarm(AlarmRequest request, Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._maintenanceClient
                                                                            .Alarm(request, headers, deadline, cancellationToken));

        /// <summary>
        ///  Alarm activates, deactivates, and queries alarms regarding cluster health in async
        /// </summary>
        /// <param name="request">Alarm request</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>Alarm Response</returns>
        public async Task<AlarmResponse> AlarmAsync(AlarmRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._maintenanceClient
                                                                            .AlarmAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// Status gets the status of the member.
        /// </summary>
        /// <param name="request">Status Request</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>Status response</returns>
        public StatusResponse Status(StatusRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._maintenanceClient
                                                                            .Status(request, headers, deadline, cancellationToken));

        /// <summary>
        /// Status gets the status of the member in async.
        /// </summary>
        /// <param name="request">Status Request</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>Status response</returns>
        public async Task<StatusResponse> StatusASync(StatusRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._maintenanceClient
                                                                            .StatusAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// Defragment defragments a member's backend database to recover storage space.
        /// </summary>
        /// <param name="request">Defragment Request</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>Defragment Response</returns>
        public DefragmentResponse Defragment(DefragmentRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._maintenanceClient
                                                                            .Defragment(request, headers, deadline, cancellationToken));

        /// <summary>
        /// Defragment defragments a member's backend database to recover storage space in async.
        /// </summary>
        /// <param name="request">Defragment Request</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>Defragment Response</returns>
        public async Task<DefragmentResponse> DefragmentAsync(DefragmentRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._maintenanceClient
                                                                            .DefragmentAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// Hash computes the hash of whole backend keyspace,
        /// including key, lease, and other buckets in storage.
        /// This is designed for testing ONLY!
        /// Do not rely on this in production with ongoing transactions,
        /// since Hash operation does not hold MVCC locks.
        /// Use "HashKV" API instead for "key" bucket consistency checks.
        /// </summary>
        /// <param name="request">Hash Request</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>Hash Response</returns>
        public HashResponse Hash(HashRequest request, Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._maintenanceClient
                                                                            .Hash(request, headers, deadline, cancellationToken));

        /// <summary>
        /// Hash computes the hash of whole backend keyspace,
        /// including key, lease, and other buckets in storage in async.
        /// This is designed for testing ONLY!
        /// Do not rely on this in production with ongoing transactions,
        /// since Hash operation does not hold MVCC locks.
        /// Use "HashKV" API instead for "key" bucket consistency checks.
        /// </summary>
        /// <param name="request">Hash Request</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>Hash Response</returns>
        public async Task<HashResponse> HashAsync(HashRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._maintenanceClient
                                                                            .HashAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// HashKV computes the hash of all MVCC keys up to a given revision.
        /// It only iterates "key" bucket in backend storage.
        /// </summary>
        /// <param name="request">HashKV Request</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>HashKV Response</returns>
        public HashKVResponse HashKV(HashKVRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._maintenanceClient
                                                                            .HashKV(request, headers, deadline, cancellationToken));

        /// <summary>
        /// HashKV computes the hash of all MVCC keys up to a given revision in async.
        /// It only iterates "key" bucket in backend storage.
        /// </summary>
        /// <param name="request">HashKV Request</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>HashKV Response</returns>
        public async Task<HashKVResponse> HashKVAsync(HashKVRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._maintenanceClient
                                                                            .HashKVAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// Snapshot sends a snapshot of the entire backend from a member over a stream to a client.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="method"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        public async Task Snapshot(SnapshotRequest request, Action<SnapshotResponse> method,
            CancellationToken cancellationToken, Grpc.Core.Metadata headers = null, DateTime? deadline = null) => await CallEtcdAsync(async (connection) =>
                                                                                                                {
                                                                                                                    using (AsyncServerStreamingCall<SnapshotResponse> snapshotter = connection
                                                                                                                        ._maintenanceClient.Snapshot(request, headers, deadline, cancellationToken))
                                                                                                                    {
                                                                                                                        while (await snapshotter.ResponseStream.MoveNext(cancellationToken).ConfigureAwait(false))
                                                                                                                        {
                                                                                                                            SnapshotResponse update = snapshotter.ResponseStream.Current;
                                                                                                                            method(update);
                                                                                                                        }
                                                                                                                    }
                                                                                                                }).ConfigureAwait(false);

        /// <summary>
        /// Snapshot sends a snapshot of the entire backend from a member over a stream to a client.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="methods"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        public async Task Snapshot(SnapshotRequest request, Action<SnapshotResponse>[] methods,
            CancellationToken cancellationToken, Grpc.Core.Metadata headers = null, DateTime? deadline = null) => await CallEtcdAsync(async (connection) =>
                                                                                                                {
                                                                                                                    using (AsyncServerStreamingCall<SnapshotResponse> snapshotter = connection
                                                                                                                        ._maintenanceClient.Snapshot(request, headers, deadline, cancellationToken))
                                                                                                                    {
                                                                                                                        while (await snapshotter.ResponseStream.MoveNext(cancellationToken).ConfigureAwait(false))
                                                                                                                        {
                                                                                                                            SnapshotResponse update = snapshotter.ResponseStream.Current;
                                                                                                                            foreach (Action<SnapshotResponse> method in methods)
                                                                                                                            {
                                                                                                                                method(update);
                                                                                                                            }
                                                                                                                        }
                                                                                                                    }
                                                                                                                }).ConfigureAwait(false);

        /// <summary>
        /// MoveLeader requests current leader node to transfer its leadership to transferee.
        /// </summary>
        /// <param name="request">MoveLeader Request</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>MoveLeader Response</returns>
        public MoveLeaderResponse MoveLeader(MoveLeaderRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._maintenanceClient
                                                                            .MoveLeader(request, headers, deadline, cancellationToken));

        /// <summary>
        /// MoveLeader requests current leader node to transfer its leadership to transferee in async.
        /// </summary>
        /// <param name="request">MoveLeader Request</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>MoveLeader Response</returns>
        public async Task<MoveLeaderResponse> MoveLeaderAsync(MoveLeaderRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._maintenanceClient
                                                                            .MoveLeaderAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);
    }
}
