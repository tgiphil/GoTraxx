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

namespace GoTraxx
{
	public struct Coordinate : IComparable
	{
		private static int[,] TransformationMatrix = Coordinate.BuildTransformationMatrix();

		public int X;
		public int Y;

		/// <summary>
		/// Initializes a new instance of the <see cref="Coordinate"/> class.
		/// </summary>
		/// <param name="p"></param>
		public Coordinate(Coordinate p)
		{
			X = p.X;
			Y = p.Y;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Coordinate"/> class.
		/// </summary>
		/// <param name="pX">X</param>
		/// <param name="pY">Y</param>
		public Coordinate(int pX, int pY)
		{
			X = pX;
			Y = pY;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Coordinate"/> class.
		/// </summary>
		/// <param name="pX">X</param>
		/// <param name="pY">Y</param>
		public Coordinate(char pX, int pY)
		{
			char lA = Char.ToLower(pX);

			X = (lA < 'I') ? lA - 'a' : lA - 'a' - 1;

			Y = pY - 1;
		}

		public static Coordinate operator +(Coordinate p1, Coordinate p2)
		{
			return new Coordinate(p1.X + p2.X, p1.Y + p2.Y);
		}

		public static Coordinate operator -(Coordinate p1, Coordinate p2)
		{
			return new Coordinate(p1.X - p2.X, p1.Y - p2.Y);
		}

		public static bool operator ==(Coordinate l, Coordinate r)
		{
			if (object.ReferenceEquals(l, r))
				return true;
			else if (object.ReferenceEquals(l, null) ||
					 object.ReferenceEquals(r, null))
				return false;

			return (l.X == r.X) && (l.Y == r.Y);
		}

		public static bool operator !=(Coordinate l, Coordinate r)
		{
			return !(l == r);
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">Another object to compare to.</param>
		/// <returns>
		/// true if obj and this instance are the same type and represent the same value; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			return (((Coordinate)obj) == this);
		}

		/// <summary>
		/// Compares the current instance with another object of the same type.
		/// </summary>
		/// <param name="obj">An object to compare with this instance.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than obj. Zero This instance is equal to obj. Greater than zero This instance is greater than obj.
		/// </returns>
		/// <exception cref="T:System.ArgumentException">obj is not the same type as this instance. </exception>
		public int CompareTo(object obj)
		{
			Coordinate lCoordinate = (Coordinate)obj;
			
			if (lCoordinate == this)
				return 0;
			else
				return -1;
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
			return ((~X) | Y);
		}

		public override string ToString()
		{
			return "(" + X.ToString() + "," + Y.ToString() + ") ";
		}

		public void StepNext()
		{
			StepNext(ref X, ref Y);
		}

		public void SpiralNext()
		{
			SpiralNext(ref X, ref Y);
		}

		public Coordinate Transform(int i)
		{
			int lX = X;
			int lY = Y;

			Transformation(i, ref lX, ref lY);

			return new Coordinate(lX, lY);
		}

		/// <summary>
		/// Determines whether the coordinate is on a board.
		/// </summary>
		/// <param name="boardSize">Size of the Board.</param>
		/// <returns>
		/// 	<c>true</c> if coordinate is on a board; otherwise, <c>false</c>.
		/// </returns>
		public bool IsOnBoard(int boardSize)
		{
			return ((X >= 0) && (Y >= 0) && (X < boardSize) && (Y < boardSize));
		}

		static public void StepNext(ref int x, ref int y)
		{
			if (x == 0)
			{
				// jump
				x = y + 1;
				y = 0;
			}
			else
				if (y < x)
				{
					// up
					y = y + 1;
				}
				else
				{
					// left
					x = x - 1;
				}

		}

		static public void SpiralNext(ref int x, ref int y)
		{
			if ((x == 0) && (y == 0))
			{
				// start
				x = -1;
				y = 0;
			}
			else
				if ((y == 1) && (x <= 0))
				{
					// jump
					x = x - 2;
					y = 0;
				}
				else
					if ((x < 0) && (y <= 0))
					{
						// down, right
						x = x + 1;
						y = y - 1;
					}
					else
						if ((x >= 0) && (y < 0))
						{
							// up, right
							x = x + 1;
							y = y + 1;
						}
						else
							if ((x > 0) && (y >= 0))
							{
								// up, left
								x = x - 1;
								y = y + 1;
							}
							else
							{
								// down, left
								x = x - 1;
								y = y - 1;
							}
		}

		public static void Transformation(int i, ref int x, ref int y)
		{
			switch (i)
			{
				case 0: { return; }
				case 1: { x = -x; return; }
				case 2: { y = -y; return; }
				case 3: { y = -y; x = -x; return; }

				case 4: { int t = y; y = x; x = t; return; }
				case 5: { int t = y; y = -x; x = t; return; }
				case 6: { int t = -y; y = x; x = t; return; }
				case 7: { int t = -y; y = -x; x = t; return; }

				case -1: { y = -y; x = -x; return; }

				default: return;
			}
		}


		/// <summary>
		/// Builds the transformation matrix.
		/// </summary>
		/// <returns></returns>
		private static int[,] BuildTransformationMatrix()
		{
			int[,] lMatrix = new int[8, 8];

			Coordinate lCoordinate = new Coordinate(1, 2);

			for (int t1 = 0; t1 < 8; t1++)
				for (int t2 = 0; t2 < 8; t2++)
				{
					Coordinate lCoordinate2  = lCoordinate.Transform(t1).Transform(t2);

					for (int t3 = 0; t3 < 8; t3++)
						if (lCoordinate == lCoordinate2.Transform(t3))
						{
							lMatrix[t1, t2] = t3;
							break;
						}

				}

			return lMatrix;
		}

		/// <summary>
		/// Reduces the transformation.
		/// </summary>
		/// <param name="pTransform1">The transform1.</param>
		/// <param name="pTransform2">The transform2.</param>
		/// <returns></returns>
		public static int ReduceTransformation(int transform1, int transform2)
		{
			return Coordinate.TransformationMatrix[transform1, transform2];
		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns></returns>
		public System.Collections.IEnumerator GetEnumerator()
		{
			for (int i = 0; i < 8; i++)
				yield return this.Transform(i);
		}
	}
}
