using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {


        public bool SetBBCodeText(string text, string sourceName, int connection = 0)
        {
            string response = _CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup-bbcode\",\"requestType\":\"set_text\",\"requestData\":{\"sourceName\":\"" + sourceName + "\",\"text\":\"" + text + "\"}}", connection);
            var responseObj = JObject.Parse(response);
            bool success = responseObj["responseData"]["success"].Value<bool>();
            return success;
        }
        public bool SetBBCodeVar(string varName, string value, int connection = 0)
        {
            string response = _CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup-bbcode\",\"requestType\":\"set_variable\",\"requestData\":{\"name\":\"" + varName + "\",\"value\":\"" + value + "\"}}", connection);
            var responseObj = JObject.Parse(response);
            bool success = responseObj["responseData"]["success"].Value<bool>();
            return success;
        }

        public bool SetGlobalTags(string tags, string source, int connection = 0)
        {
            string response = _CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup-bbcode\",\"requestType\":\"set_global_tags\",\"requestData\":{\"sourceName\":\"" + source + "\",\"tags\":\"" + tags + "\"}}", connection);

            var responseObj = JObject.Parse(response);
            bool success = responseObj["responseData"]["success"].Value<bool>();
            return success;
        }

        public string GetBBCodeText(string source, int connection = 0)
        {
            string response = _CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup-bbcode\",\"requestType\":\"get_text\",\"requestData\":{\"sourceName\":\"" + source + "\"}}", connection);
            var responseObj = JObject.Parse(response);
            string text = responseObj["responseData"]["text"].Value<string>();
            return text;
        }

        public string GetGlobalTags(string sourceName, int connection = 0)
        {
            string response = _CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup-bbcode\",\"requestType\":\"get_global_tags\",\"requestData\":{\"sourceName\":\"" + sourceName + "\"}}", connection);
            var responseObj = JObject.Parse(response);
            string success = responseObj["responseData"]["tags"].Value<string>();
            return success;
        }


        public string GetBBCodeVar(string varName, int connection = 0)
        {

            string response = _CPH.ObsSendRaw("CallVendorRequest", "{\"vendorName\":\"streamup-bbcode\",\"requestType\":\"get_variable\",\"requestData\":{\"name\":\"" + varName + "\"}}", connection);
            var responseObj = JObject.Parse(response);
            string text = responseObj["responseData"]["value"].Value<string>();
            return text;
        }


    }
}