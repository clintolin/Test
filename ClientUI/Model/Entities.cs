// Entities.cs
//

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xrm.Sdk;

namespace ClientUI.Model
{
    public class TestCase : Entity
    {
        public static new string LogicalName = "rta_testcase";
        public TestCase()
            : base("rta_testcase")
        {

        }

        [ScriptName("rta_testcaseid")]
        public string rta_testcaseid;

        [ScriptName("rta_name")]
        public string rta_name;
    }
}
