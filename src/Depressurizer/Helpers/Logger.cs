﻿using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Depressurizer.Core.Enums;

namespace Depressurizer.Helpers
{
    /// <summary>
    ///     Logger Controller
    /// </summary>
    public sealed class Logger : IDisposable
    {
        #region Static Fields

        private static readonly ConcurrentQueue<string> LogQueue = new ConcurrentQueue<string>();

        private static readonly object SyncRoot = new object();

        private static volatile Logger _instance;

        #endregion

        #region Fields

        private readonly FileStream _outputStream;

        #endregion

        #region Constructors and Destructors

        private Logger()
        {
            _outputStream = new FileStream(Locations.File.Log, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Logger Instance
        /// </summary>
        public static Logger Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                lock (SyncRoot)
                {
                    if (_instance == null)
                    {
                        _instance = new Logger();
                    }
                }

                return _instance;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Generates a logging event at the Debug level using the message.
        /// </summary>
        /// <param name="logMessage">The message to log.</param>
        public void Debug(string logMessage)
        {
            Write(LogLevel.Debug, logMessage);
        }

        /// <summary>
        ///     Generates a logging event at the Debug level using the message, using the provided objects to format.
        /// </summary>
        /// <param name="logMessage">The message to log.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Debug(string logMessage, params object[] args)
        {
            Write(LogLevel.Debug, logMessage, args);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            lock (SyncRoot)
            {
                FlushLog();

                byte[] output = new UTF8Encoding().GetBytes(Environment.NewLine);
                _outputStream.Write(output, 0, output.Length);

                _outputStream?.Dispose();
                _instance = null;
            }
        }

        /// <summary>
        ///     Generates a logging event at the Error level using the message.
        /// </summary>
        /// <param name="logMessage">The message to log.</param>
        public void Error(string logMessage)
        {
            Write(LogLevel.Error, logMessage);
        }

        /// <summary>
        ///     Generates a logging event at the Error level using the message, using the provided objects to format.
        /// </summary>
        /// <param name="logMessage">The message to log.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Error(string logMessage, params object[] args)
        {
            Write(LogLevel.Error, logMessage, args);
        }

        /// <summary>
        ///     Generates a logging event at the Error level using the message and exception.
        /// </summary>
        /// <param name="logMessage">The message to log.</param>
        /// <param name="e">The exception to log, including its stack trace.</param>
        public void Exception(string logMessage, Exception e)
        {
            Write(LogLevel.Error, logMessage + Environment.NewLine + e);
        }

        /// <summary>
        ///     Generates a logging event for the specified level using the exception.
        /// </summary>
        /// <param name="e">The exception to log, including its stack trace.</param>
        public void Exception(Exception e)
        {
            Write(LogLevel.Error, e.ToString());
        }

        /// <summary>
        ///     Generates a logging event at the Info level using the message.
        /// </summary>
        /// <param name="logMessage">The message to log.</param>
        public void Info(string logMessage)
        {
            Write(LogLevel.Info, logMessage);
        }

        /// <summary>
        ///     Generates a logging event at the Info level using the message, using the provided objects to format.
        /// </summary>
        /// <param name="logMessage">The message to log.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Info(string logMessage, params object[] args)
        {
            Write(LogLevel.Info, logMessage, args);
        }

        /// <summary>
        ///     Generates a logging event at the Verbose level using the message.
        /// </summary>
        /// <param name="logMessage">The message to log.</param>
        public void Verbose(string logMessage)
        {
            Write(LogLevel.Verbose, logMessage);
        }

        /// <summary>
        ///     Generates a logging event at the Verbose level using the message, using the provided objects to format.
        /// </summary>
        /// <param name="logMessage">The message to log.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Verbose(string logMessage, params object[] args)
        {
            Write(LogLevel.Verbose, logMessage, args);
        }

        /// <summary>
        ///     Generates a logging event at the Warn level using the message.
        /// </summary>
        /// <param name="logMessage">The message to log.</param>
        public void Warn(string logMessage)
        {
            Write(LogLevel.Warn, logMessage);
        }

        /// <summary>
        ///     Generates a logging event at the Warn level using the message, using the provided objects to format.
        /// </summary>
        /// <param name="logMessage">The message to log.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Warn(string logMessage, params object[] args)
        {
            Write(LogLevel.Warn, logMessage, args);
        }

        #endregion

        #region Methods

        private void FlushLog()
        {
            lock (SyncRoot)
            {
                while (LogQueue.Count > 0)
                {
                    LogQueue.TryDequeue(out string logEntry);
                    byte[] output = new UTF8Encoding().GetBytes(logEntry + Environment.NewLine);
                    _outputStream.Write(output, 0, output.Length);
                }
            }
        }

        private void Write(LogLevel logLevel, string logMessage, params object[] args)
        {
            Write(logLevel, string.Format(CultureInfo.InvariantCulture, logMessage, args));
        }

        private void Write(LogLevel logLevel, string logMessage)
        {
            lock (SyncRoot)
            {
                if (logLevel == LogLevel.Verbose)
                {
                    return;
                }

                string logEntry = string.Format(CultureInfo.InvariantCulture, "{0} {1,-7} | {2}", DateTime.Now, logLevel, logMessage);
                LogQueue.Enqueue(logEntry);

                Task.Run(() => System.Diagnostics.Debug.WriteLine(logEntry));

                if (LogQueue.Count >= 100)
                {
                    FlushLog();
                }
            }
        }

        #endregion
    }
}
