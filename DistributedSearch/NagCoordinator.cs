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
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace GoTraxx
{

	class NagCoordinator
	{
		public delegate void Nag();

		protected TCPService Service;
		protected List<WorkerProxy> WorkerProxies = new List<WorkerProxy>();

		protected bool Working = false;
		protected int BoardSize;
		//protected List<KeyValuePair<Color, int>> MoveList = new List<KeyValuePair<Color, int>>();

		protected Nag OnNag;

		protected List<NagNode> AvailableNagNodes = new List<NagNode>();
		protected List<WorkerProxy> AvailableWorkers = new List<WorkerProxy>();
		protected PatternCollection PatternCollection = new PatternCollection();
		protected List<NagNode>[] NagNodeByDepth = new List<NagNode>[1000];

		protected List<NagNode> Results = new List<NagNode>();

		protected static Random Random = new Random();

		public int Count
		{
			get
			{
				lock (this)
				{
					return WorkerProxies.Count;
				}
			}
		}

		public int AvailableWorkersCount
		{
			get
			{
				lock (this)
				{
					return AvailableWorkers.Count;
				}
			}
		}

		public int GetCount()
		{
			return Count;
		}

		public NagCoordinator(int portNumber, PatternCollection patternCollection)
		{
			Service = new TCPService(portNumber, 10000, NewWorker, GetCount);
			PatternCollection = patternCollection;
			OnNag = null;
			Service.StartListeningAsync();
		}

		public void SetNagCallBack(Nag nag)
		{
			OnNag = nag;
		}

		public void NewWorker(Socket socket)
		{
			WorkerProxy lWorker = new WorkerProxy(this, socket);

			lock (this)
			{
				WorkerProxies.Add(lWorker);
			}
		}

		public void Disconnected(WorkerProxy workerProxy)
		{
			lock (this)
			{
				WorkerProxies.Remove(workerProxy);
			}
		}

		public void WorkerAvailable(WorkerProxy worker)
		{
			lock (this)
			{
				if (Working)
					worker.Initalize(BoardSize);//, MoveList);
			}
		}

		public void WorkerNegotiated(WorkerProxy worker)
		{
			lock (this)
			{
				worker.SendPatterns(PatternCollection);
			}
		}

		public void WorkerReady(WorkerProxy worker)
		{
			lock (this)
			{
				if (Working)
					AvailableWorkers.Add(worker);
			}

			InstructWorkers();
		}

		protected bool IsStillValid(NagNode lNagNode)
		{
			lock (this)
			{
				List<NagNode> lNagNodes = NagNodeByDepth[lNagNode.StartDepth];

				if (lNagNodes == null)
					return false;

				return lNagNodes.Contains(lNagNode);
			}
		}

		public void WorkerResult(NagNode nagNode)
		{
			lock (this)
			{
				// note: unless nag results might be put on this list
				// so the results need to be rechecked later

				Results.Add(nagNode);
			}

			if (OnNag != null)
				OnNag(); // tell search that results are available			
		}

		public NagNode GetResult(int depth)
		{
			lock (this)
			{
				if (Results.Count == 0)
					return null;

				foreach (NagNode lNagNode in Results)
					if (IsStillValid(lNagNode))
						return lNagNode;
			}

			return null;
		}

		public void Initialize(GoBoard goBoard)
		{
			lock (this)
			{
				Working = true;
				BoardSize = goBoard.BoardSize;
				AvailableNagNodes.Clear();

				//MoveList.Clear();
				//foreach (KeyValuePair<Color, int> lMove in goBoard.MoveList)
				//	MoveList.Add(new KeyValuePair<Color, int>(lMove.Key, lMove.Value));

				InitializeAll();
			}
		}

		public void InitializeAll()
		{
			ThreadPoolHelper.Execute(InitializeAllThread);
		}

		protected void InitializeAllThread()
		{
			lock (this)
			{
				foreach (WorkerProxy lWorker in WorkerProxies)
					lWorker.Initalize(BoardSize); //, MoveList);
			}
		}

		public void InstructWorkers()
		{
			ThreadPoolHelper.Execute(InstructWorkersThread);
		}

		protected void InstructWorkersThread()
		{
			lock (this)
			{
				while (AvailableWorkers.Count != 0)
				{
					if (AvailableNagNodes.Count == 0)
						return;

					int lIndex = Random.Next(AvailableNagNodes.Count);

					WorkerProxy lWorkerProxy = AvailableWorkers[0];
					NagNode lNagNode = AvailableNagNodes[lIndex];

					AvailableWorkers.RemoveAt(0);
					AvailableNagNodes.RemoveAt(lIndex);

					lWorkerProxy.Start(lNagNode, BoardSize);
				}
			}
		}

		public void NewNagPoint(NagNode nagNode)
		{
			lock (this)
			{
				AvailableNagNodes.Add(nagNode);
			}

			InstructWorkers();
		}

		public void NewNagPoints(List<NagNode> nagNodes)
		{
			lock (this)
			{
				foreach (NagNode lNagNode in nagNodes)
					AvailableNagNodes.Add(lNagNode);
			}

			InstructWorkers();
		}

		public void CreateNagPoints(GoBoard goBoard, int alpha, int beta, Color playerToMove, int depth, int maxDepth, int moves, int maxNags)
		{
			int lLevels = maxDepth - depth;

			if (lLevels < 3)	// avoid shallow searches
				return;

			int lAvailableWorkersCount = AvailableWorkersCount;

			if (lAvailableWorkersCount == 0)
				return;

			List<NagNode> lNagNodes = null;

			lock (this)
			{
				lNagNodes = new List<NagNode>();

				NagNode lNagNode = new NagNode(goBoard, alpha, beta, playerToMove, depth, maxDepth - depth, Random.Next(9) + 1);
				lNagNodes.Add(lNagNode);

				int lNew = Math.Min(Math.Min(lAvailableWorkersCount - 1, moves), maxNags - 1);

				for (int i = 0; i < lNew; i++)
				{
					NagNode lNagNode2 = new NagNode(lNagNode, Random.Next(9) + 1);
					lNagNodes.Add(lNagNode2);
				}

				NagNodeByDepth[depth] = lNagNodes;
			}

			NewNagPoints(lNagNodes);
		}

		public void Abort(int depth)
		{
			List<NagNode> lNagNodes = null;

			lock (this)
			{
				lNagNodes = NagNodeByDepth[depth];
				NagNodeByDepth[depth] = null;
			}

			Abort(lNagNodes);
		}

		protected void Abort(List<NagNode> nagNodes)
		{
			if (nagNodes != null)
				ThreadPoolHelperWithParam<List<NagNode>>.Execute(AbortThread, nagNodes);
		}

		protected void AbortThread(List<NagNode> nagNodes)
		{
			lock (this) // just to be extra safe - lock is not required
			{
				foreach (NagNode lNagNode in nagNodes)
					if (lNagNode.Worker != null)
					{
						lNagNode.Worker.Abort();

						Results.Remove(lNagNode);
					}
			}
		}

		public void StopAll()
		{
			lock (this)
			{
				Working = false;
				AvailableNagNodes.Clear();

				foreach (WorkerProxy lWorker in WorkerProxies)
					lWorker.Abort();

				NagNodeByDepth = new List<NagNode>[1000];
				Results.Clear();
			}
		}

	}
}
