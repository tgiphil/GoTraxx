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
    public enum SafetySolverType
    {
        Unassigned = 0,
        None = 1,
        Benson = 2,
        Muller97 = 3,
        Muller04 = 4,
		Muller04Plus = 5,
		Muller97Plus = 6,
		BensonPlus = 7,
    };

    interface ISafetySolverInterface
    {
        void Solve(GoBoard goBoard, Color color);
        void UpdateSafetyKnowledge(SafetyMap safetyMap);
    }

    class SafetySolverFactory
    {
		protected Dictionary<string, SafetySolverType> Dictionary;
		protected static SafetySolverFactory Instance = new SafetySolverFactory();

		protected SafetySolverFactory()
		{
			Dictionary = new Dictionary<string, SafetySolverType>();

			Add("benson", SafetySolverType.Benson);
			Add("muller97", SafetySolverType.Muller97);
			Add("muller04", SafetySolverType.Muller04);
			Add("muller", SafetySolverType.Muller04);
			Add("muller+", SafetySolverType.Muller04Plus);
			Add("muller04+", SafetySolverType.Muller04Plus);
			Add("muller97+", SafetySolverType.Muller97Plus);
			Add("benson+", SafetySolverType.BensonPlus);
			Add("static", SafetySolverType.Muller04);
			Add("none", SafetySolverType.None);
		}

		protected void Add(string typeName, SafetySolverType solverType)
		{
			Dictionary.Add(typeName, solverType);
		}

        public static ISafetySolverInterface CreateFactory(SafetySolverType safetySolverType)
        {
            switch (safetySolverType)
            {
                case SafetySolverType.Benson: return new SolverBenson();
                case SafetySolverType.Muller97: return new SolverMuller(1997);
                case SafetySolverType.Muller04: return new SolverMuller(2004);
				case SafetySolverType.Muller97Plus: return new SolverExtended(SafetySolverType.Muller97);
				case SafetySolverType.Muller04Plus: return new SolverExtended(SafetySolverType.Muller04);
				case SafetySolverType.BensonPlus: return new SolverExtended(SafetySolverType.Benson);
                default: return new SolverNull();
            }
        }

        public static ISafetySolverInterface CreateFactory(string name)
        {
            return CreateFactory(ToType(name));
        }

		public static SafetySolverType ToType(string name)
		{
			SafetySolverType lSafetySolverType = SafetySolverType.Unassigned;

			if (!Instance.Dictionary.TryGetValue(name.Trim().ToLower(), out lSafetySolverType))
				return SafetySolverType.Unassigned;

			return lSafetySolverType;
		}

    }
}
