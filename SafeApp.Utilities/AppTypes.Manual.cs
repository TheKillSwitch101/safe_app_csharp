using System;

namespace SafeApp.Utilities {
  public abstract class IpcMsg { }

  public class AuthIpcMsg : IpcMsg {
    public AuthGranted AuthGranted;
    public uint ReqId;

    public AuthIpcMsg(uint reqId, AuthGranted authGranted) {
      ReqId = reqId;
      AuthGranted = authGranted;
    }
  }

  public class UnregisteredIpcMsg : IpcMsg {
    public uint ReqId;
    public byte[] SerialisedCfg;

    public UnregisteredIpcMsg(uint reqId, IntPtr serialisedCfgPtr, IntPtr serialisedCfgLen) {
      ReqId = reqId;
      SerialisedCfg = BindingUtils.CopyToByteArray(serialisedCfgPtr, serialisedCfgLen);
    }
  }

  public class ContainersIpcMsg : IpcMsg {
    public uint ReqId;

    public ContainersIpcMsg(uint reqId) {
      ReqId = reqId;
    }
  }

  public class ShareMdataIpcMsg : IpcMsg {
    public uint ReqId;

    public ShareMdataIpcMsg(uint reqId) {
      ReqId = reqId;
    }
  }

  public class RevokedIpcMsg : IpcMsg { }

  public class IpcMsgException : FfiException {
    public readonly uint ReqId;

    public IpcMsgException(uint reqId, int code, string description) : base(code, description) {
      ReqId = reqId;
    }
  }
}
