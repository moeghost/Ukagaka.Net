/*============================================
 * 类名 :ReflectWord
 * 描述 :用反射来查找可以加载的关键字，包括常量和函数名
 *   
 * 创建时间: 2011-2-6 12:00:12
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;
using System.Reflection;

namespace LittleGrayCalculator.Cores
{
    /// <summary>用反射来查找可以加载的关键字，包括常量和函数名</summary>
    class ReflectWord
    {

        internal static void Find(List<ConstantNode> constants, List<FunctionNode> functions, List<OperateNode> operates) {
            Assembly ass = Assembly.GetExecutingAssembly();
            Module[] modes = ass.GetModules();
            Type[] typs;

            foreach (Module m in modes) {
                typs = m.GetTypes();

                foreach (Type typ in typs) {

                    if (typ.IsSubclassOf(typeof(ConstantNode))) {
                        constants.Add(ass.CreateInstance(typ.FullName) as ConstantNode);
                    } else if (typ.IsSubclassOf(typeof(FunctionNode))) {
                        functions.Add(ass.CreateInstance(typ.FullName) as FunctionNode);
                    } else if (typ.IsSubclassOf(typeof(OperateNode))) {
                        operates.Add(ass.CreateInstance(typ.FullName) as OperateNode);
                    }
                }
            }
        }

        internal static void Find<T>(List<T> nodes)
        {
            Assembly ass = Assembly.GetExecutingAssembly();
            Module[] modes = ass.GetModules();
            Type[] typs;

            foreach (Module m in modes)
            {
                typs = m.GetTypes();

                foreach (Type typ in typs)
                {

                    if (typ.IsSubclassOf(typeof(T)))
                    {
                        nodes.Add((T)ass.CreateInstance(typ.FullName));
                    }
                    
                }
            }
        }
        public static Type GetType <T>(T node)
        {
            Assembly ass = Assembly.GetExecutingAssembly();
            Module[] modes = ass.GetModules();
            Type[] typs;

            foreach (Module m in modes)
            {
                typs = m.GetTypes();

                foreach (Type typ in typs)
                {

                    if (typ.IsSubclassOf(typeof(T)))
                    {
                        return typ;
                    }

                }
            }
            return null;
        }


    } // end class
}
