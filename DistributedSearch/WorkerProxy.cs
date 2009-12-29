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
	class WorkerProxy
	{
		public enum ConnectionState { Negotiating, Negotiated, SendingPatterns, Available, Initializing, Ready, Thinking, Aborting, Disconnecting, Disconnected };
		protected ConnectionState State;

		protected TCPConnection Worker;

		protected NagCoordinator NagCoordinator;
		protected int CommandNbr = 0;

		protected int BoardSize;
		public NagNode NagNode;
		public bool Verbose = true;


		public WorkerProxy(NagCoordinator coordinator, Socket socket)
		{
			Worker = new TCPConnection(socket, false, ProcesLine, null);
			NagCoordinator = coordinator;
			State = ConnectionState.Negotiating;
			Negotiate();
		}

		public void SendCommand(string cmd, string id, string parameters)
		{
			Worker.SendLine(id + "\t" + cmd + (string.IsNullOrEmpty(parameters) ? "" : "\t" + parameters));
		}

		public void SendCommand(string cmd, string id)
		{
			SendCommand(cmd, id, string.Empty);
		}

		public void SendCommand(string cmd, string id, string parameter1, string parameter2)
		{
			SendCommand(cmd, id, parameter1 + "\t" + parameter2);
		}

		public void SendCommand(string cmd, string id, int parameter)
		{
			SendCommand(cmd, id, parameter.ToString());
		}

		public void SendCommand(string cmd, string id, int parameter1, int parameter2)
		{
			SendCommand(cmd, id, parameter1.ToString(), parameter2.ToString());
		}

		protected string FormatIdNbr(int commandNbr, int sequence)
		{
			return commandNbr.ToString() + "." + sequence.ToString();
		}

		public void Disconnect()
		{
			lock (this)
			{
				Console.Error.WriteLine("STATUS: Disconnecting Connection");
				State = ConnectionState.Disconnecting;
				NagNode = null;
				NagCoordinator.Disconnected(this);
				Worker.Disconnect(true);
			}
		}

		protected void Negotiate()
		{
			lock (this)
			{
				State = ConnectionState.Negotiating;
				CommandNbr++;
				SendCommand("version", FormatIdNbr(CommandNbr, 1));
			}
		}

		public void Abort()
		{
			lock (this)
			{
				if (State == ConnectionState.Ready)
					return;

				if (State != ConnectionState.Thinking)
					return;

				State = ConnectionState.Aborting;
				NagNode = null;

				SendCommand("abort", FormatIdNbr(++CommandNbr, 1));
			}
		}

		public void SendPatterns(PatternCollection patternCollection)
		{
			lock (this)
			{
				if (State != ConnectionState.Negotiated)
				{
					Abort();
					return;
				}

				CommandNbr++;

				if (patternCollection != null)
				{
					StringBuilder lString = new StringBuilder();

					foreach (Pattern lPattern in patternCollection)
					{
						lString.Append(lPattern.ToString().Replace("\t", " ").Replace("~", " ").Replace("\n", "~").Replace("\r", ""));
						lString.Append("\t");
					}

					SendCommand("add_patterns", FormatIdNbr(CommandNbr, 1), lString.ToString().TrimEnd('\t'));
				}
				else
					SendCommand("add_patterns", FormatIdNbr(CommandNbr, 1), "");

				State = ConnectionState.SendingPatterns;
			}

		}

		public void Initalize(int boardSize)
		{
			lock (this)
			{
				if (State == ConnectionState.Thinking)
				{
					Abort();
					return;
				}

				if (!((State == ConnectionState.Available) || (State == ConnectionState.Ready)))
					return;

				NagNode = null;
				State = ConnectionState.Initializing;
				CommandNbr++;

				SendCommand("set_boardsize", FormatIdNbr(CommandNbr, 1), boardSize);
			}
		}

		public void Start(NagNode nagNode, int boardSize)
		{
			BoardSize = boardSize;
			NagNode = nagNode;
			NagNode.Worker = this;
			Start(nagNode.PlayerToMove, nagNode.MoveList, nagNode.Depth, nagNode.Alpha, nagNode.Beta, nagNode.PermutationNbr);
		}

		protected void Start(Color color, List<KeyValuePair<Color, int>> additionalMoves, int depth, int alpha, int beta, int permutation)
		{
			lock (this)
			{
				if (State != ConnectionState.Ready)
					return;	// something is wrong!

				State = ConnectionState.Thinking;
				CommandNbr++;

				SendCommand("clearboard", FormatIdNbr(CommandNbr, 1), depth);

				if (additionalMoves.Count != 0)
				{
					StringBuilder lString = new StringBuilder();

					foreach (KeyValuePair<Color, int> lMove in additionalMoves)
					{
						lString.Append(lMove.Key.ToChar2());
						lString.Append(" ");
						lString.Append(CoordinateSystem.ToString2(lMove.Value, BoardSize));
						lString.Append(" ");
					}

					SendCommand("play_sequence", FormatIdNbr(CommandNbr, 2), lString.ToString().TrimEnd());
				}

				SendCommand("set_depth", FormatIdNbr(CommandNbr, 3), depth);
				SendCommand("set_alpha_beta", FormatIdNbr(CommandNbr, 4), alpha, beta);
				SendCommand("set_permutation", FormatIdNbr(CommandNbr, 5), depth);
				SendCommand("search", FormatIdNbr(CommandNbr, 6), color.ToString2().ToLower());
			}
		}

		public void ProcesLine(string line)
		{
			if (line == null)
			{
				Disconnect();
				return;
			}

			string lLine = line.TrimEnd('\n');
			string[] lCmds = lLine.Split('\t');
			bool lAsync = (lCmds[0][0] == '!');
			bool lSuccess = (lCmds[0][lAsync ? 1 : 0] == '=');

			string[] lID = lCmds[1].Split('.');
			int lCommandNbr = Convert.ToInt32(lID[0]);
			int lSeqNbr = Convert.ToInt32(lID[1]);

			if ((Verbose) && (lCmds.Length > 3))
				Console.Error.WriteLine("STATUS: P< " + lLine);

			lock (this)
			{
				if ((lCommandNbr == 0) && (lSeqNbr == 0) && (!lSuccess))
				{
					// worker disconnecting...
					Disconnect();
					return;
				}

				if (State == ConnectionState.Negotiating)
				{
					if ((!lSuccess) || (lCmds[2] != "1.0"))
					{
						Disconnect();
						return;
					}

					if (lSeqNbr == 1)
					{
						State = ConnectionState.Negotiated;
						ThreadPoolHelperWithParam<WorkerProxy>.Execute(NagCoordinator.WorkerNegotiated, this);
						//NagCoordinator.WorkerNegotiated(this);
					}
				}
				else if (State == ConnectionState.SendingPatterns)
				{
					if (CommandNbr == lCommandNbr)
					{
						if (!lSuccess)
						{
							Disconnect();
							return;
						}

						if (lSeqNbr == 1)
						{
							State = ConnectionState.Available;
							ThreadPoolHelperWithParam<WorkerProxy>.Execute(NagCoordinator.WorkerAvailable, this);
							//NagCoordinator.WorkerAvailable(this);
						}
					}
				}
				else if (State == ConnectionState.Initializing)
				{
					if (CommandNbr == lCommandNbr)
					{
						if (!lSuccess)
						{
							// error with commands, disconnect to reset worker
							Disconnect();
							return;
						}

						if (lSeqNbr == 1)
						{
							State = ConnectionState.Ready;
							ThreadPoolHelperWithParam<WorkerProxy>.Execute(NagCoordinator.WorkerReady, this);
							//						NagCoordinator.WorkerReady(this);
						}
					}
				}
				else if (State == ConnectionState.Thinking)
				{
					if (CommandNbr == lCommandNbr)
					{
						if (!lSuccess)
						{
							// error with commands, disconnect to reset worker
							Disconnect();
							return;
						}

						if ((lSeqNbr == 6) && (lAsync))
						{

							State = ConnectionState.Ready;
							if (NagNode != null)
							{
								NagNode.SetResult(Convert.ToInt32(lCmds[3]), CoordinateSystem.At(lCmds[2], BoardSize));
								ThreadPoolHelperWithParam<NagNode>.Execute(NagCoordinator.WorkerResult, NagNode);
								NagNode = null;
							}

							ThreadPoolHelperWithParam<WorkerProxy>.Execute(NagCoordinator.WorkerReady, this);
						}
					}
				}
				else if (State == ConnectionState.Aborting)
				{
					if (CommandNbr == lCommandNbr)
					{
						if (!lSuccess)
						{
							// error with commands, disconnect to reset worker
							Disconnect();
							return;
						}

						if (lSeqNbr == 1)
						{
							State = ConnectionState.Ready;
							NagNode = null;
							ThreadPoolHelperWithParam<WorkerProxy>.Execute(NagCoordinator.WorkerReady, this);
						}
					}
				}
			}
		}
	}
}
