using UnityEngine;
using System.Collections;

namespace com.zachary625.unity_utility
{
    public class CodeLog
    {
        private CodeLog() { }

        private static CodeLog _I;
        public static CodeLog I {
            get {
                if (_I == null)
                {
                    _I = new CodeLog();
                }
                return _I;
            }
        }

        public interface ILogTimer
        {
            public System.DateTime Time;
        }

        public delegate void LogDelegate(string);
        public delegate void Code_Void();
        public delegate T Code_Return<T>();

        public class LogOptions
        {
            public string CodeName;
            public bool LogBounds;
            public bool LogTime;
            public bool LogDuration;
        }

        public enum EntryType {
            None,
            Inbound,
            Outbound,
            Duration,
        }

        public class Entry {
            public string CodeName;
            public EntryType Type;
            public System.DateTime Time;
            public System.TimeSpan Duration;
        }

        public void Log(LogOptions options, Code_Void code)
        {
        }

        public T Log<T>(LogOptions options, Code_Return<T> code)
        {
            return default(T);
        }

    }
}

