using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.Data;

namespace EmitTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string name1 = "EmitTest.DynamicFibonacci";
            string name2 = "EmitTest.EntityBuilder";

            #region Step 1 构建程序集
            //创建程序集名
            AssemblyName asmName = new AssemblyName(name2);

            //获取程序集所在的应用程序域
            //你也可以选择用AppDomain.CreateDomain方法创建一个新的应用程序域
            //这里选择当前的应用程序域
            AppDomain domain = AppDomain.CurrentDomain;

            //实例化一个AssemblyBuilder对象来实现动态程序集的构建
            AssemblyBuilder assemblyBuilder = domain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
            #endregion
 

            #region Step 2 定义模块
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(asmName.Name, asmName.Name + ".dll");

            #endregion


            #region Step 3 定义类型
            TypeBuilder typeBuilder = moduleBuilder.DefineType(name2, TypeAttributes.Public);
            #endregion

            #region Step 4 定义方法

            /*
            public class Fibonacci
             {
                 public int Calc(int num)
                 {
                     if (num == 1 || num == 2)
                     {
                         return 1;
                     }
                     else
                     {
                         return Calc(num - 1) + Calc(num - 2);
                     }
                 }
             }
 

            MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                 "Calc",
                 MethodAttributes.Public,
                 typeof(Int32),
                 new Type[] { typeof(Int32) });

            #endregion

            #region Step 5 实现方法

            ILGenerator calcIL = methodBuilder.GetILGenerator();

            //定义标签lbReturn1，用来设置返回值为1
            Label lbReturn1 = calcIL.DefineLabel();
            //定义标签lbReturnResutl，用来返回最终结果
            Label lbReturnResutl = calcIL.DefineLabel();

            //加载参数1，和整数1，相比较，如果相等则设置返回值为1
            calcIL.Emit(OpCodes.Ldarg_1);
            calcIL.Emit(OpCodes.Ldc_I4_1);
            calcIL.Emit(OpCodes.Beq_S, lbReturn1);

            //加载参数1，和整数2，相比较，如果相等则设置返回值为1
            calcIL.Emit(OpCodes.Ldarg_1);
            calcIL.Emit(OpCodes.Ldc_I4_2);
            calcIL.Emit(OpCodes.Beq_S, lbReturn1);

            //加载参数0和1，将参数1减去1，递归调用自身
            calcIL.Emit(OpCodes.Ldarg_0);
            calcIL.Emit(OpCodes.Ldarg_1);
            calcIL.Emit(OpCodes.Ldc_I4_1);
            calcIL.Emit(OpCodes.Sub);
            calcIL.Emit(OpCodes.Call, methodBuilder);

            //加载参数0和1，将参数1减去2，递归调用自身
            calcIL.Emit(OpCodes.Ldarg_0);
            calcIL.Emit(OpCodes.Ldarg_1);
            calcIL.Emit(OpCodes.Ldc_I4_2);
            calcIL.Emit(OpCodes.Sub);
            calcIL.Emit(OpCodes.Call, methodBuilder);

            //将递归调用的结果相加，并返回
            calcIL.Emit(OpCodes.Add);
            calcIL.Emit(OpCodes.Br, lbReturnResutl);

            //在这里创建标签lbReturn1
            calcIL.MarkLabel(lbReturn1);
            calcIL.Emit(OpCodes.Ldc_I4_1);

            //在这里创建标签lbReturnResutl
            calcIL.MarkLabel(lbReturnResutl);
            calcIL.Emit(OpCodes.Ret);

             * 
             * 
             */
            #endregion


            MethodBuilder methodBuilder = typeBuilder.DefineMethod("DynamicCreateEntity", MethodAttributes.Public);

            var gpas = methodBuilder.DefineGenericParameters("T");
            //设置T的约束s
            gpas[0].SetGenericParameterAttributes(GenericParameterAttributes.ReferenceTypeConstraint | GenericParameterAttributes.DefaultConstructorConstraint);

            //定义参数类型：(IDataRecord dr)
            methodBuilder.SetParameters(typeof(System.Data.IDataRecord));

            //定义返回值类型：T
            methodBuilder.SetReturnType(gpas[0]);

            MethodInfo Activator_CreateInstance = typeof(Activator).GetMethod("CreateInstance", Type.EmptyTypes).MakeGenericMethod(gpas[0]);
            MethodInfo Type_GetProperty = typeof(Type).GetMethod("GetProperty", new Type[] { typeof(string) });
            MethodInfo Object_GetType = typeof(object).GetMethod("GetType");

            MethodInfo PropertyInfo_Get_PropertyType = typeof(PropertyInfo).GetMethod("get_PropertyType");
            MethodInfo PropertyInfo_SetValue = typeof(PropertyInfo).GetMethod("SetValue", new Type[]{ typeof(object), typeof(object), typeof(object[])});

            MethodInfo IDataRecord_GetOrdinal = typeof(System.Data.IDataRecord).GetMethod("GetOrdinal", new Type[] { typeof(string) });
            MethodInfo IDataRecord_IsDBNull = typeof(System.Data.IDataRecord).GetMethod("IsDBNull", new Type[] { typeof(int) });
            MethodInfo IDataRecord_GetValue = typeof(System.Data.IDataRecord).GetMethod("get_Item", new Type[] { typeof(int) });
            MethodInfo IDataRecord_Get_FieldCount = typeof(System.Data.IDataRecord).GetMethod("get_FieldCount");
            MethodInfo IDataRecord_GetName = typeof(System.Data.IDataRecord).GetMethod("GetName", new Type[] { typeof(int) });


            ILGenerator ilgen = methodBuilder.GetILGenerator();
            LocalBuilder result = ilgen.DeclareLocal(gpas[0]);
            //ilgen.Emit(OpCodes.Newobj, gpas[0].GetConstructor(Type.EmptyTypes));
            ilgen.Emit(OpCodes.Call, Activator_CreateInstance);
            ilgen.Emit(OpCodes.Stloc, result);

            //循环中使用的局部变量 　　　 
            LocalBuilder i = ilgen.DeclareLocal(typeof(Int32));
            Label compareLabel = ilgen.DefineLabel(); 　　　 
            Label enterLoopLabel = ilgen.DefineLabel();

            LocalBuilder propertyInfo = ilgen.DeclareLocal(typeof(PropertyInfo));

            
                 //int i = 0 　　　 
                ilgen.Emit(OpCodes.Ldc_I4_0); 　　　 
                ilgen.Emit(OpCodes.Stloc_1); 　　　 
                ilgen.Emit(OpCodes.Br, compareLabel);

                //定义一个标签，表示从下面开始进入循环体 　　　 
                ilgen.MarkLabel(enterLoopLabel);


                ilgen.Emit(OpCodes.Ldloc, result);
                ilgen.Emit(OpCodes.Callvirt, Object_GetType);
                ilgen.Emit(OpCodes.Ldarg_1);
                ilgen.Emit(OpCodes.Ldloc, i);
                ilgen.Emit(OpCodes.Callvirt, IDataRecord_GetName);
                ilgen.Emit(OpCodes.Callvirt, Type_GetProperty);
                ilgen.Emit(OpCodes.Stloc, propertyInfo);

                Label endIfLabel = ilgen.DefineLabel();
                ilgen.Emit(OpCodes.Ldarg_1);
                ilgen.Emit(OpCodes.Ldloc, i);
                ilgen.Emit(OpCodes.Callvirt, IDataRecord_IsDBNull);
                ilgen.Emit(OpCodes.Brtrue, endIfLabel);
                ilgen.Emit(OpCodes.Ldloc, propertyInfo);
                ilgen.Emit(OpCodes.Ldloc, result);
                ilgen.Emit(OpCodes.Ldarg_1);
                ilgen.Emit(OpCodes.Ldloc, i);
                ilgen.Emit(OpCodes.Callvirt, IDataRecord_GetValue);
                //ilgen.Emit(OpCodes.Callvirt, PropertyInfo_Get_PropertyType);
                //ilgen.Emit(OpCodes.Unbox_Any);
                ilgen.Emit(OpCodes.Ldnull);
                ilgen.Emit(OpCodes.Callvirt, PropertyInfo_SetValue);
                ilgen.MarkLabel(endIfLabel);
                

                //i++ 　　　 
                ilgen.Emit(OpCodes.Ldloc_S, i); 　　　 
                ilgen.Emit(OpCodes.Ldc_I4_1); 　　　 
                ilgen.Emit(OpCodes.Add); 　　　 
                ilgen.Emit(OpCodes.Stloc_S, i);  　　　 

                //定义一个标签，表示从下面开始进入循环的比较 　　　 
                ilgen.MarkLabel(compareLabel);

                //i < ints.Length
                ilgen.Emit(OpCodes.Ldloc_S, i);  　　　 
                ilgen.Emit(OpCodes.Ldarg_1); 　　　 
                ilgen.Emit(OpCodes.Callvirt, IDataRecord_Get_FieldCount);　　　 
                ilgen.Emit(OpCodes.Conv_I4); 　　　 
                ilgen.Emit(OpCodes.Clt); 　　　 
                ilgen.Emit(OpCodes.Brtrue_S, enterLoopLabel);


            
            ilgen.Emit(OpCodes.Ldloc, result);
            ilgen.Emit(OpCodes.Ret);

            #region Step 6 收获

            Type type = typeBuilder.CreateType();

            assemblyBuilder.Save(asmName.Name + ".dll");

            DataTable dt = new DataTable("user");
            dt.Columns.Add("UserID", typeof(int));
            dt.Columns.Add("UserName", typeof(string));
            dt.Rows.Add(new object[] { 1, "fuck" });
            dt.Rows.Add(new object[] { 2, "you" });

            User user = new Program().DynamicCreateEntity<User>(dt.CreateDataReader());


            //object obj = Activator.CreateInstance(type);
            //MethodInfo method = type.GetMethod("DynamicCreateEntity", new Type[]{ typeof(IDataRecord)});
            //method = method.MakeGenericMethod(typeof(User));
            //User user = method.Invoke(obj, new object[] { dt.CreateDataReader() }) as User;
            Console.WriteLine(user.UserID);
            Console.WriteLine(user.UserName);
            Console.ReadLine();

            //object ob = Activator.CreateInstance(type);

            //for (int i = 1; i < 10; i++)
            //{
            //    Console.WriteLine(type.GetMethod("Calc").Invoke(ob, new object[] { i }));
            //}
            //Console.ReadLine();
            #endregion
 
        }

        public T DynamicCreateEntity<T>(IDataRecord record1) where T : class, new()
        {
            T local = Activator.CreateInstance<T>();
            for (int i = 0; i < record1.FieldCount; i++)
            {
                PropertyInfo property = local.GetType().GetProperty(record1.GetName(i));
                if (!record1.IsDBNull(i))
                {
                    property.SetValue(local, record1[i], null);
                }
            }
            return local;
        }
    }
}
