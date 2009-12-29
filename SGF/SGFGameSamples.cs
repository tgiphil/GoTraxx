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

/*
 * The 1993-ZHONG-JIALIN-HANE-YASUMASA-1 has the following copyright:
 * 
 * Copyright 1993 Tim L. Casey
 * 
 * Permission to reproduce this game is given, provided proper credit is given.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace GoTraxx
{
	public static class SGFGameSamples
	{
		static public string BENSON_SAFE_1
		{
			get
			{
				return @"(;SZ[19]AW[ab][ac][bc][ba][ca][cb][qb][rb][rc][sc][sa][qa][sr][rq][rs][qs][qr][se][re][rf][rg][sg][rh][ri][si][sk][rk][ql][rm][pm][qn][sl][no][op][mp][mq][nq][mr][lr][ls][ns][or][os][sm][me][md][nd][od][oe][nf][og][ng][pf][pg][oa][ob][nb][mb][la][kb][jb][ib][hb][gb][ga][ma][ja][rp][sp][dm][cm][bn][bo][bp][cr][dr][er][fr][gq][gp][gi][fi][eh][dh][dg][df][de][ed][fd][gd][he][hf][ii][hd][ih][ei][nn][pp][om][pn][po][if][ig][hi][ge][jg][kf][je][kg][id][bq][cp][dq][cn][fq][ho][hn][hm][fl][gk][fk][hl][gm][ek][dk][dl][hp]AB[co][dn][en][fp][fo][ef][eg][fg][fh][gh][gf][fe][ee][hg][hh][eq][do][ep][dp][em][gn][fm][go])";
			}
		}

		static public string BENSON_SAFE_2
		{
			get
			{
				return @"(;SZ[19]AW[jg][jh][sr][ss][ag][bg][bf][dr][ds]AB[gb][ge][he][ie][re][se][gf][if][jf][kf][rf][hg][kg][rg][sg][hh][ih][kh][qh][sh][ji][ki][pi][ri][si][pj][qj][sj][rk][rl][sl][mn][jo][kp][lp][rq][qr][ko][ln][km][lm][qq][rp][sp][ro][sn][rn][no][jm][in][io][mq][mr][np][op][lr][kr][jq][oq][ns][or][os][jr][qs][nn][eb][gc][ib][ab][bb][cb][cc][dc][da][fc][fa][hb][ia][ka][kb][lb][lc][mc][ma][nb][ob][pb][nd][pd][od][rc][sc][pc][qc][cg][cf][be][ce][ae][ah][bh][di][ci][cj][dj][dh][bj][aj][cs][cr][cq][cp][co][cn][cm][cl][bl][al][ao][bo][dq][eq][fr][gr][gs][es])";
			}
		}


		static public string BENSON_SAFE_3
		{
			get
			{
				return @"(;SZ[19]AW[ih][hj][hi][ij][kh][kj][lj][li][mj][nj][oj][oi][pi][ph][pg][pf][of][oe][ne][le][me][lf][kf][kg][ig][if][hf][he][fe][ge][ee][ef][dg][dh][di][ei][fj][ej][gj][mk][ml][mm][lm][ln][kn][jn][in][hn][hm][gm][gl][gk][df][ji]AB[hh][lh][jk][lg][mg][mi][ni][nh][oh][og][mf][nf][hg][gg][gf][ff][gi][fi][fh][eh][eg][ik][kk][jm][hk][hl][jl][lk][ll][km][im][jj][jh][jg][jf][ie][je][ke][id][hd][gd][fd][ed][dd][ce][de][cf][cg][ch][ci][dj][cj][ek][fk][fl][fm][fn][gn][ho][io][ko][lo][mn][nn][nm][nl][nk][ok][pk][pj][qj][qi][qh][qg][qf][qe][pe][od][nd][md][ld][kd][ip][iq][kp][kq][mo][go][ir][is][ks][kr][hs][ls][ms][ns][os][pd][gs][fs][es][ds][cs][bs][ar][aq][ap][ao][an][am][al][ak][aj][ai][ah][ag][af][ae][ad][ac][ab][ba][ca][da][ea][fa][ga][ha][ia][ja][ka][la][ma][na][oa][pa][qa][ra][sc][sb][sd][se][sf][sg][sh][si][sj][sk][sl][sm][sn][so][sp][sq][sr][rs][qs][ps][dk][aa][sa])";
			}
		}


		static public string BENSON_UNSAFE
		{
			get
			{
				return @"(;SZ[19]AW[sa][ac][bc][rc][sc][ae][be][ce][cf][cg][ah][bh][di][aj][bj][cj][dj][cq][dq][eq][cr][fr][gr][cs][es][gs][dh][ci][ss][cb][ba][ca][db][eb][ea][pb][pa][rb][qb][cn][cm][cl][dl][el][fl][fm][fn][eo][do][co][fo][em][ii][ij][jj][ki][kh][jh][jg][jf][hi][gh][gi][fh][fg][ff][ge][he][ie][je][fe]AB[hb][re][se][rf][qh][sh][rk][rl][sl][mn][jo][kp][lp][rq][dr][qr][ds][ko][ln][km][lm][qq][rp][sp][ro][sn][rn][no][jm][in][io][mq][mr][np][lr][kr][oq][ns][or][os][qs][aa][ja][jc][ib][kb][lb][mb][ma][ic][gb][ga][ri][rj][jq][qg][sg][dm][en][om][pm][pl][pk][pj][oj][nj][nl][ml][mk][lj][ni][mi][li][pc][pd][pe][of][og][ng][mf][ne][me][md][nc][oc][gf][gg][hh][ih][ig][if][hf][ir][js][is][ls][nn])";
			}
		}


		static public string DYER
		{
			get
			{
				return @"(;FF[4]CA[UTF-8]AP[GoGui:1.0.2]SZ[7]KM[6.5]DT[2006-10-08]AB[ag][af][ae][ad][ab][bg][bf][be][bd][bc][bb][ba][cg][cf][ce][cd][dg][df][de][dd][dc][eg][ef][ee][ed][ea][fg][ff][fe][fd][gg][gf][ge][gd]AW[cc][cb][db][ec][fc][fa][gc]PL[B])";
			}
		}

		static public string GAME_001_001
		{
			get
			{
				return @"(;GM[1]VW[]FF[1]DT[July 7, 1948]SZ[19]PB[Iwamoto Kaoru]PW[Go Seigen]KM[0]C[White wins by 1 or 2 points.]GN[001_001]RE[White wins by 1 or 2 points.];B[qd];W[dc];B[pp];W[dq];B[od];W[cf];B[co];W[dl];B[fo];W[gq];B[jc];W[qi];B[fc];W[hc];B[de];W[ce];B[db];W[dd];B[cb];W[fd];B[gc];W[gd];B[hb];W[hd];B[ib];W[qn];B[np];W[rp];B[qq];W[pl];B[iq];W[cm];B[gk];W[eo];B[cq];W[cr];B[dp];W[bq];B[ep];W[cp];B[bp];W[fm];B[eq];W[cq];B[en];W[go];B[gr];W[er];B[fr];W[hl];B[ij];W[hq];B[gn];W[fn];B[do];W[ho];B[es];W[bo];B[hr];W[qf];B[em];W[fl];B[el];W[fk];B[ek];W[fj];B[kf];W[pc];B[pd];W[mp];B[no];W[ko];B[bn];W[ap];B[im];W[mq];B[nq];W[in];B[ej];W[hm];B[cn];W[kh];B[ph];W[pi];B[oh];W[qh];B[pf];W[qc];B[oc];W[rd];B[re];W[rc];B[qe];W[ob];B[nb];W[pb];B[ci];W[jf];B[je];W[jg];B[ie];W[hg];B[mo];W[an];B[am];W[ao];B[bm];W[lf];B[lg];W[kg];B[ke];W[mb];B[nc];W[na];B[rq];W[jq];B[mr];W[lm];B[mk];W[lj];B[mg];W[bh];B[ch];W[bi];B[bj];W[eh];B[lk];W[kk];B[ok];W[pk];B[kl];W[jk];B[ll];W[ir];B[lo];W[qo];B[sh];W[si];B[jl];W[ki];B[kp];W[jp];B[rb];W[sb];B[oa];W[pa];B[dg];W[cg];B[dh];W[cj];B[ck];W[eg];B[df];W[aj];B[dj];W[bc];B[cc];W[cd];B[bb];W[ec];B[eb];W[lc];B[ff];W[ac];B[gg];W[gh];B[hf];W[ig];B[kb];W[ld];B[le];W[md];B[ra];W[sd];B[lb];W[mc];B[me];W[lq];B[lr];W[kr];B[jo];W[ls];B[nr];W[il];B[kn];W[kq];B[pm];W[qm];B[ol];W[on];B[om];W[oo];B[qp];W[ro];B[se];W[qb];B[dr];W[nn];B[ei];W[fi];B[rg];W[rh];B[qg];W[sq];B[sr];W[sp];B[sg];W[rr];B[rj];W[ri];B[pj];W[qj];B[oj];W[qk];B[rs];W[ss];B[ab];W[be];B[sr];W[ak];B[qr];W[bk];B[bl];W[cj];B[ar];W[br];B[bj];W[bg];B[al];W[cj];B[la];W[ma];B[bj];W[ai];B[nd];W[qa];B[mm];W[nm];B[nl];W[hs];B[gs];W[is];B[io];W[ip];B[jm];W[mn];B[ln];W[ml];B[jn];W[hn];B[mm];W[nh];B[ng];W[mi];B[gp];W[hp];B[fp];W[ml];B[ef];W[fh];B[mm];W[op];B[oq];W[ml];B[kd];W[sa];B[mm];W[cs];B[ms];W[ml];B[fe];W[ed];B[mm];W[ds];B[er];W[ml];B[kc];W[oa];B[mm];W[ks];B[ni];W[mh];B[lh];W[li];B[oi];W[ml];B[po];W[pn];B[mm];W[mj];B[lp];W[ml];B[rl];W[ql];B[mm];W[nj];B[cj];W[id];B[fg];W[ml];B[bd];W[ad];B[mm];W[ge];B[nk];W[ml];B[af];W[ae];B[mm];W[gf];B[if];W[ml];B[ik];W[hh];B[mm];W[ic];B[jd];W[ml];B[kj];W[jj];B[mm];W[he];B[fq];W[ml];B[ji];W[kj];B[mm];W[ee])";
			}
		}


		static public string GAME_1993_ZHONG_JIALIN_HANE_YASUMASA_1
		{
			get
			{
				return @"(;GM[1]VW[]FF[1]DT[December 11, 1993]SZ[19]PB[Zhong Jialin]PW[Hane Yasumasa]KM[0.5]CP[Copyright 1993 Tim L. Casey];AB[pd]AB[dp];W[qp];B[dd];W[fq];B[op];W[lp];B[pn];W[oo];B[po];W[pp];B[no];W[oq];B[on];W[nq];B[ln];W[qf];B[qh];W[ph];B[pi];W[pg];B[qi];W[nd];B[ne];W[me];B[oe];W[qc];B[qd];W[pc];B[rc];W[od];B[pe];W[rb];B[rd];W[mf];B[ng];W[md];B[mg];W[lg];B[lh];W[kh];B[li];W[ki];B[lj];W[kj];B[lk];W[fd];B[kg];W[lf];B[hd];W[ff];B[fc];W[gc];B[gd];W[ec];B[fe];W[fb];B[ed];W[fc];B[ge];W[ic];B[ef];W[kk];B[kl];W[jl];B[km];W[rn];B[id];W[jc];B[jg];W[ih];B[jh];W[ij];B[hl];W[gj];B[ji];W[hk];B[gl];W[ej];B[el];W[eg];B[dg];W[df];B[ee];W[eh];B[cg];W[if];B[eq];W[fp];B[ip];W[do];B[co];W[dn];B[ep];W[fo];B[cn];W[cp];B[cq];W[bp];B[bq];W[cm];B[bo];W[dl];B[eo];W[en];B[fn];W[er];B[ap];W[io];B[ho];W[hp];B[hq];W[hn];B[gp];W[go];B[jo];W[hp];B[fm];W[gq];B[dk];W[ch];B[dc];W[ck];B[dm];W[bg];B[dh];W[di];B[cf];W[bi];B[bf];W[af];B[ae];W[ag];B[bd];W[ko];B[rm];W[nf];B[of];W[og];B[sb];W[ra];B[rf];W[oh];B[nh];W[ni];B[mh];W[ro];B[qm];W[gf];B[jd];W[kd];B[hc];W[hb];B[hf];W[ig];B[hg];W[hh];B[gh];W[jn];B[cl];W[bk];B[dj];W[cj];B[dr];W[db];B[cb];W[ca];B[ba];W[da];B[bb];W[je];B[fr];W[gr];B[es];W[fg];B[gs];W[hr];B[hs];W[is];B[fs];W[ir];B[mp];W[mq];B[lo];W[jp];B[fi];W[ei];B[gg];W[bm];B[bl];W[al];B[dl];W[gi];B[sn];W[so];B[sm];W[fh];B[he];W[np];B[mo];W[il];B[fk];W[fj];B[an];W[am];B[hm];W[im];B[gn];W[qo];B[oo];W[kn];B[qn];W[qb];B[sc];W[sa];B[qg];W[ak];B[bn];W[gk];B[ek];W[jm];B[ie];W[tt];B[tt];W[tt])";
			}
		}


	}
}

