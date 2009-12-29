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
	class GTPCommCGOS : GTPCommInternal
	{
		public enum CGOSServerVersion { E0, E1 };

		public static string DefaultAddress = "cgos.boardspace.net";
		public static int DefaultPortNbr = 6867;

		protected TCPConnection Server;

		protected string UserName;
		protected string Password;

		protected bool Verbose;
		protected int GamesToPlay;
		protected bool SupportTimeLeft;

		protected bool WaitingForGame;

		protected CGOSServerVersion CGOSServer;

		public GTPCommCGOS(string server, int portNbr, string userName, string password, int gamesToPlay, bool verbose)
		{
			Server = new TCPConnection(server, portNbr, 15, 100, null, verbose);
			UserName = userName;
			Password = password;
			Verbose = verbose;
			SupportTimeLeft = false;
			GamesToPlay = gamesToPlay;
			WaitingForGame = false;
			CGOSServer = CGOSServerVersion.E0;
		}

		// overriding to change access modifier to protected
		protected new string GetResponse()
		{
			return base.GetResponse();
		}

		// overriding to change access modifier to protected
		protected new void FlushResponses()
		{
			base.FlushResponses();
		}

		protected new void SendToEngine(string str)
		{
			if (Verbose)
				Console.Error.Write("STATUS: E> " + str);

			base.SendToEngine(str);
		}

		protected void CheckTimeLeftSupportedByEngine()
		{
			SupportTimeLeft = false;

			FlushResponses();
			SendToEngine("list_commands\n");

			string lResponse = GetResponse();

			if (string.IsNullOrEmpty(lResponse))
				return;

			if (lResponse.IndexOf("time_left\n") >= 0)
				SupportTimeLeft = true;
		}

		public void Run()
		{
			CheckTimeLeftSupportedByEngine();

			Server.Run(ProcessLine);
		}

		public void ProcessLine(string line)
		{
			if (string.IsNullOrEmpty(line))
				return;

			string[] lCmds = line.TrimEnd('\n').TrimEnd('\r').Replace("\r", "").Split(' ');

			//if (Verbose)
			//	Console.Error.WriteLine("STATUS: C< " + lLine.ToString().Replace("\r", ""));

			switch (lCmds[0])
			{
				case "username":
					{
						Server.SendLine(UserName);
						break;
					}
				case "password":
					{
						Server.SendLine(Password);
						break;
					}
				case "protocol":
					{
						Server.SendLine("e1");
						CGOSServer = CGOSServerVersion.E1;
						break;
					}
				case "successfully":
					{
						Server.SendLine("ping");
						WaitingForGame = true;
						break;
					}
				case "you":	// You are already logged on!  Closing connection.
					{
						Server.Disconnect();
						break;
					}
				case "Error:":	// You are already logged on!  Closing connection.
					{
						Server.Disconnect();
						break;
					}
				case "setup": // for newer version of CGOS
					{
						WaitingForGame = false;

						SendToEngine("boardsize " + lCmds[2] + "\n");
						SendToEngine("clear_board\n");
						SendToEngine("komi " + lCmds[3] + "\n");

						if (SupportTimeLeft)
						{
							double lTimeLeft = 1000;

							if (Double.TryParse(lCmds[4], out lTimeLeft))
							{
								SendToEngine("time_left b " + (lTimeLeft / 1000) + " 0 0\n");
								SendToEngine("time_left w " + (lTimeLeft / 1000) + " 0 0\n");
							}
						}

						int lArg = 0;

						while ((lArg * 2) + 7 < lCmds.Length)
						{
							SendToEngine("play " + (lArg % 2 == 0 ? "b" : "w") + " " + lCmds[lArg * 2 + 7] + "\n");

							lArg++;
						}

						FlushResponses();
						break;
					}
				case "newgame": // for older version of CGOS
					{
						WaitingForGame = false;

						SendToEngine("boardsize " + lCmds[1] + "\n");
						SendToEngine("clear_board\n");
						SendToEngine("komi " + lCmds[2] + "\n");
						if (SupportTimeLeft)
						{
							double lTimeLeft = 1000;

							if (Double.TryParse(lCmds[3], out lTimeLeft))
							{
								SendToEngine("time_left b " + lTimeLeft + " 0 0\n");
								SendToEngine("time_left w " + lTimeLeft + " 0 0\n");
							}
						}

						FlushResponses();
						break;
					}
				case "play":
					{
						if (SupportTimeLeft)
						{
							double lTimeLeft = 1000;

							if (Double.TryParse(lCmds[3], out lTimeLeft))
								SendToEngine("time_left " + lCmds[1] + " " + (lTimeLeft / 1000) + " 0 0\n");

							FlushResponses();
						}

						SendToEngine("play " + lCmds[1] + " " + lCmds[2] + "\n");
						FlushResponses();
						break;
					}
				case "genmove":
					{
						FlushResponses();

						if (SupportTimeLeft)
						{
							double lTimeLeft = 1000;

							if (Double.TryParse(lCmds[2], out lTimeLeft))
								SendToEngine("time_left " + lCmds[1] + " " + (lTimeLeft / 1000) + " 0 0\n");

							FlushResponses();
						}

						SendToEngine("genmove " + lCmds[1] + "\n");

						string lResponse = GetResponse();
						Server.SendLine(lResponse.Replace('=', ' ').Trim());

						break;
					}
				case "gameover":
					{
						// todo: tell GTP engine... 

						WaitingForGame = true;
						if (GamesToPlay <= 1)
						{
							Server.SendLine("");
							Server.Terminate = true;
							break;
						}

						if (CGOSServer == CGOSServerVersion.E0)
							Server.SendLine("ok");
						else
							Server.SendLine("ready");

						if (GamesToPlay > 0)
							GamesToPlay--;

						SendToEngine("boardsize " + lCmds[1] + "\n");
						SendToEngine("clear_board\n");

						SendToEngine("gotraxx-just_think\n");

						FlushResponses();

						if (Verbose)
							Console.Error.WriteLine("STATUS: Games Left: " + GamesToPlay.ToString());

						break;
					}
				case "info":
					{
						break;
					}
				case "time_left":
					{
						if (SupportTimeLeft)
						{
							SendToEngine("time_left " + lCmds[1] + " " + lCmds[2] + " 0\n");
							FlushResponses();
						}
						break;
					}
				default:
					break;
			}

		}

	}
}
