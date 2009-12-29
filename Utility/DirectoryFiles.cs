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
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace GoTraxx
{
	public class DirectoryFiles
	{
		protected List<string> Files;

		/// <summary>
		/// Initializes a new instance of the <see cref="DirectoryFiles"/> class.
		/// </summary>
		public DirectoryFiles()
		{
			Clear();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DirectoryFiles"/> class.
		/// </summary>
		/// <param name="directory">Directory.</param>
		public DirectoryFiles(string directory)
		{
			Clear();
			PopulateFiles(directory);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DirectoryFiles"/> class.
		/// </summary>
		/// <param name="directory">Directory.</param>
		/// <param name="mask">Mask.</param>
		public DirectoryFiles(string directory, string mask)
		{
			Clear();
			PopulateFiles(directory, mask, false);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DirectoryFiles"/> class.
		/// </summary>
		/// <param name="directory">Directory.</param>
		/// <param name="mask">Mask.</param>
		/// <param name="subDirectories">if set to <c>true</c> files in sub directories are included.</param>
		public DirectoryFiles(string directory, string mask, bool subDirectories)
		{
			Clear();
			PopulateFiles(directory, mask, subDirectories);
		}

		/// <summary>
		/// Clears this instance.
		/// </summary>
		public void Clear()
		{
			Files = new List<string>();
		}

		/// <summary>
		/// Gets the count.
		/// </summary>
		/// <value>The count.</value>
		public int Count
		{
			get
			{
				return Files.Count;
			}
		}

		/// <summary>
		/// Gets the <see cref="System.String"/> at the specified index.
		/// </summary>
		/// <value></value>
		public string this[int arg]
		{
			get
			{
				return Files[arg];
			}
		}

		/// <summary>
		/// Populates the files.
		/// </summary>
		/// <param name="directory">Directory.</param>
		/// <param name="mask">Mask.</param>
		/// <param name="subDirectories">if set to <c>true</c> files in sub directories are included.</param>
		/// <returns></returns>
		public bool PopulateFiles(string directory, string mask, bool subDirectories)
		{
			string lString = Directory.GetCurrentDirectory();

			Console.Error.WriteLine(lString);
	
			foreach (string lFile in Directory.GetFiles(directory, (mask == string.Empty) ? "*.*" : mask, subDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
				Files.Add(lFile);

			return true;
		}

		/// <summary>
		/// Populates the files.
		/// </summary>
		/// <param name="directory">Directory.</param>
		/// <param name="mask">Mask.</param>
		/// <returns></returns>
		public bool PopulateFiles(string directory, string mask)
		{
			return PopulateFiles(directory, mask, false);
		}

		/// <summary>
		/// Populates the files.
		/// </summary>
		/// <param name="directory">Directory.</param>
		/// <returns></returns>
		public bool PopulateFiles(string directory)
		{
			return PopulateFiles(directory, string.Empty, false);
		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns></returns>
		public System.Collections.IEnumerator GetEnumerator()
		{
			foreach (string lFile in Files)
				yield return lFile;
		}

	}
}
