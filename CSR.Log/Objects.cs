using System;

namespace CSR.Logging
{
    public class Objects
    {

    }

    public class ReturnValueInfo
    {
        public bool status { get; set; }
        public string message { get; set; }
        public string jsonData { get; set; }
    }

    public class ObjLogging
    {
        public string Key { get; set; }
        public string Command { get; set; }
    }

    public class ObjDefineColumn
    {
        public string Key { get; set; }
        public string Type { get; set; }
        public string Number { get; set; }
    }

    public class ObjData
    {
        public string Key { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }

    public class ObjQuery
    {
        public string key { get; set; }
        public string value { get; set; }
        public string condition { get; set; }
        public string operation { get; set; }
    }
}
