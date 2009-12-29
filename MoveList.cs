using System;
using System.Collections.Generic;
using System.Text;

namespace GoTraxx
{
	class MoveList
	{
		protected static Random Random = new Random();
		protected List<int> Moves;
		protected double[] Values;

		/// <summary>
		/// Gets all the moves.
		/// </summary>
		/// <value>All moves.</value>
		public List<int> AllMoves
		{
			get
			{
				return Moves;
			}
		}

		/// <summary>
		/// Gets the number of moves in list.
		/// </summary>
		/// <value>The count.</value>
		public int Count
		{
			get
			{
				return Moves.Count;
			}
		}

		/// <summary>
		/// Gets the size of the board.
		/// </summary>
		/// <value>The size of the board.</value>
		public int BoardSize
		{
			get
			{
				return Values.Length;
			}
		}

		/// <summary>
		/// Gets the i-th moves in list
		/// </summary>
		/// <value></value>
		public int this[int i]
		{
			get
			{
				return Moves[i];
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MoveList"/> class.
		/// </summary>
		/// <param name="boardSize">Size of the board.</param>
		public MoveList(int boardSize)
		{
			Moves = new List<int>();
			Values = new double[boardSize * boardSize + 1];
		}

		/// <summary>
		/// Adds the specified move.
		/// </summary>
		/// <param name="move">The move.</param>
		public void Add(int move)
		{
			Moves.Add(move);
			Values[move + 1] = 0;
		}

		/// <summary>
		/// Adds the specified move with a value
		/// </summary>
		/// <param name="move">The move.</param>
		/// <param name="value">The value.</param>
		public void Add(int move, double value)
		{
			Moves.Add(move);
			Values[move + 1] = value;
		}

		/// <summary>
		/// Sets the value of a move.
		/// </summary>
		/// <param name="move">The move.</param>
		/// <param name="value">The value.</param>
		public void SetValue(int move, double value)
		{
			Values[move + 1] = value;
		}

		/// <summary>
		/// Sets the min value.
		/// </summary>
		/// <param name="move">The move.</param>
		/// <param name="value">The value.</param>
		public void SetMinValue(int move, double value)
		{
			if (Values[move + 1] < value)
				Values[move + 1] = value;
		}

		/// <summary>
		/// Gets the priority of a move
		/// </summary>
		/// <param name="move">The move.</param>
		/// <returns></returns>
		public double GetValue(int move)
		{
			return Values[move + 1];
		}

		/// <summary>
		/// Removes a move.
		/// </summary>
		/// <param name="move">The move.</param>
		public void RemoveMove(int move)
		{
			Moves.Remove(move);
		}

		/// <summary>
		/// Prunes the moves.
		/// </summary>
		/// <param name="minValue">The min value.</param>
		public void PruneMoves(double minValue)
		{
			for (int lMove = 0; lMove < Values.Length; lMove++)
				if (Values[lMove + 1] < minValue)
					Moves.Remove(lMove);
		}

		/// <summary>
		/// Sorts the move list
		/// </summary>
		public void QuickSort()
		{
			if (Moves.Count > 1)
				QuickSort(0, Moves.Count - 1);
		}

		/// <summary>
		/// Quick sorts the move list
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		protected void QuickSort(int left, int right)
		{
			int i = left, j = right;

			double x = Values[Moves[(left + right) / 2] + 1];

			do
			{
				while ((Values[Moves[i] + 1] > x) && (i < right))
					i++;
				while ((x > Values[Moves[j] + 1]) && (j > left))
					j--;

				if (i <= j)
				{
					int y = Moves[i];
					Moves[i] = Moves[j];
					Moves[j] = y;
					i++;
					j--;
				}
			} while (i <= j);

			if (left < j)
				QuickSort(left, j);

			if (i < right)
				QuickSort(i, right);
		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns></returns>
		public System.Collections.IEnumerator GetEnumerator()
		{
			foreach (int lMove in Moves)
				yield return lMove;
		}

		/// <summary>
		/// Determines whether the move lists contains the specified move.
		/// </summary>
		/// <param name="move">The move.</param>
		/// <returns>
		/// 	<c>true</c> if [contains] [the specified move]; otherwise, <c>false</c>.
		/// </returns>
		public bool Contains(int move)
		{
			return Moves.Contains(move);
		}

		/// <summary>
		/// Gets the top move.
		/// </summary>
		/// <returns></returns>
		public int GetBestMove()
		{
			if (Moves.Count == 0)
				return CoordinateSystem.PASS;

			int lBest = Moves[0];
			double lBestValue = Values[lBest + 1];

			foreach (int lMove in Moves)
				if (lBestValue < Values[lMove + 1])
				{
					lBest = lMove;
					lBestValue = Values[lMove + 1];
				}

			return lBest;
		}

		static public List<int> Randomize(List<int> list, int permutation, int depth)
		{
			if ((depth > 3) || (permutation <= 0))
				return list;

			int lSize = list.Count;

			if (lSize == 1)
				return list;

			double lPercent = ((10 - permutation) * .10) - ((depth - 1) * .25);

			if (lPercent > 1.0)
				lPercent = 1.0;
			else if (lPercent < 0)
				return list;


			int lSwaps = list.Count * (int)lPercent;

			while (lSwaps > 0)
			{
				int lFrom = Random.Next(lSize);
				int lTo = Random.Next(lSize);

				if (lFrom != lTo)
				{
					int lTemp = list[lFrom];
					list[lFrom] = list[lTo];
					list[lTo] = lTemp;
					--lSwaps;
				}
			}

			return list;
		}
	}
}
