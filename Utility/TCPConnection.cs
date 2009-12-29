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

namespace GoTraxx
{
	public class TCPConnection
	{
		public delegate void ProcessLine(string line);
		public delegate void Disconected();

		private string Server;
		private int PortNbr;

		protected int ReconnectInterval = 15; // in seconds
		protected int ConnectionAttemptsLeft = 0;
		public bool Terminate = false;

		private Socket Socket;
		private byte[] SocketBuffer = new byte[1024];
		private string Buffer = string.Empty;

		private ProcessLine OnProcessLine = null;
		private Disconected OnDisconnect = null;
		private bool NotifyOnDisconnect = false;

		public bool Verbose;

		public TCPConnection(string server, int portNbr, int reconnectInterval, int maxConnectionAttempts, Disconected onDisconnect, bool verbose)
		{
			Server = server;
			PortNbr = portNbr;
			Verbose = verbose;
			ReconnectInterval = reconnectInterval;
			ConnectionAttemptsLeft = maxConnectionAttempts;
		}

		public TCPConnection(Socket socket, bool verbose, ProcessLine processLine, Disconected onDisconnect)
		{
			Socket = socket;
			Verbose = verbose;
			OnProcessLine = processLine;
			OnDisconnect = onDisconnect;
			NotifyOnDisconnect = true;
			BeginReceive();
		}

		public bool Connected
		{
			get
			{
				lock (this)
				{
					if (Socket == null)
						return false;

					return Socket.Connected;
				}
			}
		}

		protected void BeginReceive()
		{
			try
			{
				lock (this)
				{
					if (Socket != null)
						Socket.BeginReceive(SocketBuffer, 0, 0, SocketFlags.None, new AsyncCallback(DataReadyHandler), null);
				}
			}
			catch (Exception e)
			{
				if (Verbose)
					Console.Error.WriteLine("ERROR: " + e.ToString());
			}
		}

		protected bool Connect()
		{

			if (Verbose)
				Console.Error.WriteLine("STATUS: Attempting connection to: " + Server);

			try
			{
				lock (this)
				{
					NotifyOnDisconnect = true;
					Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

					if (Socket == null)
						return false;

					Socket.Connect(Server, PortNbr);
					NotifyOnDisconnect = true;

					BeginReceive();
				}

			}
			catch (Exception e)
			{
				if (Verbose)
					Console.Error.WriteLine("ERROR: " + e.ToString());
			}

			if (Verbose)
				if (Connected)
					Console.Error.WriteLine("STATUS: Successfully connected!");
				else
					Console.Error.WriteLine("STATUS: Failed connection!");

			return Connected;
		}

		public void Disconnect()
		{
			Disconnect(true);
		}

		public void Disconnect(bool suppressNotification)
		{
			try
			{
				lock (this)
				{
					if (Socket != null)
					{
						Socket.Shutdown(SocketShutdown.Both);
						Socket.Close();
						Socket = null;
					}
					NotifyDisconnect(suppressNotification);
				}
			}
			catch (Exception e)
			{
				if (Verbose)
					Console.Error.WriteLine("ERROR: " + e.ToString());
			}
		}

		protected void NotifyDisconnect(bool suppressNotification)
		{
			lock (this)
			{
				if (NotifyOnDisconnect)
				{
					NotifyOnDisconnect = false;
					if ((!suppressNotification) && (OnDisconnect != null))
						OnDisconnect();
				}
			}
		}

		private void Receive()
		{
			try
			{
				lock (this)
				{
					if (Socket != null)
						while (Socket.Available > 0)
						{
							int lReadBytes = Socket.Receive(SocketBuffer);

							if (lReadBytes > 0)
							{
								string lMsg = Encoding.UTF8.GetString(SocketBuffer, 0, lReadBytes);

								Buffer += lMsg;

								if (Verbose)
									Console.Error.WriteLine("STATUS: R< " + lMsg.Trim('\n'));
							}

							Monitor.Pulse(this);
						}
				}
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("ERROR: " + e.ToString());
			}
		}

		private void DataReadyHandler(IAsyncResult ar)
		{
			Receive();

			if (OnProcessLine != null)
				Process();

			BeginReceive();

			if (!Connected)
				NotifyDisconnect(false);
		}

		public void SendLine(string str)
		{
			if (Verbose)
				Console.Error.WriteLine("STATUS: S> " + str);

			try
			{
				lock (Socket)
				{
					Socket.Send(Encoding.ASCII.GetBytes(str + "\n"));
				}
			}
			catch (Exception e)
			{
				if (Verbose)
					Console.Error.WriteLine("ERROR: " + e.ToString());
			}
		}

		protected string GetLine()
		{
			lock (this)
			{
				int lLine = Buffer.IndexOf("\n");

				if (lLine < 0)
					return null;

				string lString = Buffer.Substring(0, lLine + 1);

				Buffer = Buffer.Substring(lLine + 1);

				return lString;
			}
		}

		public void Run(ProcessLine processLine)
		{
			while (!Terminate)
			{
				if (!Connected)
					Connect();

				if (Connected)
					Process(processLine);
				else
					if (--ConnectionAttemptsLeft <= 0)
						Terminate = true;

				if (!Terminate)
				{
					if (Verbose)
						Console.Error.WriteLine("STATUS: Will attempt to reconnect in " + ReconnectInterval.ToString() + " seconds.");

					Thread.Sleep(new TimeSpan(0, 0, ReconnectInterval));
				}
			}

			Disconnect();
		}

		protected void Process(ProcessLine processLine)
		{
			while (Connected)
			{
				lock (this)
				{
					processLine(GetLine());

					if (Terminate)
						Disconnect();

					Monitor.Wait(this, TimeSpan.FromMilliseconds(500));
				}
			}
		}

		protected void Process()
		{
			lock (this)
			{
				OnProcessLine(GetLine());

				if (Terminate)
					Disconnect();
			}
		}

	}
}

