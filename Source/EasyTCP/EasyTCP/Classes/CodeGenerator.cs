﻿namespace EasyTCP
{
	internal class CodeGenerator : Config.EasyTCP
	{
		public CodeGenerator(string Filename, bool UseNullPropagation) : base(Filename)
		{
			this.UseNullPropagation = UseNullPropagation;
		}

		private readonly bool UseNullPropagation;

		public void Generate()
		{
			var SW = Global.SW;

			// Writing Stream Class
			SW.WriteLine("internal class {0} : BaseStream", Stream.ClassName);
			SW.Block(() =>
			{
				// Version Field
				SW.WriteLine("private static readonly Version VERSION = new Version(\"{0}\");", Stream.Version);
				SW.WriteLine();

				#region Constructor
				SW.WriteLine($"public {Stream.ClassName}(int WriteBufferLength = 65536) : base(WriteBufferLength)");
				SW.Block(() =>
				{
					SW.WriteLine("T = new Thread(ReceiveData) { IsBackground = true };");
				});
				#endregion

				SW.Region("Overridden", () =>
				{
					SW.WriteLine("protected override Thread T { get; }");
					SW.WriteLine("protected override Version Version { get { return VERSION; } }");
				});

				#region Delegates
				SW.Region("Delegates", () =>
				{
					foreach (var P in Stream.Packet)
					{
						string S = P.EventSignature();
						if (S != "") S = ", " + S;

						SW.WriteLine("public delegate void dlg{0}({1} Sender{2});", P.Name, Stream.ClassName, S);
					}
				});
				#endregion

				#region Events
				SW.Region("Events", () =>
				{
					foreach (var P in Stream.Packet)
						SW.WriteLine("public event dlg{0} On{0};", P.Name);

					if (UseNullPropagation) return;

					SW.WriteLine();
					foreach (var P in Stream.Packet)
						AddFireMethod(P);
				});
				#endregion

				#region Send Methods
				SW.Region("Send Methods", () =>
				{
					foreach (var P in Stream.Packet)
					{
						P.WriteDesc();
						foreach (var D in P.Data)
							SW.WriteParameterDesc(D.Name, D.Desc);

						SW.WriteLine("public void Send{0}({1})", P.Name, P.SendSignature());
						SW.Block(() =>
						{
							SW.WriteLine("if (Closing) return;");
							SW.WriteLine();

							SW.WriteLine("lock (WriteLock)");
							SW.Block(() =>
							{
								SW.WriteLine("WriteCode({0});", P.Code);

								foreach (var D in P.Data)
									D.WriteWrite();

								SW.WriteLine("Flush();");
							});
						});
						SW.WriteLine();
					}
				});
				#endregion

				#region ReceiveData
				SW.WriteLine("private void ReceiveData()");
				SW.Block(() =>
				{
					SW.TryCatch(Loop, ()=>SW.WriteLine("fireException(E);"));

					void Loop()
					{
					SW.WriteLine("for (;;)");
					SW.Block(() =>
					{
						SW.WriteLine("ushort Code = BR.ReadUInt16();");
						SW.WriteLine("int Len = BR.ReadInt32();");
						SW.WriteLine();

						SW.WriteLine("switch (Code)");
						SW.WriteLine("{");
						SW.WriteLine("case 0xFFFF:");
						SW.Inside(() =>
						{
							SW.WriteLine("SendCloseRequest();");
							SW.WriteLine();

							SW.WriteLine("Client.Close();");
							SW.WriteLine("fireClosed();");
							SW.WriteLine("return; // Exit the thread");
						});

						foreach (var P in Stream.Packet)
						{
							SW.WriteLine();
							SW.WriteLine($"case {P.Code}: // {P.Name}");
							SW.Block(() =>
							{
								foreach (var D in P.Data)
									SW.WriteLine($"{D.Type}{(D.isList ? "[]" : "")} o{D.Name};");

								SW.WriteLine();

								foreach (var D in P.Data)
									D.WriteRead("o" + D.Name);

								SW.WriteLine();

								// Firing the event
								string Args = P.Arguments("o");
								if (UseNullPropagation && !string.IsNullOrEmpty(Args))
									Args = ", " + Args;

								SW.WriteLine(UseNullPropagation ? "On{0}?.Invoke(this{1});" : "fire{0}({1});", P.Name, Args);

								SW.WriteLine("break;");
							});
						}

						SW.WriteLine();
						SW.WriteLine("default:");
						SW.Inside(() =>
						{
							SW.WriteLine("BR.ReadBytes(Len); // Skip Packet");
							SW.WriteLine("break;");
						});

						SW.WriteLine("}");
					});
					}
				});
				#endregion
			});

			// Declaring Enums & DataTypes
			foreach (var Enum in Enums) Enum.Declare();
			foreach (var DT in DataTypes) DT.Declare();

			if (DataTypes.Count <= 0 && Enums.Count <= 0) return;

			// Writing Extension Methods
			SW.WriteLine("internal static partial class Ext");
			SW.Block(() =>
			{
				foreach (var Enum in Enums)
				{
					SW.WriteLine($"// {Enum.Name}");
					SW.WriteLine($"public static void Write(this BinaryWriter BW, {Enum.Name} Value) {{ BW.Write((Enum)Value); }}");
					SW.WriteLine($"public static void Read(this BinaryReader BR, out {Enum.Name} Value) {{ Value = ({Enum.Name})BR.ReadInt32(); }}");
					SW.WriteLine();
				}

				foreach (var DT in DataTypes)
				{
					SW.WriteLine($"// {DT.Name}");
					
					// Write
					SW.WriteLine($"public static void Write(this BinaryWriter BW, {DT.Name} Obj)");
					SW.Block(() =>
					{
						foreach (var F in DT.Fields)
							F.WriteWrite($"Obj.{F.Name}");
					});

					// Read
					SW.WriteLine($"public static void Read(this BinaryReader BR, out {DT.Name} Obj)");
					SW.Block(() =>
					{
						SW.WriteLine($"Obj = new {DT.Name}();");
						foreach (var F in DT.Fields)
							F.WriteRead($"Obj.{F.Name}");
					});
				}
			});
		}

		private static void AddFireMethod(string Name, string Signature, string Arguments)
			=> Global.SW.WriteLine(
				"private void fire{0}({1}) {{ var _ = On{0}; if (_ != null) _(this{3}{2}); }}",
				Name, Signature, Arguments, string.IsNullOrEmpty(Arguments) ? "" : ", ");

		private static void AddFireMethod(StreamData.PacketData P) => AddFireMethod(P.Name, P.EventSignature(), P.Arguments());
	}
}
