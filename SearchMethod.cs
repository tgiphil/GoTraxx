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
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;

namespace GoTraxx
{
	class AbortedSearchException : Exception { }

	class SearchMethod
	{
		protected GoBoard Board;
		protected ISearchInterface SearchInterface;
		protected SearchOptions SearchOptions;
		protected Color PlayerToMove;

		protected int NodesSearched = 0;
		protected int NodesEvaluated = 0;
		protected int TranspositionTableHits = 0;

		protected SearchStatusType Status = SearchStatusType.Initializing;
		protected SearchStatus SearchStatus;
		protected SearchStatus SearchStatusCopy;

		protected bool CheckSuperKo = false;

		protected OnCompletion OnCompletion = null;

		protected volatile bool UpdateStatusFlag = false;
		protected volatile bool StopThinkingFlag = false;

		public SearchMethod(ISearchInterface searchInterface)
		{
			SetSearchInterface(searchInterface);
		}

		public void SetSearchInterface(ISearchInterface searchInterface)
		{
			SearchInterface = searchInterface;
		}

		protected void Stop()
		{
			throw new AbortedSearchException();
		}

		protected void UpdateStatus()
		{
			lock (this)
			{
				SearchStatus.Nodes = NodesSearched;
				SearchStatus.Evals = NodesEvaluated;
				SearchStatus.TranspositionTableHits = TranspositionTableHits;
				SearchStatus.Status = Status;

				SearchStatusCopy = SearchStatus.Clone();
				UpdateStatusFlag = false;
				Monitor.PulseAll(this);
			}
		}

		public void Initialize(GoBoard goBoard, Color playerToMove, SearchOptions searchOptions, OnCompletion onCompletion)
		{
			lock (this)
			{
				SearchStatus = new SearchStatus();
				SearchStatus.BoardSize = goBoard.BoardSize;
				Status = SearchStatusType.Thinking;
				UpdateStatus();
				UpdateStatusFlag = true;
				StopThinkingFlag = false;

				SearchOptions = searchOptions.Clone();
				NodesSearched = NodesEvaluated = 0;
				CheckSuperKo = searchOptions.CheckSuperKo;
				OnCompletion = onCompletion;

				SearchInterface.Initialize(goBoard, SearchOptions);

				Board = goBoard;

				PlayerToMove = playerToMove;
			}
		}

		public virtual void GoThink()
		{
			return;
		}

		public void StartThinking()
		{
			try
			{
				GoThink();
			}
			catch (AbortedSearchException lException)
			{
				int lLevels = lException.StackTrace.Length;	// dummy status to avoid unused variable warning message
			}

			lock (this)
			{
				SearchStatus.Timer.Stop();

				if (Status == SearchStatusType.Stopping)
					Status = SearchStatusType.Stopped;
				else
					Status = SearchStatusType.Completed;

				UpdateStatus(); // and again
			}

			NotifyAll();
		}

		protected void NotifyAll()
		{
			SearchStatus lSearchStatus;

			lock (this)
			{
				if (OnCompletion == null)
					return;

				// todo: create another thread to call OnCompletion

				// call delegate, if any (note, outside of lock!)
				lSearchStatus = SearchStatus.Clone();
				
				OnCompletion(lSearchStatus);
			}

		}


		public void StopThinking()
		{
			lock (this)
			{
				// check is search has completed or been stopped
				if ((Status == SearchStatusType.Completed) || (Status == SearchStatusType.Stopped))
				{
					UpdateStatus();
					NotifyAll();
					return;
				}

				// change status to stopping
				Status = SearchStatusType.Stopping;

				// set flag to stop thinking
				StopThinkingFlag = true;

				// get another update 
				UpdateStatusFlag = true;
			}
		}

		public SearchStatus GetStatus()
		{
			lock (this)
			{
				UpdateStatusFlag = true;

				while ((UpdateStatusFlag) && (Status != SearchStatusType.Stopped) && (Status != SearchStatusType.Completed))
					Monitor.Wait(this, TimeSpan.FromMilliseconds(50));

				return SearchStatusCopy.Clone();
			}
		}

		public void SetNagCoordinator(NagCoordinator nagCoordinator)
		{
		}

	}
}
