using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace com.zachary625.unity_utility
{

    public delegate bool Predicate_Delegate();
    public delegate DateTime Time_Delegate();
    public delegate string Format_Delegate(CodeLogEntry entry);
    public delegate void Log_Delegate(string logContent);

    public class CodeLogOptions
    {
        /// <summary>
        /// Identifier for current log
        /// </summary>
        public string Identifier;
        public bool LogBounds;
        public bool LogTime;
        public bool LogDuration;
        public bool LogStack;
         
        /// <summary>
        /// Delegate for filtering logs
        /// </summary>
        public Predicate_Delegate PredicateDelegate;
        public Format_Delegate FormatDelegate;
        public Log_Delegate LogDelegate;

    }


    public enum CodeLogEntryType
    {
        None,
        Inbound,
        Outbound,
        Duration,
    }

    public class CodeLogEntry
    {
        public string Identifier;
        public CodeLogEntryType Type;
        public DateTime? Time;
        public TimeSpan? Duration;
    }
}
