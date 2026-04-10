using System.Collections.Generic;
using System.Linq;

namespace StreamUP
{
    partial class StreamUpLib
    {
        public List<int> GetRandomNumbers(int min, int max, int count = 1)
        {
            var numbers = Enumerable.Range(min, max - min).ToList();
            return numbers.OrderBy(x => random.Next()).Take(count).ToList();

        }
    }
}