using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Kyozy.MiniblinkNet
{
    public class JsObject
    {
        internal Int64 _jsGetProperty(IntPtr es, Int64 obj, string propertyName)
        {
            return jsGetProperty(es, obj, propertyName);
        }

        internal byte _jsSetProperty(IntPtr es, Int64 obj, string propertyName, Int64 value)
        {
            return jsSetProperty(es, obj, propertyName, value) ? (byte)1 : (byte)0;
        }

        internal void _jsFinalize(IntPtr data)
        {
            jsFinalize();
            Marshal.FreeHGlobal(data);
        }

        public virtual JsValue jsGetProperty(IntPtr jsExecState, JsValue obj, string propertyName)
        {
            return MBApi.jsUndefined();
        }
        public virtual bool jsSetProperty(IntPtr jsExecState, JsValue obj, string propertyName, JsValue value)
        {
            return false;
        }

        public virtual void jsFinalize()
        {

        }
    }
}
