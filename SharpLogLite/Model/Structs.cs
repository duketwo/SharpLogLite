using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpLogLite.Model
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct TextMessage
    {
        [FieldOffset(0), MarshalAs(UnmanagedType.U8)]
        public UInt64 Timestamp;
        [FieldOffset(8), MarshalAs(UnmanagedType.U4)]
        public LogSeverity Severity;
        [FieldOffset(12), MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string Module;
        [FieldOffset(44), MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string Channel;
        [FieldOffset(76), MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Message;

        public override string ToString()
        {
            return String.Format("{0} {1} {2} {3} {4}", this.Timestamp, this.Severity, this.Module, this.Channel, this.Message);
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct ConnectionMessage
    {
        [FieldOffset(0)]
        public UInt32 Version;
        [FieldOffset(8)]
        public UInt32 Pid;

        public override string ToString()
        {
            return String.Format("{0} {1}", this.Version, this.Pid);
        }
    }


    [StructLayout(LayoutKind.Explicit, Pack = 0)]
    public struct RawLogMessage
    {
        [FieldOffset(0), MarshalAs(UnmanagedType.U4)]
        public MessageType Type;
        [FieldOffset(8)]
        public TextMessage TextMessage;
        [FieldOffset(8)]
        public ConnectionMessage ConnectionMessage;
    }

    public struct SharpLogMessage
    {
        public DateTime DateTime;
        public LogSeverity Severity;
        public string Module;
        public string Channel;
        public string Message;

        public SharpLogMessage(DateTime dateTime, LogSeverity severity, string module, string channel, string message)
        {
            this.DateTime = dateTime;
            this.Severity = severity;
            this.Module = module;
            this.Channel = channel;
            this.Message = message;
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2} {3} {4}", this.DateTime, this.Severity, this.Module, this.Channel, this.Message);
        }
    }

    public class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 344;
        public byte[] buffer = new byte[BufferSize];
        public SharpLogMessage sharpLogMessage;
    }

}
