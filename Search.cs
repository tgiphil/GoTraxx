using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GoTraxx
{
	class Search
	{
		protected GoBoard Board;
		protected ISearchMethodInterface SearchInterface;
		protected SearchOptions SearchOptions;
		protected Color PlayerToMove;
		protected SearchMethodType SearchMethodType;
		protected Thread SearchThread;
		protected NagCoordinator NagCoordinator;

		protected void StartThread()
		{
			SearchInterface.StartThinking();
		}

		public void Start(GoBoard goBoard, Color playerToMove, SearchOptions searchOptions, SearchMethodType searchMethodType, OnCompletion onCompletion)
		{
			// stop existing search, if any
			if (SearchInterface != null)
				Stop();

			if (SearchMethodType != searchMethodType)
			{
				SearchMethodType = searchMethodType;
				SearchInterface = SearchMethodFactory.CreateFactory(searchMethodType);
			}

			// make a private copy of the board
			Board = goBoard.Clone();

			// make a private copy of the search options
			SearchOptions = searchOptions.Clone();

			// set player to move
			PlayerToMove = playerToMove;

			// set the Nag Coordinator
			SearchInterface.SetNagCoordinator(NagCoordinator);

			// initialize the search parameters
			SearchInterface.Initialize(Board, PlayerToMove, SearchOptions, onCompletion);

			// start search
			SearchThread = new Thread(this.StartThread);
			SearchThread.Start();
		}

		public bool RequestStop()
		{
			if (SearchInterface == null)
				return true;

			SearchStatusType lStatus = SearchInterface.GetStatus().Status;

			if ((lStatus == SearchStatusType.Completed) || (lStatus == SearchStatusType.Stopped))
				return true;

			// harmless if already stopped (method does not block)
			SearchInterface.StopThinking();

			return false;
		}

		public void StartPonder(int lMove, Color playerToMove)
		{
			Console.Error.WriteLine("Pondering >>>");

			Stop();
			Board.PlayStone(lMove, playerToMove, true);

			Start(Board, playerToMove.Opposite, SearchOptions, SearchMethodType, null);
		}

		public void Stop()
		{
			while (true)
			{
				if (RequestStop())
					return;

				lock (SearchInterface)
				{
					Monitor.Wait(SearchInterface, 250);
				}
			}
		}

		public SearchStatus GetStatus()
		{
			return SearchInterface.GetStatus();
		}

		public void RunUntil(int timeOut, bool earlyTimeOut)
		{
			DateTime lEndTime = DateTime.Now.AddSeconds(timeOut);

			while (true)
			{
				SearchStatus lSearchStatus = GetStatus();
				SearchStatusType lSearchStatusType = lSearchStatus.Status;

				if ((lSearchStatusType == SearchStatusType.Completed) || (lSearchStatusType == SearchStatusType.Stopped))
					return;

				// if it very unlike that another result will return in the remaining time, stop researching, so the time can be used later
				if (earlyTimeOut)
				{
					if ((lSearchStatusType == SearchStatusType.Thinking) && (lSearchStatus.BestMoveUpdate.Add(lSearchStatus.BestMoveUpdateTimeLapse).AddSeconds(-1) > lEndTime))
					{
						Console.Error.WriteLine("Early Time Out:");
						Console.Error.WriteLine("Saved: " + (lEndTime - DateTime.Now).Seconds.ToString() + " Seconds");
						return;
					}
				}
				if (DateTime.Now > lEndTime)
					return;

				lock (SearchInterface)
				{
					Monitor.Wait(SearchInterface, 250);
				}
			}
		}

		public void SetNagCoordinator(NagCoordinator nagCoordinator)
		{
			NagCoordinator = nagCoordinator;
		}
	}
}
