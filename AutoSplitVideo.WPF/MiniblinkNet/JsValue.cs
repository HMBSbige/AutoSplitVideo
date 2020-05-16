using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Kyozy.MiniblinkNet
{
    public delegate long jsCallAsFunction(IntPtr jsExecState, long obj, JsValue[] args);

    public struct JsValue
    {
        private static Dictionary<string, wkeJsNativeFunction> sm_jsFunctionDictionary = new Dictionary<string, wkeJsNativeFunction>();
        private static Dictionary<IntPtr, jsCallAsFunctionCallback> sm_jsCallAsFunctionDictionary = new Dictionary<IntPtr, jsCallAsFunctionCallback>();
        private static jsFinalizeCallback sm_finalize = new jsFinalizeCallback(jsFinalize);

        

        public Int64 Value;

        public JsValue(Int64 value)
        {
            this.Value = value;
        }

        /// <summary>
        /// 重载隐式转换
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static implicit operator Int64(JsValue v)
        {
            return v.Value;
        }
        /// <summary>
        /// 重载隐式转换
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static implicit operator JsValue(Int64 v)
        {
            return new JsValue(v);
        }


        /// <summary>
        /// 获取值类型
        /// </summary>
        /// <returns></returns>
        public jsType GetValueType()
        {
            return MBApi.jsTypeOf(Value);
        }
        /// <summary>
        /// 转换到Int32
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <returns></returns>
        public int ToInt32(IntPtr jsExecState)
        {
            return MBApi.jsToInt(jsExecState, Value);
        }
        /// <summary>
        /// 转换到float
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <returns></returns>
        public float ToFloat(IntPtr jsExecState)
        {
            return MBApi.jsToFloat(jsExecState, Value);
        }
        /// <summary>
        /// 转换到double
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <returns></returns>
        public double ToDouble(IntPtr jsExecState)
        {
            return MBApi.jsToDouble(jsExecState, Value);
        }
        /// <summary>
        /// 转换到bool
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <returns></returns>
        public bool ToBoolean(IntPtr jsExecState)
        {
            return MBApi.jsToBoolean(jsExecState, Value) != 0;
        }
        /// <summary>
        /// 转换到string
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <returns></returns>
        public string ToString(IntPtr jsExecState)
        {
            IntPtr pStr = MBApi.jsToTempStringW(jsExecState, Value);
            if (pStr == IntPtr.Zero)
            {
                return null;
            }
            return Marshal.PtrToStringUni(pStr);
        }
        /// <summary>
        /// 获取属性
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <param name="propName">属性名</param>
        /// <returns></returns>
        public JsValue GetProp(IntPtr jsExecState, string propName)
        {
            return MBApi.jsGet(jsExecState, this.Value, propName);
        }
        /// <summary>
        /// 设置属性
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <param name="propName">属性名</param>
        /// <param name="v">jsValue</param>
        public void SetProp(IntPtr jsExecState, string propName, JsValue v)
        {
            MBApi.jsSet(jsExecState, this.Value, propName, v.Value);
        }

        /// <summary>
        /// 删除属性
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <param name="propName">属性名</param>
        public void DeleteProp(IntPtr jsExecState, string propName)
        {
            MBApi.jsDeleteObjectProp(jsExecState, this.Value, propName);
        }

        /// <summary>
        /// 获取属性自索引
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <param name="index">从0开始的索引</param>
        /// <returns></returns>
        public JsValue GetPropAt(IntPtr jsExecState, int index)
        {
            return MBApi.jsGetAt(jsExecState, this.Value, index);
        }
        /// <summary>
        /// 设置属性自索引
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <param name="index">从0开始的索引</param>
        /// <param name="v">jsValue</param>
        public void SetPropAt(IntPtr jsExecState, int index, JsValue v)
        {
            MBApi.jsSetAt(jsExecState, this.Value, index, v.Value);
        }
        /// <summary>
        /// 获取成员数
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <returns></returns>
        public int GetLength(IntPtr jsExecState)
        {
            return MBApi.jsGetLength(jsExecState, this.Value);
        }
        /// <summary>
        /// 设置成员数
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <param name="length"></param>
        public void SetLength(IntPtr jsExecState, int length)
        {
            MBApi.jsSetLength(jsExecState, this.Value, length);
        }
        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <param name="function"></param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        public JsValue Call(IntPtr jsExecState, JsValue function, params JsValue[] args)
        {
            int count = args.Length;
            long[] longArgs = new long[count];
            for (int i = 0; i < count; i++)
            {
                longArgs[i] = args[i].Value;
            }
            return MBApi.jsCall(jsExecState, function, this.Value, longArgs, count);
        }

        /// <summary>
        /// 获取数组缓冲区
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <returns></returns>
        public wkeMemBuf GetArrayBuffer(IntPtr jsExecState)
        {
            wkeMemBuf buf = new wkeMemBuf();
            IntPtr pBuff = MBApi.jsGetArrayBuffer(jsExecState, this.Value);
            if (pBuff != IntPtr.Zero)
            {
                buf = (wkeMemBuf)Marshal.PtrToStructure(pBuff, typeof(wkeMemBuf));
            }
            return buf;
        }

        /// <summary>
        /// 获取对象Keys
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <returns></returns>
        public string[] GetKeys(IntPtr jsExecState)
        {
            IntPtr jsKeys = MBApi.jsGetKeys(jsExecState, this.Value);
            if(jsKeys!=IntPtr.Zero)
            {
                int len = Marshal.ReadInt32(jsKeys);
                int sizePtr = Marshal.SizeOf(typeof(IntPtr));
                IntPtr ppKeys = Marshal.ReadIntPtr(jsKeys, sizePtr );
                string[] keys = new string[len];
                for (int i = 0; i < len; i++)
                {
                    keys[i] = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(ppKeys, sizePtr * i));
                }
                return keys;
            }
            return null;
        }

        #region 属性
        /// <summary>
        /// JsValue是否数值型
        /// </summary>
        public bool IsNumber
        {
            get { return MBApi.jsIsNumber(Value) != 0; }
        }
        /// <summary>
        /// JsValue是否文本型
        /// </summary>
        public bool IsString
        {
            get { return MBApi.jsIsString(Value) != 0; }
        }
        /// <summary>
        /// JsValue是否逻辑型
        /// </summary>
        public bool IsBoolean
        {
            get { return MBApi.jsIsBoolean(Value) != 0; }
        }
        /// <summary>
        /// JsValue是否对象型
        /// </summary>
        public bool IsObject
        {
            get { return MBApi.jsIsObject(Value) != 0; }
        }
        /// <summary>
        /// JsValue是否方法型
        /// </summary>
        public bool IsFunction
        {
            get { return MBApi.jsIsFunction(Value) != 0; }
        }
        /// <summary>
        /// JsValue是否未定义
        /// </summary>
        public bool IsUndefined
        {
            get { return MBApi.jsIsUndefined(Value) != 0; }
        }
        /// <summary>
        /// JsValue是否 null
        /// </summary>
        public bool IsNull
        {
            get { return MBApi.jsIsNull(Value) != 0; }
        }
        /// <summary>
        /// JsValue是否数组
        /// </summary>
        public bool IsArray
        {
            get { return MBApi.jsIsArray(Value) != 0; }
        }
        /// <summary>
        /// JsValue是否 true
        /// </summary>
        public bool IsTrue
        {
            get { return MBApi.jsIsTrue(Value) != 0; }
        }
        /// <summary>
        /// JsValue是否 false
        /// </summary>
        public bool IsFalse
        {
            get { return MBApi.jsIsFalse(Value) != 0; }
        }



        #endregion

        #region 静态方法
        /// <summary>
        /// 绑定JS方法
        /// </summary>
        /// <param name="name">js方法名</param>
        /// <param name="fn">long (IntPtr jsExecState, IntPtr userData)</param>
        /// <param name="userData">用户数据</param>
        /// <param name="argCount">参数个数</param>
        public static void BindFunction(string name, wkeJsNativeFunction fn, IntPtr userData, uint argCount)
        {
            sm_jsFunctionDictionary[name] = fn;
            MBApi.wkeJsBindFunction(name, fn, userData, argCount);
        }
        /// <summary>
        /// 绑定JS方法
        /// </summary>
        /// <param name="name">js方法名</param>
        /// <param name="fn">long (IntPtr jsExecState, IntPtr userData)</param>
        /// <param name="argCount">参数个数</param>
        public static void BindFunction(string name, wkeJsNativeFunction fn, uint argCount)
        {
            sm_jsFunctionDictionary[name] = fn;
            MBApi.wkeJsBindFunction(name, fn, IntPtr.Zero, argCount);
        }
        /// <summary>
        /// 绑定JS取属性
        /// </summary>
        /// <param name="name">属性名</param>
        /// <param name="fn">long (IntPtr jsExecState, IntPtr userData)</param>
        /// <param name="userData">用户数据</param>
        public static void BindGetter(string name, wkeJsNativeFunction fn, IntPtr userData)
        {
            sm_jsFunctionDictionary[name] = fn;
            MBApi.wkeJsBindGetter(name, fn, userData);
        }
        /// <summary>
        /// 绑定JS取属性
        /// </summary>
        /// <param name="name">属性名</param>
        /// <param name="fn">long (IntPtr jsExecState, IntPtr userData)</param>
        public static void BindGetter(string name, wkeJsNativeFunction fn)
        {
            sm_jsFunctionDictionary[name] = fn;
            MBApi.wkeJsBindGetter(name, fn, IntPtr.Zero);
        }
        /// <summary>
        /// 绑定JS置属性
        /// </summary>
        /// <param name="name">属性名</param>
        /// <param name="fn">long (IntPtr jsExecState, IntPtr userData)</param>
        /// <param name="userData">用户数据</param>
        public static void BindSetter(string name, wkeJsNativeFunction fn, IntPtr userData)
        {
            sm_jsFunctionDictionary[name] = fn;
            MBApi.wkeJsBindSetter(name, fn, userData);
        }
        /// <summary>
        /// 绑定JS置属性
        /// </summary>
        /// <param name="name">属性名</param>
        /// <param name="fn">long (IntPtr jsExecState, IntPtr userData)</param>
        public static void BindSetter(string name, wkeJsNativeFunction fn)
        {
            sm_jsFunctionDictionary[name] = fn;
            MBApi.wkeJsBindSetter(name, fn, IntPtr.Zero);
        }
        /// <summary>
        /// 获取参数个数
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <returns></returns>
        public static int ArgCount(IntPtr jsExecState)
        {
            return MBApi.jsArgCount(jsExecState);
        }
        /// <summary>
        /// 获取参数类型
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <param name="argIndex"></param>
        /// <returns></returns>
        public static jsType ArgType(IntPtr jsExecState, int argIndex)
        {
            return MBApi.jsArgType(jsExecState, argIndex);
        }
        /// <summary>
        /// 获取参数
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <param name="argIndex">从0开始的索引</param>
        /// <returns></returns>
        public static JsValue Arg(IntPtr jsExecState, int argIndex)
        {
            return MBApi.jsArg(jsExecState, argIndex);
        }
        /// <summary>
        /// 获取代表 int 的jsValue
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static JsValue Int32Value(int n)
        {
            return MBApi.jsInt(n);
        }
        /// <summary>
        /// 获取代表 float 的 jsValue
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static JsValue FloatValue(float f)
        {
            return MBApi.jsFloat(f);
        }
        /// <summary>
        /// 获取代表 double 的 jsValue
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static JsValue DoubleValue(double d)
        {
            return MBApi.jsDouble(d);
        }
        /// <summary>
        /// 获取代表 bool 的 jsValue
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static JsValue BoolValue(bool b)
        {
            return MBApi.jsBoolean((byte)(b ? 1 : 0));
        }
        /// <summary>
        /// 获取代表 undefined 的 jsValue
        /// </summary>
        /// <returns></returns>
        public static JsValue UndefinedValue()
        {
            return MBApi.jsUndefined();
        }
        /// <summary>
        /// 获取代表 null 的 jsValue
        /// </summary>
        /// <returns></returns>
        public static JsValue NullValue()
        {
            return MBApi.jsNull();
        }
        /// <summary>
        /// 获取代表 true 的 jsValue
        /// </summary>
        /// <returns></returns>
        public static JsValue TrueValue()
        {
            return MBApi.jsTrue();
        }
        /// <summary>
        /// 获取代表 false 的 jsValue
        /// </summary>
        /// <returns></returns>
        public static JsValue FalseValue()
        {
            return MBApi.jsFalse();
        }
        /// <summary>
        /// 获取代表 string 的 jsValue
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static JsValue StringValue(IntPtr jsExecState, string str)
        {
            return MBApi.jsStringW(jsExecState, str);
        }
        /// <summary>
        /// 获取代表空 object 的 jsValue
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <returns></returns>
        public static JsValue EmptyObjectValue(IntPtr jsExecState)
        {
            return MBApi.jsEmptyObject(jsExecState);
        }
        /// <summary>
        /// 获取代表空 array 的 jsValue
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <returns></returns>
        public static JsValue EmptyArrayValue(IntPtr jsExecState)
        {
            return MBApi.jsEmptyArray(jsExecState);
        }

        public static JsValue ArrayValue(IntPtr jsExecState, StringBuilder buffer)
        {
            return MBApi.jsArrayBuffer(jsExecState, buffer, buffer.Length);
        }

        /// <summary>
        /// 获取全局对象(window)的 JsValue 
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <returns></returns>
        public static JsValue GlobalObjectValue(IntPtr jsExecState)
        {
            return MBApi.jsGlobalObject(jsExecState);
        }

        /// <summary>
        /// 获取代表 object 的 JsValue
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <param name="jsObj">继承此类，Overite 方法</param>
        /// <returns></returns>
        public static JsValue ObjectValue(IntPtr jsExecState, JsObject jsObj)
        {
            if (jsObj == null)
                return MBApi.jsUndefined();
            IntPtr pData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(jsData)));
            if (pData == IntPtr.Zero)
                return MBApi.jsUndefined();
            jsData jd = new jsData();
            jd.typeName = "object";
            jd.propertyGet = jsObj._jsGetProperty;
            jd.propertySet = jsObj._jsSetProperty;
            jd.finalize = jsObj._jsFinalize;
            Marshal.StructureToPtr(jd, pData, false);
            return MBApi.jsObject(jsExecState, pData);
        }
        /// <summary>
        /// 获取代表 function 的 JsValue
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <param name="function">long (IntPtr jsExecState, long obj, jsValue[] args)</param>
        /// <returns></returns>
        public static JsValue FunctionValue(IntPtr jsExecState, jsCallAsFunction function)
        {
            if (function == null)
                return MBApi.jsUndefined();
            IntPtr pData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(jsData)));
            if (pData == IntPtr.Zero)
                return MBApi.jsUndefined();
            jsData jd = new jsData();
            jd.typeName = "function";
            sm_jsCallAsFunctionDictionary[pData] = new jsCallAsFunctionCallback((IntPtr es, Int64 obj, IntPtr args, int argCount) => 
            {
                JsValue[] argsValue = new JsValue[argCount];
                for (int i = 0; i < argCount; i++)
                {
                    argsValue[i] = MBApi.jsArg(es, i);
                }
                return function(es, obj, argsValue);
            });
            jd.callAsFunction = sm_jsCallAsFunctionDictionary[pData];
            jd.finalize = sm_finalize;
            Marshal.StructureToPtr(jd, pData, false);
            return MBApi.jsFunction(jsExecState, pData);
        }


        private static void jsFinalize(IntPtr data)
        {
            Marshal.FreeHGlobal(data);
            if (sm_jsCallAsFunctionDictionary.ContainsKey(data))
                sm_jsCallAsFunctionDictionary.Remove(data);
        }

        /// <summary>
        /// 获取 WebView 句柄
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <returns></returns>
        public static IntPtr GetWebView(IntPtr jsExecState)
        {
            return MBApi.jsGetWebView(jsExecState);
        }
        /// <summary>
        /// 计算表达式
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <param name="script"></param>
        /// <returns></returns>
        public static JsValue Eval(IntPtr jsExecState, string script)
        {
            return MBApi.jsEvalW(jsExecState, script);
        }
        /// <summary>
        /// 计算表达式
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <param name="script"></param>
        /// <param name="isInClosure">是否闭包</param>
        /// <returns></returns>
        public static JsValue Eval(IntPtr jsExecState, string script, bool isInClosure)
        {
            return MBApi.jsEvalExW(jsExecState, script, isInClosure);
        }
        /// <summary>
        /// 调用全局方法
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <param name="function"></param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        public static JsValue CallGlobal(IntPtr jsExecState, JsValue function, params JsValue[] args)
        {
            int count = args.Length;
            long[] longArgs = new long[count];
            for (int i = 0; i < count; i++)
            {
                longArgs[i] = args[i].Value;
            }
            return MBApi.jsCallGlobal(jsExecState, function, longArgs, count);
        }
        /// <summary>
        /// 获取全局属性
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <param name="propName">属性名</param>
        /// <returns></returns>
        public static JsValue GetGlobal(IntPtr jsExecState, string propName)
        {
            return MBApi.jsGetGlobal(jsExecState, propName);
        }
        /// <summary>
        /// 设置全局属性
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <param name="propName">属性名</param>
        /// <param name="value">JsValue</param>
        public static void SetGlobal(IntPtr jsExecState, string propName, JsValue value)
        {
            MBApi.jsSetGlobal(jsExecState, propName, value.Value);
        }
        /// <summary>
        /// 判断一个jsValue是否是有效js值
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <param name="value">JsValue</param>
        /// <returns></returns>
        public static bool IsJsValueValid(IntPtr jsExecState, JsValue value)
        {
            return MBApi.jsIsJsValueValid(jsExecState, value.Value) != 0;
        }
        /// <summary>
        /// 判断一个jsExecState 是否有效
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <returns></returns>
        public static bool IsValidExecState(IntPtr jsExecState)
        {
            return MBApi.jsIsValidExecState(jsExecState) != 0;
        }

        /// <summary>
        /// 当wkeRunJs抛出异常时，可以捕获
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <returns></returns>
        public static jsExceptionInfo GetLastErrorIfException(IntPtr jsExecState)
        {
            jsExceptionInfo info = new jsExceptionInfo();
            IntPtr ptr = MBApi.jsGetLastErrorIfException(jsExecState);
            int sizePtr = Marshal.SizeOf(typeof(IntPtr));
            if (ptr != IntPtr.Zero)
            {
                info.Message = Help.PtrToStringUTF8(Marshal.ReadIntPtr(ptr, 0));
                info.SourceLine = Help.PtrToStringUTF8(Marshal.ReadIntPtr(ptr, sizePtr));
                info.ScriptResourceName = Help.PtrToStringUTF8(Marshal.ReadIntPtr(ptr, sizePtr * 2));
                info.LineNumber = Marshal.ReadInt32(ptr, sizePtr * 3);
                info.StartPosition = Marshal.ReadInt32(ptr, sizePtr * 4);
                info.EndPosition = Marshal.ReadInt32(ptr, sizePtr * 5);
                info.StartColumn = Marshal.ReadInt32(ptr, sizePtr * 6);
                info.EndColoumn = Marshal.ReadInt32(ptr, sizePtr * 7);
                info.CallStackString = Help.PtrToStringUTF8(Marshal.ReadIntPtr(ptr, sizePtr * 8));
            }
            return info;
        }
        /// <summary>
        /// 抛出异常
        /// </summary>
        /// <param name="jsExecState"></param>
        /// <param name="Exception"></param>
        /// <returns></returns>
        public static JsValue ThrowException(IntPtr jsExecState, string Exception)
        {
            return MBApi.jsThrowException(jsExecState, Encoding.UTF8.GetBytes(Exception));
        }

        #endregion


    }

}
