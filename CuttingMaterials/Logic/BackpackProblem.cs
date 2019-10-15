using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingMaterials.Logic
{
    class BackpackProblem
    {
        List<int> entries;
        public BackpackProblem(List<int> entries)
        {
            this.entries = entries;
        }
       
        ///<summary>
        ///Решает задачу о рбкзаке методом динамического програмирования 
        /// </summary>
        /// <param name="m">Объем рюкзака</param>
        /// <param name="n">Кол-во предметов</param>
        /// <returns></returns>
        public List<int> CalcDP(int m, int n)
        {
            var a = new int[m + 1, n + 1];

            for (int i = 1; i <= m; i++)
            {
                if (i >= entries[0])
                    a[i, 1] = entries[0];
            }

            for (int i = 2; i <= n; i++)
            {
                for (int x = 1; x <= m; x++)
                {
                    if (x - entries[i - 1] < 0)
                        a[x, i] = a[x, i - 1];
                    else
                    {
                        int a1 = a[x, i - 1];
                        int a2 = a[x - entries[i - 1], i - 1] + entries[i - 1];

                        a[x, i] = a2 > a1 ? a2 : a1;
                    }
                }
            }

            return GetComb(new List<int>(), a, m, n);
        }

        List<int> GetComb(List<int> comb, int[,] a, int m, int n)
        {
            if (n < 1)
                return comb;

            int a1 = a[m, n - 1];
            int a2 = 0;
            if (m >= entries[n - 1])
                a2 = a[m - entries[n - 1], n - 1] + entries[n - 1];

            if (a1 == a2 || a1 > a2)
               GetComb(comb, a, m, n - 1);

            if (a2 > a1)
            {
                comb.Add(entries[n - 1]);
                GetComb(comb, a, m - entries[n - 1], n - 1);
            }

            return comb;
        }
    }
}
