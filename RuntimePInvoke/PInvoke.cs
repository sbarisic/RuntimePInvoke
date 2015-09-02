using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace RuntimePInvoke {
	[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	public sealed class RuntimeDllImportAttribute : Attribute {
		public string EntryPoint {
			get;
			set;
		}

		public CallingConvention CallingConvention {
			get;
			set;
		}

		public CharSet CharSet {
			get;
			set;
		}

		public bool PreserveSig {
			get;
			set;
		}

		public RuntimeDllImportAttribute() {
			this.EntryPoint = null;
			this.CallingConvention = CallingConvention.Winapi;
			this.CharSet = CharSet.Ansi;
			this.PreserveSig = true;
		}
	}

	public static class PInvoke {
		static Type[] GetParamTypes(MethodInfo MInf) {
			List<Type> ParamTypes = new List<Type>();
			ParameterInfo[] Params = MInf.GetParameters();
			for (int j = 0; j < Params.Length; j++)
				ParamTypes.Add(Params[j].ParameterType);
			return ParamTypes.ToArray();
		}

		public static void Load(Type T, string DllName) {
			AppDomain Cur = AppDomain.CurrentDomain;
			AssemblyName AsmName = new AssemblyName("RuntimePInvokeAsm");
			AssemblyBuilder AsmBuilder = Cur.DefineDynamicAssembly(AsmName, AssemblyBuilderAccess.RunAndSave);
			ModuleBuilder ModBuild = AsmBuilder.DefineDynamicModule(AsmName.Name);
			TypeBuilder TBuild = ModBuild.DefineType("RuntimePInvoke_" + T.Name, TypeAttributes.Public);
			MethodAttributes MAttr = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PinvokeImpl;

			RuntimeDllImportAttribute Attr;
			FieldInfo[] Fields = T.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

			for (int i = 0; i < Fields.Length; i++)
				if (typeof(Delegate).IsAssignableFrom(Fields[i].FieldType) &&
					(Attr = Fields[i].GetCustomAttribute<RuntimeDllImportAttribute>()) != null) {
					MethodInfo MInf = Fields[i].FieldType.GetMethod("Invoke");

					string EntryPoint = Attr.EntryPoint != null ? Attr.EntryPoint : Fields[i].Name;
					MethodBuilder MB = TBuild.DefinePInvokeMethod(EntryPoint, DllName, MAttr, CallingConventions.Standard,
					MInf.ReturnType, GetParamTypes(MInf), Attr.CallingConvention, Attr.CharSet);
					if (Attr.PreserveSig)
						MB.SetImplementationFlags(MB.GetMethodImplementationFlags() | MethodImplAttributes.PreserveSig);
				}

			Type RPInv = TBuild.CreateType();
			for (int i = 0; i < Fields.Length; i++)
				if (typeof(Delegate).IsAssignableFrom(Fields[i].FieldType) &&
					(Attr = Fields[i].GetCustomAttribute<RuntimeDllImportAttribute>()) != null) {
					string EntryPoint = Attr.EntryPoint != null ? Attr.EntryPoint : Fields[i].Name;
					Fields[i].SetValue(null, Delegate.CreateDelegate(Fields[i].FieldType,
						RPInv.GetMethod(EntryPoint, GetParamTypes(Fields[i].FieldType.GetMethod("Invoke"))), true));
				}
		}
	}
}