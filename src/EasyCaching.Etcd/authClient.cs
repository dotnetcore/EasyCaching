// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;

using Etcdserverpb;

namespace dotnet_etcd
{
    public partial class EtcdClient
    {
        /// <summary>
        /// Authenticate processes an authenticate request.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthenticateResponse Authenticate(AuthenticateRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._authClient
                                                                            .Authenticate(request, headers, deadline, cancellationToken));

        /// <summary>
        /// Authenticate processes an authenticate request.
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._authClient
                                                                            .AuthenticateAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// AuthEnable enables authentication
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthEnableResponse AuthEnable(AuthEnableRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._authClient
                                                                            .AuthEnable(request, headers, deadline, cancellationToken));

        /// <summary>
        /// AuthEnableAsync enables authentication in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthEnableResponse> AuthEnableAsync(AuthEnableRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._authClient
                                                                            .AuthEnableAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// AuthDisable disables authentication
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthDisableResponse AuthDisable(AuthDisableRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._authClient
                                                                            .AuthDisable(request, headers, deadline, cancellationToken));

        /// <summary>
        /// AuthDisableAsync disables authentication in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthDisableResponse> AuthDisableAsync(AuthDisableRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._authClient
                                                                            .AuthDisableAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// UserAdd adds a new user
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthUserAddResponse UserAdd(AuthUserAddRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._authClient
                                                                            .UserAdd(request, headers, deadline, cancellationToken));

        /// <summary>
        /// UserAddAsync adds a new user in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthUserAddResponse> UserAddAsync(AuthUserAddRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._authClient
                                                                            .UserAddAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// UserGet gets detailed user information
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthUserGetResponse UserGet(AuthUserGetRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._authClient
                                                                            .UserGet(request, headers, deadline, cancellationToken));

        /// <summary>
        /// UserGetAsync gets detailed user information in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthUserGetResponse> UserGetAsync(AuthUserGetRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._authClient
                                                                            .UserGetAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// UserList gets a list of all users
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthUserListResponse UserList(AuthUserListRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._authClient
                                                                            .UserList(request, headers, deadline, cancellationToken));

        /// <summary>
        /// UserListAsync gets a list of all users in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthUserListResponse> UserListAsync(AuthUserListRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._authClient
                                                                            .UserListAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// UserDelete deletes a specified user
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthUserDeleteResponse UserDelete(AuthUserDeleteRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._authClient
                                                                            .UserDelete(request, headers, deadline, cancellationToken));

        /// <summary>
        /// UserDeleteAsync deletes a specified user in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthUserDeleteResponse> UserDeleteAsync(AuthUserDeleteRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._authClient
                                                                            .UserDeleteAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// UserChangePassword changes the password of a specified user
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthUserChangePasswordResponse UserChangePassword(AuthUserChangePasswordRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._authClient
                                                                            .UserChangePassword(request, headers, deadline, cancellationToken));

        /// <summary>
        /// UserChangePasswordAsync changes the password of a specified user in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthUserChangePasswordResponse> UserChangePasswordAsync(AuthUserChangePasswordRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._authClient
                                                                            .UserChangePasswordAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// UserGrant grants a role to a specified user
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthUserGrantRoleResponse UserGrantRole(AuthUserGrantRoleRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._authClient
                                                                            .UserGrantRole(request, headers, deadline, cancellationToken));

        /// <summary>
        /// UserGrantRoleAsync grants a role to a specified user in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthUserGrantRoleResponse> UserGrantRoleAsync(AuthUserGrantRoleRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._authClient
                                                                            .UserGrantRoleAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// UserRevokeRole revokes a role of specified user
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthUserRevokeRoleResponse UserRevokeRole(AuthUserRevokeRoleRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._authClient
                                                                            .UserRevokeRole(request, headers, deadline, cancellationToken));

        /// <summary>
        /// UserRevokeRoleAsync revokes a role of specified user in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthUserRevokeRoleResponse> UserRevokeRoleAsync(AuthUserRevokeRoleRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._authClient
                                                                            .UserRevokeRoleAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// RoleAdd adds a new role
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthRoleAddResponse RoleAdd(AuthRoleAddRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._authClient
                                                                            .RoleAdd(request, headers, deadline, cancellationToken));

        /// <summary>
        /// RoleAddAsync adds a new role in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthRoleAddResponse> RoleAddAsync(AuthRoleAddRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._authClient
                                                                            .RoleAddAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// RoleGet gets detailed role information
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthRoleGetResponse RoleGet(AuthRoleGetRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._authClient
                                                                            .RoleGet(request, headers, deadline, cancellationToken));

        /// <summary>
        /// RoleGetASync gets detailed role information in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthRoleGetResponse> RoleGetASync(AuthRoleGetRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._authClient
                                                                            .RoleGetAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// RoleList gets lists of all roles
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthRoleListResponse RoleList(AuthRoleListRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._authClient
                                                                            .RoleList(request, headers, deadline, cancellationToken));

        /// <summary>
        /// RoleListAsync gets lists of all roles async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthRoleListResponse> RoleListAsync(AuthRoleListRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._authClient
                                                                            .RoleListAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// RoleDelete deletes a specified role
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthRoleDeleteResponse RoleDelete(AuthRoleDeleteRequest request, Grpc.Core.Metadata headers = null,
            DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._authClient
                                                                            .RoleDelete(request, headers, deadline, cancellationToken));

        /// <summary>
        /// RoleDeleteAsync deletes a specified role in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthRoleDeleteResponse> RoleDeleteAsync(AuthRoleDeleteRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._authClient
                                                                            .RoleDeleteAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// RoleGrantPermission grants a permission of a specified key or range to a specified role
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthRoleGrantPermissionResponse RoleGrantPermission(AuthRoleGrantPermissionRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._authClient
                                                                            .RoleGrantPermission(request, headers, deadline, cancellationToken));

        /// <summary>
        /// RoleGrantPermissionAsync grants a permission of a specified key or range to a specified role in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthRoleGrantPermissionResponse> RoleGrantPermissionAsync(
            AuthRoleGrantPermissionRequest request, Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._authClient
                                                                            .RoleGrantPermissionAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);

        /// <summary>
        /// RoleRevokePermission revokes a key or range permission of a specified role
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public AuthRoleRevokePermissionResponse RoleRevokePermission(AuthRoleRevokePermissionRequest request,
            Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => CallEtcd((connection) => connection._authClient
                                                                            .RoleRevokePermission(request, headers, deadline, cancellationToken));

        /// <summary>
        /// RoleRevokePermissionAsync revokes a key or range permission of a specified role in async
        /// </summary>
        /// <param name="request">The request to send to the server.</param>
        /// <param name="headers">The initial metadata to send with the call. This parameter is optional.</param>
        /// <param name="deadline">An optional deadline for the call. The call will be cancelled if deadline is hit.</param>
        /// <param name="cancellationToken">An optional token for canceling the call.</param>
        /// <returns>The response received from the server.</returns>
        public async Task<AuthRoleRevokePermissionResponse> RoleRevokePermissionAsync(
            AuthRoleRevokePermissionRequest request, Grpc.Core.Metadata headers = null, DateTime? deadline = null,
            CancellationToken cancellationToken = default) => await CallEtcdAsync(async (connection) => await connection._authClient
                                                                            .RoleRevokePermissionAsync(request, headers, deadline, cancellationToken)).ConfigureAwait(false);
    }
}
