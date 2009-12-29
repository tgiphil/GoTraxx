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
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace GoTraxx
{
	public class TCPService
	{
        public delegate void NewConnection(Socket socket);
        public delegate int GetCount();

		private Socket Listener;

		public int PortNumber;
		public int MaxConnections;
		public int AcceptedConnections = 0;

		public bool Verbose = true;
        protected NewConnection OnNewConnection;
        protected GetCount GetConnectCount;

        public TCPService(int portNumber, int maxConnections, NewConnection newConnection, GetCount getCount)
		{
			PortNumber = portNumber;
			MaxConnections = maxConnections;
			AcceptedConnections = 0;
            OnNewConnection = newConnection;
            GetConnectCount = getCount;
		}

		public void StartListening()
		{
			try
			{
				lock (this)
				{
					Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					Listener.Bind(new IPEndPoint(IPAddress.Any, PortNumber));
					Listener.Listen(100);
					Listener.BeginAccept(new AsyncCallback(AcceptConnection), null);
				}
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e.ToString());
			}
		}

		protected static void StartListening(object pGameServer)
		{
			TCPService lService = pGameServer as TCPService;

			lService.StartListening();
		}

		public void StartListeningAsync()
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback(StartListening), this);
		}

		public void StopListening()
		{
			lock (this)
			{
				Listener.Close();
				Listener = null;
			}
		}

		private void AcceptConnection(IAsyncResult ar)
		{
			lock (this)
			{
				if (Listener == null)
					return;

				Socket lSocket = Listener.EndAccept(ar);

				if (Verbose)
					Console.Error.WriteLine("STATUS: Accepting Connection");

                if (GetConnectCount() >= MaxConnections)
				{
					lSocket.Shutdown(SocketShutdown.Both);
					lSocket.Close();
				}
				else
				{
					AcceptedConnections++;

                    OnNewConnection(lSocket);
				}

				Listener.BeginAccept(new AsyncCallback(AcceptConnection), null);
			}
		}
	}
}
