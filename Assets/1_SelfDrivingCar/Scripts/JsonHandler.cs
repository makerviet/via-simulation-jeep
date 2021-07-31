using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class JsonHandler {
    
    public static string FromDictionaryToJson(Dictionary<string, string> dictionary)
    {
        var kvs = dictionary.Select(kvp => string.Format("\"{0}\":\"{1}\"", kvp.Key, kvp.Value));
        return string.Concat("{", string.Join(",", kvs), "}");
    }

    public static Dictionary<string, string> FromJsonToDictionary(string json)
    {
        string[] keyValueArray = json.Replace("{", string.Empty).Replace("}", string.Empty).Replace("\"", string.Empty).Split(',');
        return keyValueArray.ToDictionary(item => item.Split(':')[0].Trim(), item => item.Split(':')[1].Trim(',').Trim());
    }

}