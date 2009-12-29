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
	public class SimpleTimer
	{
		protected int StartTick;
		protected int EndTick;

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleTimer"/> class.
		/// </summary>
		public SimpleTimer()
		{
			StartTick = Environment.TickCount;
			EndTick = 0;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleTimer"/> class.
		/// </summary>
		/// <param name="simpleTimer">The simple timer.</param>
		public SimpleTimer(SimpleTimer simpleTimer)
		{
			StartTick = simpleTimer.StartTick;
			EndTick = simpleTimer.EndTick;
		}

		/// <summary>
		/// Clones this instance.
		/// </summary>
		/// <returns></returns>
		public SimpleTimer Clone()
		{
			SimpleTimer lSimpleTimer = new SimpleTimer();

			lSimpleTimer.StartTick = StartTick;
			lSimpleTimer.EndTick = EndTick;

			return lSimpleTimer;
		}

		/// <summary>
		/// Restarts the timer.
		/// </summary>
		public void Restart()
		{
			StartTick = Environment.TickCount;
			EndTick = 0;
		}

		/// <summary>
		/// Stops the timer.
		/// </summary>
		public void Stop()
		{
			EndTick = Environment.TickCount;
		}

		/// <summary>
		/// Gets the start time.
		/// </summary>
		/// <value>The start time.</value>
		public DateTime StartTime
		{
			get
			{
				return new DateTime(StartTick);
			}
		}

		/// <summary>
		/// Gets the end time.
		/// </summary>
		/// <value>The end time.</value>
		public DateTime EndTime
		{
			get
			{
				if (EndTick == 0)
					return DateTime.Now;
				else
					return new DateTime(EndTick);
			}
		}

		/// <summary>
		/// Gets the elapsed ticks.
		/// </summary>
		/// <value>The elapsed ticks.</value>
		public long ElapsedTicks
		{
			get
			{
				if (EndTick == 0)
					return Environment.TickCount - StartTick;
				else
					return EndTick - StartTick;
			}
		}

		/// <summary>
		/// Gets the elapsed time.
		/// </summary>
		/// <value>The elapsed.</value>
		public TimeSpan Elapsed
		{
			get
			{
				return new TimeSpan(ElapsedTicks);
			}
		}

		/// <summary>
		/// Gets the milliseconds.
		/// </summary>
		/// <returns></returns>
		public long MilliSecondsElapsed
		{
			get
			{
				return ElapsedTicks;
			}
		}

		/// <summary>
		/// Gets the seconds elapsed.
		/// </summary>
		/// <returns></returns>
		public long SecondsElapsed
		{
			get
			{
				return MilliSecondsElapsed / 1000;
			}
		}

		/// <summary>
		/// Gets the minutes elapsed.
		/// </summary>
		/// <returns></returns>
		public long MinutesElapsed
		{
			get
			{
				return MilliSecondsElapsed / 1000 / 60;
			}
		}

	}
}
