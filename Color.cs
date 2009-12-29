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

	public struct Color
	{
		private enum ColorEnum : byte { Black = 0, White = 1, Empty = 2, Border = 3, Both = 4 };

		private ColorEnum mColor;

		/// <summary>
		/// Initializes a new instance of the <see cref="Color"/> class.
		/// </summary>
		/// <param name="color">Color</param>
		private Color(ColorEnum color)
		{
			mColor = color;
		}

		public static Color ToColor(char color)
		{
			return new Color((Char.ToUpper(color) == 'B') ? ColorEnum.Black : ColorEnum.White);
		}

		public static Color ToColor(string color)
		{
			if (color.Length < 1)
				return new Color(ColorEnum.Empty);

			return ToColor(color[0]);
		}

		/// <summary>
		/// Gets the black color.
		/// </summary>
		/// <value>The black.</value>
		public static Color Black
		{
			get
			{
				return new Color(ColorEnum.Black);
			}
		}

		/// <summary>
		/// Gets the white color.
		/// </summary>
		/// <value>The white.</value>
		public static Color White
		{
			get
			{
				return new Color(ColorEnum.White);
			}
		}

		/// <summary>
		/// Gets the empty (no color) value.
		/// </summary>
		/// <value>The empty.</value>
		public static Color Empty
		{
			get
			{
				return new Color(ColorEnum.Empty);
			}
		}

		public static Color Both
		{
			get
			{
				return new Color(ColorEnum.Both);
			}
		}

		/// <summary>
		/// Gets a value indicating whether this color is black.
		/// </summary>
		/// <value><c>true</c> if this instance is black; otherwise, <c>false</c>.</value>
		public bool IsBlack
		{
			get
			{
				return mColor == ColorEnum.Black;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this color is white.
		/// </summary>
		/// <value><c>true</c> if this instance is white; otherwise, <c>false</c>.</value>
		public bool IsWhite
		{
			get
			{
				return mColor == ColorEnum.White;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is empty (no color).
		/// </summary>
		/// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
		public bool IsEmpty
		{
			get
			{
				return mColor == ColorEnum.Empty;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is the border (no color).
		/// </summary>
		/// <value><c>true</c> if this instance is the border; otherwise, <c>false</c>.</value>
		public bool IsBorder
		{
			get
			{
				return mColor == ColorEnum.Border;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is both colors.
		/// </summary>
		/// <value><c>true</c> if this instance is both colors; otherwise, <c>false</c>.</value>
		public bool IsBoth
		{
			get
			{
				return mColor == ColorEnum.Both;
			}
		}

		/// <summary>
		/// Gets the opposite.
		/// </summary>
		/// <value>The opposite.</value>
		public Color Opposite
		{
			get
			{
				return new Color((mColor == ColorEnum.White) ? ColorEnum.Black : ColorEnum.White);
			}
		}

		public int ToInteger()
		{
			return (byte)mColor;
		}

		public static bool IsValidColor(char color)
		{
			return ((Char.ToUpper(color) == 'B') || (Char.ToUpper(color) == 'W'));
		}

		public static bool IsValidColor(string color)
		{
			if (color.Length < 1)
				return false;

			return IsValidColor(color[0]);
		}

		public char ToChar2()
		{
			switch (mColor)
			{
				case ColorEnum.Black: return 'b';
				case ColorEnum.White: return 'w';
				case ColorEnum.Empty: return ' ';
				case ColorEnum.Border: return '#';
				default: return '?';	// should never get here
			}
		}

		public char ToChar()
		{
			switch (mColor)
			{
				case ColorEnum.Black: return 'X';
				case ColorEnum.White: return 'O';
				case ColorEnum.Empty: return '.';
				case ColorEnum.Border: return '#';
				default: return '?';	// should never get here
			}
		}

		public override string ToString()
		{
			return ToChar().ToString();
		}

		public string ToString2()
		{
			switch (mColor)
			{
				case ColorEnum.Black: return "Black";
				case ColorEnum.White: return "White";
				case ColorEnum.Empty: return "Empty";
				case ColorEnum.Border: return "Border";
				default: return "Unknown";	// should never get here
			}
		}

		public static bool operator ==(Color l, Color r)
		{
			if (object.ReferenceEquals(l, r))
				return true;
			else if (object.ReferenceEquals(l, null) ||
					 object.ReferenceEquals(r, null))
				return false;

			return (l.mColor == r.mColor);
		}

		public static bool operator !=(Color l, Color r)
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

			return (((Color)obj) == this);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
			return (byte)mColor;
		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<Color> Colors
		{
			get
			{
				yield return Color.Black;
				yield return Color.White;
			}
		}

	}
}
