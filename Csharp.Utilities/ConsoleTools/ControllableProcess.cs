// /* **********************************************************************************
//  *
//  * Copyright (c) Sky Sanders. All rights reserved.
//  * 
//  * This source code is subject to terms and conditions of the Microsoft Public
//  * License (Ms-PL). A copy of the license can be found in the license.htm file
//  * included in this distribution.
//  *
//  * You must not remove this notice, or any other, from this software.
//  *
//  * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Csharp.Utilities.ConsoleTools
{
    /// <summary>
    /// Implements a process that can be monitored and interacted with.
    /// 
    /// StandardOut and StandardError are monitored at character and line level and
    /// made visible by thread safe accessors.
    /// 
    /// StandardIn is writable via SetInput.
    /// 
    /// Is there a standard implementation of this somewhere that I do not know about?
    /// It seems like a common enough requirement for legacy interop.
    /// </summary>
    public class ControllableProcess : IDisposable
    {
        #region Private fields

        private readonly Queue<string> _errorQueue;
        private readonly Queue<string> _inputQueue;
        private readonly Queue<string> _ouputQueue;
        private readonly Process _process;
        private string _currentErrorLine;
        private string _currentOutputLine;

        private bool _disposed;
        private Thread _errorThread;
        private Thread _inputThread;
        private object _outputLock = new object();
        private Thread _outputThread;
        private bool _stopped;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="arguments"></param>
        /// <param name="workingDirectory"></param>
        public ControllableProcess(string filename, string arguments, string workingDirectory)
        {
            _inputQueue = new Queue<string>();
            _ouputQueue = new Queue<string>();
            _errorQueue = new Queue<string>();

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = arguments,
                CreateNoWindow = true,
                ErrorDialog = false,
                FileName = filename,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory
            };

            _process = new Process { StartInfo = startInfo };
        }

        public string CurrentErrorLine
        {
            get
            {
                lock (_outputLock)
                {
                    return _currentErrorLine;
                }
            }
        }

        public string CurrentOutputLine
        {
            get
            {
                lock (_outputLock)
                {
                    return _currentOutputLine;
                }
            }
        }

        public event EventHandler<OutputEventArgs> HasOutputLine;
        public event EventHandler<OutputEventArgs> HasOutputChar;
        public event EventHandler<OutputEventArgs> HasErrorLine;
        public event EventHandler<OutputEventArgs> HasErrorChar;


        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            _process.Start();

            StateObject outState = new StateObject
            {
                StreamOut = _process.StandardOutput,
                Queue = _ouputQueue,
                IsStandardOut = true
            };

            _outputThread = new Thread(ReadQueueWorker);
            _outputThread.Start(outState);

            StateObject errState = new StateObject
            {
                StreamOut = _process.StandardError,
                Queue = _errorQueue,
                IsStandardError = true
            };

            _errorThread = new Thread(ReadQueueWorker);
            _errorThread.Start(errState);

            StateObject inputState = new StateObject
            {
                StreamIn = _process.StandardInput,
                Queue = _inputQueue
            };

            _inputThread = new Thread(WriteQueueWorker);
            _inputThread.Start(inputState);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public int Stop(string input)
        {
            return Stop(100, 100, 100, 100, input);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int Stop()
        {
            return Stop(null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="inputTimeout"></param>
        /// <param name="outputTimeout"></param>
        /// <param name="errorTimeout"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public int Stop(int timeout, int inputTimeout, int outputTimeout, int errorTimeout, string input)
        {
            try
            {
                if (_inputThread.IsAlive)
                {
                    // user is trying to interact with the process to kill it.
                    if (input != null)
                    {
                        // don't set stopped yet, want let the input queue finish
                        SetInput(input);
                        // give the worker threads time to join
                        Thread.Sleep(timeout);
                    }
                    else
                    {
                        _stopped = true;
                        ThreadSoftStop(_inputThread, inputTimeout);
                    }
                }
                _stopped = true;
                ThreadSoftStop(_outputThread, outputTimeout);
                ThreadSoftStop(_errorThread, errorTimeout);
                _process.WaitForExit(timeout);
            }
            catch (Exception ex)
            {
                // probably going to see ThreadAbortExceptions here
                // figure out what to do here
                throw;
            }

            return _process.HasExited ? _process.ExitCode : -999;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string[] GetOutput()
        {
            return LockAndDequeue(_ouputQueue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string[] GetError()
        {
            return LockAndDequeue(_errorQueue);
        }

        /// <summary>
        /// </summary>
        /// <param name="input"></param>
        public void SetInput(string input)
        {
            LockAndEnqueue(_inputQueue, input);
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        _process.Kill();
                        _process.Dispose();
                    }
                    catch (Exception ex)
                    {
                        // sanity check. empty catch later
                        throw;
                    }
                }
                _disposed = true;
            }
        }

        #endregion

        #region Private implementation

        protected virtual void OnHasErrorLine(OutputEventArgs e)
        {
            EventHandler<OutputEventArgs> handler = HasErrorLine;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnHasErrorChar(OutputEventArgs e)
        {
            EventHandler<OutputEventArgs> handler = HasErrorChar;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnHasOutputLine(OutputEventArgs e)
        {
            EventHandler<OutputEventArgs> handler = HasOutputLine;
            if (handler != null) handler(this, e);
        }

        protected virtual void OnHasOutputChar(OutputEventArgs e)
        {
            EventHandler<OutputEventArgs> handler = HasOutputChar;
            if (handler != null) handler(this, e);
        }


        /// <summary>
        /// </summary>
        /// <param name="state">StatObject with StreamOut</param>
        private void ReadQueueWorker(object state)
        {
            StateObject s = (StateObject)state;


            lock (_outputLock)
            {
                if (s.IsStandardError)
                {
                    _currentErrorLine = string.Empty;
                }
                if (s.IsStandardOut)
                {
                    _currentOutputLine = string.Empty;
                }
            }


            char[] buffer = new char[1];
            int charsRead = s.StreamOut.Read(buffer, 0, 1);

            while (charsRead > 0)
            {
                OutputEventArgs evtArgs;
                lock (_outputLock)
                {
                    evtArgs = new OutputEventArgs { Char = buffer[0], CurrentLine = _currentOutputLine };

                    if (s.IsStandardOut)
                    {
                        _currentOutputLine += buffer[0];
                        OnHasOutputChar(evtArgs);
                    }
                    if (s.IsStandardError)
                    {
                        _currentErrorLine += buffer[0];
                        OnHasErrorChar(evtArgs);
                    }
                }
                // necessary i think to allow parsing of chars and lines
                // to work properly
                Thread.Sleep(1);

                lock (_outputLock)
                {
                    //TODO: this is cheap and faulty. good nuff fo gubment werk but probably
                    //implement a simple lexer/parser in here.

                    // end of line?
                    if (buffer[0] == '\n')
                    {
                        LockAndEnqueue(s.Queue, _currentOutputLine.TrimEnd());
                        _currentOutputLine = string.Empty;
                        if (s.IsStandardOut)
                        {
                            OnHasOutputLine(evtArgs);
                        }
                        if (s.IsStandardError)
                        {
                            OnHasErrorLine(evtArgs);
                        }
                    }
                }

                if (_stopped)
                {
                    break;
                }

                charsRead = s.StreamOut.Read(buffer, 0, 1);
            }
        }


        /// <summary>
        /// </summary>
        /// <param name="state">StatObject with StreamIn</param>
        private void WriteQueueWorker(object state)
        {
            StateObject s = (StateObject)state;

            while (!_stopped)
            {
                if (s.Queue.Count > 0 && !_stopped)
                {
                    string[] inputs = LockAndDequeue(s.Queue);
                    foreach (string input in inputs)
                    {
                        s.StreamIn.Write(input);
                    }
                }
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Thread safety helper
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        private static string[] LockAndDequeue(Queue<string> queue)
        {
            string[] result;
            lock (queue)
            {
                result = new string[queue.Count];
                queue.CopyTo(result, 0);
                queue.Clear();
            }
            return result;
        }

        /// <summary>
        /// Thread safety helper
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="input"></param>
        private static void LockAndEnqueue(Queue<string> queue, string input)
        {
            lock (queue)
            {
                queue.Enqueue(input);
            }
        }

        private static void ThreadSoftStop(Thread thread, int timeout)
        {
            if (thread.IsAlive && !thread.Join(timeout))
            {
                try
                {
                    thread.Abort();
                }
                catch (ThreadAbortException ex)
                {
                }
            }
        }

        #endregion

        #region Nested type: StateObject

        private class StateObject
        {
            public bool IsStandardError;
            public bool IsStandardOut;
            public Queue<string> Queue;
            public StreamWriter StreamIn;
            public StreamReader StreamOut;
        }

        #endregion
    }

    public class OutputEventArgs : EventArgs
    {
        public char Char;
        public string CurrentLine;
    }
}
