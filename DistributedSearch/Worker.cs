/*
 * Copyright (c) 2007 Philipp Garcia (phil@gotraxx.org)
 * 
 * This file is part of GoTraxx (www.gotraxx.org).
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 * This license governs use of the accompanying software. If you use the software, you 
 * accept this license. If you do not accept the license, do not use the software.
 * 
 * Permission is granted to anyone to use this software for any noncommercial purpose, 
 * and to alter it and redistribute it freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not claim that 
 *    you wrote the original software. 
 * 
 * 2. Altered source versions must be plainly marked as such, and must not be 
 *    misrepresented as being the original software.
 * 
 * 3. If you bring a patent claim against the original author or any contributor over 
 *    patents that you claim are infringed by the software, your patent license from 
 *    such contributor to the software ends automatically.
 * 
 * 4. This software may not be used in whole, nor in part, to enter any competition 
 *    without written permission from the original author. 
 * 
 * 5. This notice may not be removed or altered from any source distribution.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GoTraxx
{
    class Worker
    {
        public static string DefaultAddress = "distributed.gotraxx.org";
        public static int DefaultPortNbr = 7000;

        public static string Version = "1.0";

        protected TCPConnection Server;

        protected string Space;
        protected string AccessID;
        protected bool Verbose;

        protected GoBoard Board;
        protected SearchEngine SearchEngine;

        public delegate void SendResponse(string response);

        public Worker(string server, int portNbr, string space, string accessID, int reconnectInterval, bool verbose)
        {
			Server = new TCPConnection(server, portNbr, reconnectInterval, 1000, OnDisconnect, verbose);

            Space = space;
            AccessID = accessID;
            Verbose = verbose;
        }

        public void Run()
        {
            Board = new GoBoard(9);
            SearchEngine = new SearchEngine(Board);

			SearchEngine.SearchOptions.EarlyTimeOut = false;
			SearchEngine.SearchOptions.MaxSeconds = 60 * 60 * 5;	// five hours max.

            Server.Run(ProcessLine);
        }

		public void OnDisconnect()
		{
			SearchEngine.ReInitialize();
		}

        public void ProcessLine(string line)
        {
            if (string.IsNullOrEmpty(line))
                return;

            string lLine = line.TrimEnd('\n').TrimEnd('\r');

            if (Verbose)
                Console.Error.WriteLine("STATUS: W< " + lLine);

            string[] lCmds = lLine.Split('\t');

            if (lCmds.Length < 2)
            {
                Server.SendLine("?\t");
                return;
            }

            if (lCmds[0] == "disconnect")
            {
                Server.Disconnect();
                return;
            }

            if (lCmds[0] == "terminate")
            {
                Server.Terminate = true;
                Server.Disconnect();
                return;
            }

            List<string> lParameters = new List<string>();

            for (int lIndex = 2; lIndex < lCmds.Length; lIndex++)
                lParameters.Add(lCmds[lIndex]);

            WorkerFunctions.Execute(lCmds[1], lCmds[0], lParameters, Board, SearchEngine, Server.SendLine);
        }

    }
}
