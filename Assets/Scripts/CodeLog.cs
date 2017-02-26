using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace com.zachary625.unity_utility
{
    /// <summary>
    /// Profile for a piece of code.
    /// </summary>
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

        /// <summary>
        /// Delegate for the CodeLog to gain current DateTime.
        /// The inbound/outbound timestamp and duration calculation is based on this delegate.
        /// By default, the delegate uses System.DateTime.Now
        /// </summary>
        public Time_Delegate TimeDelegate
        {
            get
            {
                return _TimeDelegate;
            }

            set
            {
                _TimeDelegate = value;
            }
        }

        /// <summary>
        /// Delegate for the CodeLog to convert a log entry to a string
        /// </summary>
        public Format_Delegate FormatDelegate
        {
            get
            {
                return _FormatDelegate;
            }

            set
            {
                _FormatDelegate = value;
            }
        }

        /// <summary>
        /// Delegate for the CodeLog to output the formatted log string
        /// By default, the delegate uses Debug.Log
        /// </summary>
        public Log_Delegate LogDelegate
        {
            get
            {
                return _LogDelegate;
            }

            set
            {
                _LogDelegate = value;
            }
        }

        /// <summary>
        /// Current options for current CodeLog instance.
        /// Temporary overriding these options are done by passing a LogOptions instance during the Log() call.
        /// </summary>
        public LogOptions Options
        {
            get
            {
                return _Options;
            }

            set
            {
                _Options = value;
            }
        }

        /// <summary>
        /// Delegate for filtering logs
        /// </summary>
        public Predicate_Delegate PredicateDelegate
        {
            get
            {
                return _PredicateDelegate;
            }

            set
            {
                _PredicateDelegate = value;
            }
        }

        public delegate bool Predicate_Delegate();
        public delegate DateTime Time_Delegate();
        public delegate void Log_Delegate(string logContent);
        public delegate string Format_Delegate(Entry entry);

        public delegate void Code_Void();
        public delegate T Code_Return<T>();

        public class LogOptions
        {
            public string Name;
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
            public string Name;
            public EntryType Type;
            public DateTime? Time;
            public TimeSpan? Duration;
        }

        private LogOptions _Options = new LogOptions();
        private Stack<LogOptions> _OptionsStack = new Stack<LogOptions>();

        private Predicate_Delegate _PredicateDelegate = () =>
        {
            return true;
        };

        private Time_Delegate _TimeDelegate = () =>
        {
            return DateTime.Now;
        };

        private Format_Delegate _FormatDelegate = (entry) =>
        {
            string result = "";
            switch (entry.Type)
            {
                case EntryType.Inbound:
                    {
                        result = "+ " + entry.Name;
                        if (entry.Time.HasValue)
                        {
                            result += " @ " + entry.Time.ToString() + "." + entry.Time.Value.Millisecond;
                        }
                        break;
                    }
                case EntryType.Outbound:
                    {
                        result = "- " + entry.Name;
                        if (entry.Time.HasValue)
                        {
                            result += " @ " + entry.Time.ToString()+"."+entry.Time.Value.Millisecond;
                        }
                        break;
                    }
                case EntryType.Duration:
                    {
                        result = "@ " + entry.Name;
                        if (entry.Duration.HasValue)
                        {
                            result += " : " + entry.Duration.Value.TotalMilliseconds + " ms";
                        }
                        break;
                    }
            }
            return result;
        };

        private Log_Delegate _LogDelegate = (text) =>
        {
            Debug.Log(text);
        };

        public void Log(Code_Void code, LogOptions options = null)
        {
            if (code == null)
            {
                return;
            }
            if (options == null)
            {
                options = this.Options;
                if (options == null)
                {
                    code();
                    return;
                }
            }
            if (FormatDelegate == null || LogDelegate == null)
            {
                code();
                return;
            }
            if (PredicateDelegate != null)
            {
                if (!PredicateDelegate())
                {
                    code();
                    return;
                }
            }

            DateTime? tick = null, tock = null;
            if (options.LogTime || options.LogDuration)
            {
                if (TimeDelegate != null)
                {
                    tick = TimeDelegate();
                }
            }

            if (options.LogBounds)
            {
                LogDelegate(FormatDelegate(new Entry()
                {
                    Type = EntryType.Inbound,
                    Name = options.Name,
                    Time = tick,
                }));
            }

            code();

            if (options.LogTime || options.LogDuration)
            {
                if (TimeDelegate != null)
                {
                    tock = TimeDelegate();
                }
            }
            if (options.LogBounds)
            {
                LogDelegate(FormatDelegate(new Entry()
                {
                    Type = EntryType.Outbound,
                    Name = options.Name,
                    Time = tock,
                }));
            }


            if (options.LogDuration)
            {
                if (tick.HasValue && tock.HasValue)
                {
                    LogDelegate(FormatDelegate(new Entry()
                    {
                        Type = EntryType.Duration,
                        Name = options.Name,
                        Duration = tock.Value - tick.Value,
                    }));
                }
            }

            return;
        }

        public T Log<T>(Code_Return<T> code, LogOptions options = null)
        {
            if (code == null)
            {
                return default(T);
            }
            if(options == null)
            {
                options = this.Options;
                if (options == null)
                {
                    return code();
                }
            }
            if (FormatDelegate == null || LogDelegate == null)
            {
                return code();
            }
            if (PredicateDelegate != null)
            {
                if (!PredicateDelegate())
                {
                    return code();
                }
            }

            T result = default(T);
            DateTime? tick = null, tock = null;
            if (options.LogTime || options.LogDuration)
            {
                if (TimeDelegate != null)
                {
                    tick = TimeDelegate();
                }
            }

            if (options.LogBounds)
            {
                LogDelegate(FormatDelegate(new Entry()
                {
                    Type = EntryType.Inbound,
                    Name = options.Name,
                    Time = tick,
                }));
            }

            result = code();

            if (options.LogTime || options.LogDuration)
            {
                if (TimeDelegate != null)
                {
                    tock = TimeDelegate();
                }
            }
            if (options.LogBounds)
            {
                LogDelegate(FormatDelegate(new Entry()
                {
                    Type = EntryType.Outbound,
                    Name = options.Name,
                    Time = tock,
                }));
            }


            if (options.LogDuration)
            {
                if (tick.HasValue && tock.HasValue)
                {
                    LogDelegate(FormatDelegate(new Entry()
                    {
                        Type = EntryType.Duration,
                        Name = options.Name,
                        Duration = tock.Value - tick.Value,
                    }));
                }
            }

            return result;
        }

    }
}

