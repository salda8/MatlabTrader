using System.Collections.Generic;
using IBApi;

namespace MATLAB_trader.Common
{
    internal class Tools
    {
        public static List<TagValue> GetFakeParameters(int numParams)
        {
            var fakeParams = new List<TagValue>();
            for (var i = 0; i < numParams; i++)
                fakeParams.Add(new TagValue("tag" + i, i.ToString()));
            return fakeParams;
        }

        public static bool IsOdd(int value)
        {
            return value%2 != 0;
        }
    }
}