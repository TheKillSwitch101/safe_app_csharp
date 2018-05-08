﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SafeApp.Utilities;

namespace SafeApp.MockAuthBindings {
  // ReSharper disable ConvertToLocalFunction
  // ReSharper disable UnusedMember.Global
  // ReSharper disable MemberCanBePrivate.Global
  public class Authenticator : IDisposable {
    private static readonly IAuthBindings AuthBindings = MockAuthResolver.Current;
    //ReSharper disable once UnassignedField.Global
    public static EventHandler Disconnected;
    private IntPtr _authPtr;
    private GCHandle _disconnectedHandle;
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public bool IsDisconnected { get; private set; }

    private Authenticator() {
      IsDisconnected = true;
      _authPtr = IntPtr.Zero;
    }

    public void Dispose() {
      FreeAuth();
      GC.SuppressFinalize(this);
    }

    public static bool IsMockBuild() {
      return AuthBindings.IsMockBuild();
    }

    public Task<AccountInfo> AuthAccountInfoAsync() {
      return AuthBindings.AuthAccountInfoAsync(_authPtr);
    }

    public Task<List<AppAccess>> AuthAppsAccessingMutableDataAsync(byte[] name, ulong typeTag) {
      return AuthBindings.AuthAppsAccessingMutableDataAsync(_authPtr, name, typeTag);
    }

    public static Task<string> AuthExeFileStemAsync() {
      return AuthBindings.AuthExeFileStemAsync();
    }

    public Task AuthFlushAppRevocationQueueAsync() {
      return AuthBindings.AuthFlushAppRevocationQueueAsync(_authPtr);
    }

    public static Task AuthInitLoggingAsync(string outputFileName) {
      return AuthBindings.AuthInitLoggingAsync(outputFileName);
    }

    public static Task AuthOutputLogPathAsync(string outputFileName) {
      return AuthBindings.AuthOutputLogPathAsync(outputFileName);
    }

    public Task AuthReconnectAsync() {
      return AuthBindings.AuthReconnectAsync(_authPtr);
    }

    public Task<List<RegisteredApp>> AuthRegisteredAppsAsync() {
      return AuthBindings.AuthRegisteredAppsAsync(_authPtr);
    }

    public Task AuthRevokeAppAsync(string appId) {
      return AuthBindings.AuthRevokeAppAsync(_authPtr, appId);
    }

    public Task<List<AppExchangeInfo>> AuthRevokedAppsAsync() {
      return AuthBindings.AuthRevokedAppsAsync(_authPtr);
    }

    public Task AuthRmRevokedAppAsync(string appId) {
      return AuthBindings.AuthRmRevokedAppAsync(_authPtr, appId);
    }

    public static Task AuthSetAdditionalSearchPathAsync(string newPath) {
      return AuthBindings.AuthSetAdditionalSearchPathAsync(newPath);
    }

    public static Task<Authenticator> CreateAccountAsync(string locator, string secret, string invitation) {
      return Task.Run(
        () => {
          var authenticator = new Authenticator();
          var tcs = new TaskCompletionSource<Authenticator>(TaskCreationOptions.RunContinuationsAsynchronously);
          Action disconnect = () => { OnDisconnected(authenticator); };
          Action<FfiResult, IntPtr, GCHandle> cb = (result, ptr, disconnectHandle) => {
            if (result.ErrorCode != 0) {
              if (disconnectHandle.IsAllocated) {
                disconnectHandle.Free();
              }

              tcs.SetException(result.ToException());
              return;
            }

            authenticator.Init(ptr, disconnectHandle);
            tcs.SetResult(authenticator);
          };
          AuthBindings.CreateAccount(locator, secret, invitation, disconnect, cb);
          return tcs.Task;
        });
    }

    public Task<IpcReq> DecodeIpcMessageAsync(string msg) {
      return AuthBindings.DecodeIpcMessage(_authPtr, msg);
    }

    public static Task<IpcReq> UnRegisteredDecodeIpcMsgAsync(string msg) {
      return AuthBindings.UnRegisteredDecodeIpcMsgAsync(msg);
    }

    public Task<string> EncodeAuthRespAsync(AuthIpcReq authIpcReq, bool allow) {
      return AuthBindings.EncodeAuthRespAsync(_authPtr, ref authIpcReq.AuthReq, authIpcReq.ReqId, allow);
    }

    public Task<string> EncodeContainersRespAsync(ContainersIpcReq req, bool allow) {
      return AuthBindings.EncodeContainersRespAsync(_authPtr, ref req.ContainersReq, req.ReqId, allow);
    }

    public Task<string> EncodeShareMdataRespAsync(ShareMDataIpcReq req, bool allow) {
      return AuthBindings.EncodeShareMDataRespAsync(_authPtr, ref req.ShareMDataReq, req.ReqId, allow);
    }

    public static Task<string> EncodeUnregisteredRespAsync(uint reqId, bool allow) {
      return AuthBindings.EncodeUnregisteredRespAsync(reqId, allow);
    }

    ~Authenticator() {
      FreeAuth();
    }

    private void FreeAuth() {
      if (_disconnectedHandle.IsAllocated) {
        _disconnectedHandle.Free();
      }

      if (_authPtr == IntPtr.Zero) {
        return;
      }

      AuthBindings.AuthFree(_authPtr);
      _authPtr = IntPtr.Zero;
    }

    private void Init(IntPtr authPtr, GCHandle disconnectedHandle) {
      _authPtr = authPtr;
      _disconnectedHandle = disconnectedHandle;
      IsDisconnected = false;
    }

    public static Task<Authenticator> LoginAsync(string locator, string secret) {
      return Task.Run(
        () => {
          var authenticator = new Authenticator();
          var tcs = new TaskCompletionSource<Authenticator>(TaskCreationOptions.RunContinuationsAsynchronously);
          Action disconnect = () => { OnDisconnected(authenticator); };
          Action<FfiResult, IntPtr, GCHandle> cb = (result, ptr, disconnectHandle) => {
            if (result.ErrorCode != 0) {
              if (disconnectHandle.IsAllocated) {
                disconnectHandle.Free();
              }

              tcs.SetException(result.ToException());
              return;
            }

            authenticator.Init(ptr, disconnectHandle);
            tcs.SetResult(authenticator);
          };
          AuthBindings.Login(locator, secret, disconnect, cb);
          return tcs.Task;
        });
    }

    private static void OnDisconnected(Authenticator authenticator) {
      authenticator.IsDisconnected = true;
      Disconnected?.Invoke(authenticator, EventArgs.Empty);
    }

    public Task TestSimulateNetworkDisconnectAsync()
    {
      return AuthBindings.TestSimulateNetworkDisconnectAsync(_authPtr);
    }
  }
}
