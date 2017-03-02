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
        public delegate void Code_Void();
        public delegate T Code_Return<T>();

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

        private Stack<Format_Delegate> _FormatDelegateStack = new Stack<Format_Delegate>();
        public void PushFormatDelegate(Format_Delegate formatDelegate)
        {
            _FormatDelegateStack.Push(formatDelegate);
        }

        public Format_Delegate PopFormatDelegate()
        {
            if (_FormatDelegateStack.Count > 0)
            {
                return _FormatDelegateStack.Pop();
            }
            else
            {
                return null;
            }
        }

        public Format_Delegate FormatDelegate {
            get {
                if (_FormatDelegateStack.Count > 0)
                {
                    return _FormatDelegateStack.Peek();
                }
                else {
                    return _DefaultFormatDelegate;
                }
            }
        }

        /// <summary>
        /// Delegate for the CodeLog to gain current DateTime.
        /// The inbound/outbound timestamp and duration calculation is based on this delegate.
        /// By default, the delegate uses System.DateTime.Now
        /// </summary>
        public Time_Delegate TimeDelegate = _TimeDelegate;


        private Stack<Log_Delegate> _LogDelegateStack = new Stack<Log_Delegate>();
        public void PushLogDelegate(Log_Delegate logDelegate)
        {
            _LogDelegateStack.Push(logDelegate);
        }

        public Log_Delegate PopLogDelegate()
        {
            if (_LogDelegateStack.Count > 0)
            {
                return _LogDelegateStack.Pop();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Delegate for the CodeLog to output the formatted log string
        /// By default, the delegate uses Debug.Log
        /// </summary>
        public Log_Delegate LogDelegate {
            get
            {
                if (_LogDelegateStack.Count > 0)
                {
                    return _LogDelegateStack.Peek();
                }
                else
                {
                    return _DefaultLogDelegate;
                }
            }
        }

        public Predicate_Delegate PredicateDelegate;

        private static Format_Delegate _DefaultFormatDelegate = (CodeLogEntry entry) =>
        {
            string result = "";
            switch (entry.Type)
            {
                case CodeLogEntryType.Inbound:
                    {
                        result = "+ " + entry.Identifier;
                        if (entry.Time.HasValue)
                        {
                            result += " @ " + entry.Time.ToString() + "." + entry.Time.Value.Millisecond;
                        }
                        break;
                    }
                case CodeLogEntryType.Outbound:
                    {
                        result = "- " + entry.Identifier;
                        if (entry.Time.HasValue)
                        {
                            result += " @ " + entry.Time.ToString() + "." + entry.Time.Value.Millisecond;
                        }
                        break;
                    }
                case CodeLogEntryType.Duration:
                    {
                        result = "@ " + entry.Identifier;
                        if (entry.Duration.HasValue)
                        {
                            result += " : " + entry.Duration.Value.TotalMilliseconds + " ms";
                        }
                        break;
                    }
            }
            return result;
        };

        private static Time_Delegate _TimeDelegate = () =>
        {
            return DateTime.Now;
        };

        private static Log_Delegate _DefaultLogDelegate = (text) =>
        {
            Debug.Log(text);
        };

        public void Log(Code_Void code, CodeLogOptions options = null)
        {
            Log<object>(() => {
                code();
                return null;
            }, options);
        }

        public T Log<T>(Code_Return<T> code, CodeLogOptions options = null)
        {
            if (code == null)
            {
                return default(T);
            }

            if (options == null)
            {
                return code();
            }

            if (options.PredicateDelegate != null)
            {
                if (!options.PredicateDelegate())
                {
                    return code();
                }
            }
            else if (PredicateDelegate != null)
            {
                if (!PredicateDelegate())
                {
                    return code();
                }
            }

            Format_Delegate formatDelegate = options.FormatDelegate;
            if (formatDelegate == null)
            {
                formatDelegate = FormatDelegate;
            }

            Log_Delegate logDelegate = options.LogDelegate;
            if (logDelegate == null)
            {
                logDelegate = LogDelegate;
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
                logDelegate(formatDelegate(new CodeLogEntry()
                {
                    Type = CodeLogEntryType.Inbound,
                    Identifier = options.Identifier,
                    Time = tick,
                }));
            }

            T t = code();

            if (options.LogTime || options.LogDuration)
            {
                if (TimeDelegate != null)
                {
                    tock = TimeDelegate();
                }
            }
            if (options.LogBounds)
            {
                logDelegate(formatDelegate(new CodeLogEntry()
                {
                    Type = CodeLogEntryType.Outbound,
                    Identifier = options.Identifier,
                    Time = tock,
                }));
            }


            if (options.LogDuration)
            {
                if (tick.HasValue && tock.HasValue)
                {
                    logDelegate(formatDelegate(new CodeLogEntry()
                    {
                        Type = CodeLogEntryType.Duration,
                        Identifier = options.Identifier,
                        Duration = tock.Value - tick.Value,
                    }));
                }
            }

            return t;
        }

    }

}

