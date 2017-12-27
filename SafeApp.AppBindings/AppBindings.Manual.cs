#if !NETSTANDARD1_2 || __DESKTOP__
using System;
using System.Threading.Tasks;
using SafeApp.Utilities;

#if __IOS__
using ObjCRuntime;
#endif

namespace SafeApp.AppBindings {
  public partial class AppBindings {
    #region App Creation

    public void AppUnregistered(byte[] bootstrapConfig, Action oDisconnectNotifierCb, Action<FfiResult, IntPtr> oCb) {
      var userData = BindingUtils.ToHandlePtr((oDisconnectNotifierCb, oCb));

      AppUnregisteredNative(bootstrapConfig, (IntPtr)bootstrapConfig.Length, userData, OnAppDisconnectCb, OnAppCreateCb);
    }

    public void AppRegistered(string appId, ref AuthGranted authGranted, Action oDisconnectNotifierCb, Action<FfiResult, IntPtr> oCb) {
      var authGrantedNative = authGranted.ToNative();
      var userData = BindingUtils.ToHandlePtr((oDisconnectNotifierCb, oCb));

      AppRegisteredNative(appId, ref authGrantedNative, userData, OnAppDisconnectCb, OnAppCreateCb);

      authGrantedNative.Free();
    }

#if __IOS__
        [MonoPInvokeCallback(typeof(NoneCb))]
#endif
    private static void OnAppDisconnectCb(IntPtr userData) {
      var (action, _) = BindingUtils.FromHandlePtr<(Action, Action<FfiResult, IntPtr>)>(userData, false);

      action();
    }

#if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultAppCb))]
#endif
    private static void OnAppCreateCb(IntPtr userData, ref FfiResult result, IntPtr app) {
      var (_, action) = BindingUtils.FromHandlePtr<(Action, Action<FfiResult, IntPtr>)>(userData, false);

      action(result, app);
    }

    #endregion

    #region DecodeIpcMsg

    public Task<IpcMsg> DecodeIpcMsgAsync(string msg) {
      var (task, userData) = BindingUtils.PrepareTask<IpcMsg>();
      DecodeIpcMsgNative(
        msg,
        userData,
        OnDecodeIpcMsgAuthCb,
        OnDecodeIpcMsgUnregisteredCb,
        OnDecodeIpcMsgContainersCb,
        OnDecodeIpcMsgShareMdataCb,
        OnDecodeIpcMsgRevokedCb,
        OnDecodeIpcMsgErrCb);

      return task;
    }

#if __IOS__
        [MonoPInvokeCallback(typeof(UintAuthGrantedNativeCb))]
#endif
    private static void OnDecodeIpcMsgAuthCb(IntPtr userData, uint reqId, ref AuthGrantedNative authGranted) {
      var tcs = BindingUtils.FromHandlePtr<TaskCompletionSource<IpcMsg>>(userData);
      tcs.SetResult(new AuthIpcMsg(reqId, new AuthGranted(authGranted)));
    }

#if __IOS__
        [MonoPInvokeCallback(typeof(UintByteListCb))]
#endif
    private static void OnDecodeIpcMsgUnregisteredCb(IntPtr userData, uint reqId, IntPtr serialisedCfgPtr, IntPtr serialisedCfgLen) {
      var tcs = BindingUtils.FromHandlePtr<TaskCompletionSource<IpcMsg>>(userData);
      tcs.SetResult(new UnregisteredIpcMsg(reqId, serialisedCfgPtr, serialisedCfgLen));
    }

#if __IOS__
        [MonoPInvokeCallback(typeof(UintCb))]
#endif
    private static void OnDecodeIpcMsgContainersCb(IntPtr userData, uint reqId) {
      var tcs = BindingUtils.FromHandlePtr<TaskCompletionSource<IpcMsg>>(userData);
      tcs.SetResult(new ContainersIpcMsg(reqId));
    }

#if __IOS__
        [MonoPInvokeCallback(typeof(UintCb))]
#endif
    private static void OnDecodeIpcMsgShareMdataCb(IntPtr userData, uint reqId) {
      var tcs = BindingUtils.FromHandlePtr<TaskCompletionSource<IpcMsg>>(userData);
      tcs.SetResult(new ShareMdataIpcMsg(reqId));
    }

#if __IOS__
        [MonoPInvokeCallback(typeof(NoneCb))]
#endif
    private static void OnDecodeIpcMsgRevokedCb(IntPtr userData) {
      var tcs = BindingUtils.FromHandlePtr<TaskCompletionSource<IpcMsg>>(userData);
      tcs.SetResult(new RevokedIpcMsg());
    }

#if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultUintCb))]
#endif
    private static void OnDecodeIpcMsgErrCb(IntPtr userData, ref FfiResult result, uint reqId) {
      var tcs = BindingUtils.FromHandlePtr<TaskCompletionSource<IpcMsg>>(userData);
      tcs.SetException(new IpcMsgException(reqId, result.ErrorCode, result.Description));
    }

    #endregion
  }
}
#endif
