
using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

namespace StreamUP
{
    public class SimpleDatabase
    {
        private readonly string _filePath;
        private Dictionary<string, object> _data;

        public SimpleDatabase(string filePath)
        {
            _filePath = filePath;
            _data = Load();
        }

        private Dictionary<string, object> Load()
        {
            if (!File.Exists(_filePath))
                return new Dictionary<string, object>();

            var json = File.ReadAllText(_filePath);
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
        }

        private void Save()
        {
            var json = JsonConvert.SerializeObject(_data, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }

        public void Add(string key, object value)
        {

            if (value is string || value is int || value is decimal)
            {
                _data[key] = value;

            }
            else
            {
                _data[key] = value;

            }

            Save();
        }

        public void Update(string key, object newValue)
        {
            if (_data.ContainsKey(key))
            {

                if (newValue is string || newValue is int || newValue is decimal)
                {
                    _data[key] = newValue;

                }
                else
                {
                    _data[key] = newValue;

                }
            Save();
            }
            else
            {
                Add(key, newValue);
            }
           
            
        }

        public T Get<T>(string key, T defaultValue)
        {
            if (_data.TryGetValue(key, out var jsonValue))
            {
                if (typeof(T) == typeof(string))
                {
                    return (T)Convert.ChangeType(jsonValue, typeof(T));
                }
                else if (typeof(T) == typeof(int))
                {
                    //return int.Parse(jsonValue);
                    return (T)Convert.ChangeType(jsonValue, typeof(T));
                }
                else if (typeof(T) == typeof(decimal))
                {
                    //return decimal.Parse(jsonValue);
                    return (T)Convert.ChangeType(jsonValue, typeof(T));
                }
                else if (typeof(T) == typeof(Dictionary<string, bool>) || typeof(T) == typeof(Dictionary<string, string>) || typeof(T) == typeof(Dictionary<string, int>))
                {
                    var dictValue = JsonConvert.DeserializeObject(jsonValue.ToString(), typeof(T));
                    return (T)dictValue;

                }
                else if(typeof(T) == typeof(List<string>))
                {
                  
                    var listValue = JsonConvert.DeserializeObject(jsonValue.ToString(), typeof(T));
                    return (T)Convert.ChangeType(listValue, typeof(T));


                }
                else
                {

                    return defaultValue;
                }
                
            }
            return defaultValue;
        }





        public void Delete(string key)
        {
            if (_data.Remove(key))
            {
                Save();
            }
        
        }

        public IEnumerable<string> GetAllKeys()
        {
            return _data.Keys;
        }
    }
}
