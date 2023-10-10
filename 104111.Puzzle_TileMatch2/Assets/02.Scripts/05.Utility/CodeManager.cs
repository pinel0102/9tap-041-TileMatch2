/*
 * Code Manager
 * 
 * Author : Pinelia Luna
 * Update : 2015.06.26
 * Version : 1.0.0
 * Contact : pinel0102@gmail.com
 * 
 * Summary:
 * - CodeManager is a C#-based class which helps the code working process.
 * 
 * How to use: 
 * - Import the CodeManager.cs file into your project.
 * - Type CodeManager.xxxx in your C# scripts anywhere you want.
 * - This class might be useful with any log methods, such as Debug.Log in Unity.
 * 
 * Methods:
 * - public string GetDeclaringType()	// Returns the declaring type, usually the class name.
 * - public string GetClassName()		// Returns the current class name.
 * - public string GetMethodName()		// Returns the current method name.
 * - public string GetMethodCall()		// Returns the method call orders recursively.
 * - public string GetFunctionName()	// Returns the current method name.
 * - public string GetFunctionCall()	// Returns the method call orders recursively.
 * 
*/

using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Reflection;
using System.Text;

/// <summary>
/// The CodeManager is a C#-based class which helps the code working process.
/// </summary>
public static class CodeManager
{
    //static bool showCurrentTime = true;

    const string strGetMethodName = "GetMethodName";
    const string strGetFunctionName = "GetFunctionName";
    const string strGetCoroutineName = "GetCoroutineName";
    const string strGetFunctionCall = "GetFunctionCall";

    /// <summary>
    /// Returns the declaring type, usually the class name.
    /// </summary>
    /// <param name="showNamespace">If namespace exsists, includes it.</param>	
    /// <param name="index">Defines the class level. (current: 0, +1 per previous level).</param>	
    /// <returns>The declaring type, usually the class name.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string GetDeclaringType(bool showCurrentTime = false, bool showNamespace = false, int index = 0)
    {
        StringBuilder sb = new StringBuilder();

        var st = new StackTrace(new StackFrame(index + 1));
                
        if (st.FrameCount > 0)
        {
            try
            {
                sb.Append("[");

                if (showNamespace)
                    sb.Append(st.GetFrame(0).GetMethod().DeclaringType.ToString());
                else
                    sb.Append(st.GetFrame(0).GetMethod().DeclaringType.Name);

                
                var stt = new StackTrace(new StackFrame(index));

                if (stt.GetFrame(0).GetMethod().Name.Equals(strGetMethodName) || 
                    stt.GetFrame(0).GetMethod().Name.Equals(strGetFunctionName) || 
                    stt.GetFrame(0).GetMethod().Name.Equals(strGetCoroutineName))

                    sb.Append(".");
                else
                    sb.Append("] ");		
                
            }
            catch
            {
                sb.Remove(0, sb.Length);
                sb.Append(GetMethodName() + "Failed. (The index is unvaliable.)");
            }
        }
        else
        {
            sb.Append("[Warning] StackTrace.FrameCount = ");
            sb.Append(st.FrameCount);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns the current class name.
    /// </summary>	
    /// <param name="index">Defines the class level. (current: 0, +1 per previous level).</param>	
    /// <returns>The class name.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string GetClassName(bool showCurrentTime = false, bool showNamespace = false, int index = 0)
    {
        return GetDeclaringType(showCurrentTime, showNamespace, index + 1);
    }


    /// <summary>
    /// Returns the current method name.
    /// </summary>	
    /// <param name="showClassName">Includes the class name.</param>
    /// <param name="showParams">Includes the declaring type and params.</param>	
    /// <param name="index">Defines the method level. (current: 0, +1 per previous level).</param>
    /// <returns>The method name.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string GetMethodName(bool showCurrentTime = false, bool showNamespace = false, bool showClassName = true, bool showParams = false, int index = 0)
    {
        StringBuilder sb = new StringBuilder();

        var st = new StackTrace(new StackFrame(index + 1));

        if (st.FrameCount > 0)
        {
            try
            {
                if (showCurrentTime)
                {
                    sb.Append(System.DateTime.Now.ToString("<tt hh:mm:ss.fff> "));
                }

                if (showClassName)
                {					
                    sb.Append(GetDeclaringType(false, showNamespace, index + 1));
                }
                else
                {
                    sb.Append("[");
                }
                
                if (showParams)
                    sb.Append(st.GetFrame(0).GetMethod().ToString());
                else
                    sb.Append(st.GetFrame(0).GetMethod().Name);
                
                sb.Append("] ");
            }
            catch
            {
                sb.Remove(0, sb.Length);
                sb.Append(GetClassName() + "[" + MethodBase.GetCurrentMethod().Name + "] Failed. (The index is unvaliable.)");
            }
        }
        else
        {
            sb.Append("[Warning] StackTrace.FrameCount = ");
            sb.Append(st.FrameCount);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns the current method name.
    /// </summary>	
    /// <param name="showClassName">Includes the class name.</param>
    /// <param name="showParams">Includes the declaring type and params.</param>	
    /// <param name="index">Defines the method level. (current: 0, +1 per previous level).</param>
    /// <returns>The method name.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string GetFunctionName(bool showCurrentTime = false, bool showNamespace = false, bool showClassName = true, bool showParams = false, int index = 0)
    {
        return GetMethodName(showCurrentTime, showNamespace, showClassName, showParams, index + 1);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="showCurrentTime"></param>
    /// <param name="showNamespace"></param>
    /// <param name="showClassName"></param>
    /// <param name="showParams"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string GetCoroutineName(bool showCurrentTime = false, bool showNamespace = true, bool showClassName = true, bool showParams = false, int index = 0)
    {
        return GetMethodName(showCurrentTime, showNamespace, showClassName, showParams, index + 1);
    }


    /// <summary>
    /// Returns the method call orders recursively.
    /// </summary>
    /// <param name="showClassName">Includes the class name.</param>
    /// <param name="showParams">Includes the declaring type and params.</param>	
    /// <param name="fromThisMethod">Starts with the current method.</param>
    /// <returns>The method call orders.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string GetMethodCall(bool showClassName = false, bool showParams = false, bool fromThisMethod = false)
    {
        StringBuilder sb = new StringBuilder();

        var st = new StackTrace(1, true);

        if (st.FrameCount > 0)
        {
            try
            {
                if (fromThisMethod)
                {
                    for (int i = 0; i < st.FrameCount; i++)
                    {
                        MethodBase mb = st.GetFrame(i).GetMethod();

                        if (mb.Name.Equals(strGetFunctionCall))
                            continue;

                        sb.Append("[");

                        if (showClassName)
                        {
                            sb.Append(mb.DeclaringType.Name);
                            sb.Append(".");
                        }

                        if (showParams)
                            sb.Append(mb.ToString());
                        else
                            sb.Append(mb.Name);
                        
                        sb.Append("]");

                        if (i < st.FrameCount - 1)
                            sb.Append(" ← ");
                    }
                }
                else
                {
                    for (int i = st.FrameCount - 1; i > -1; i--)
                    {
                        MethodBase mb = st.GetFrame(i).GetMethod();

                        if (mb.Name.Equals(strGetFunctionCall))
                            continue;
                        
                        if (i < st.FrameCount - 1)
                            sb.Append(" → ");

                        sb.Append("[");

                        if (showClassName)
                        {
                            sb.Append(mb.DeclaringType.Name);
                            sb.Append(".");
                        }

                        if (showParams)
                            sb.Append(mb.ToString());
                        else
                            sb.Append(mb.Name);

                        sb.Append("]");
                    }
                }
            }
            catch
            {
                sb.Remove(0, sb.Length);
                sb.Append(GetMethodName() + "Failed.");
            }
        }
        else
        {
            sb.Append("[Warning] StackTrace.FrameCount = ");
            sb.Append(st.FrameCount);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns the method call orders recursively.
    /// </summary>
    /// <param name="showClassName">Includes the class name.</param>
    /// <param name="showParams">Includes the declaring type and params.</param>	
    /// <param name="fromThisMethod">Starts with the current method.</param>
    /// <returns>The method call orders.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static string GetFunctionCall(bool showClassName = false, bool showParams = false, bool fromThisFunction = false)
    {
        return GetMethodCall(showClassName, showParams, fromThisFunction);
    }
}
