using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // GET OBS FILTER DATA
        public bool GetObsMoveFilterDuration(string sourceName, string filterName, int obsConnection, out int moveDuration)
        {
            LogInfo($"Requesting move filter duration of [{sourceName}] - [{filterName}]");

            if (!GetObsSourceFilterData(sourceName, filterName, obsConnection, out JObject filterData))
            {
                LogError("Unable to retrieve move filter");
                moveDuration = -1;
                return false;
            }

            // Check filter settings exists
            if (filterData["filterSettings"] == null)
            {
                LogError("filterSettings not found in the response");
                moveDuration = -1;
                return false;
            }
            if (filterData["filterSettings"]["duration"] == null)
            {
                LogError("duration not found in filterSettings");
                moveDuration = -1;
                return false;
            }

            moveDuration = (int)filterData["filterSettings"]["duration"];

            LogInfo("Successfully retrieved move filter duration");
            return true;
        }


    }
}
