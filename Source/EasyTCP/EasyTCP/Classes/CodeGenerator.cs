using System.Collections.Generic;

namespace EasyTCP
{
	using Properties;

	internal class CodeGenerator : Config.EasyTCP
	{
		public CodeGenerator(string Filename, bool UseNullPropagation) : base(Filename)
		{
			this.UseNullPropagation = UseNullPropagation;
		}

		private readonly bool UseNullPropagation;

		private static readonly Dictionary<string, string> dicRead = new Dictionary<string, string>
		{
			["bool"] = "ReadBoolean",
			["char"] = "ReadChar",
			["byte"] = "ReadByte",
			["sbyte"] = "ReadSByte",
			["short"] = "ReadInt16",
			["ushort"] = "ReadUInt16",
			["int"] = "ReadInt32",
			["uint"] = "ReadUInt32",
			["long"] = "ReadInt64",
			["ulong"] = "ReadUInt64",
			["string"] = "ReadString",
			["float"] = "ReadSingle",
			["double"] = "ReadDouble",
			["decimal"] = "ReadDecimal"
		};

		public void Generate(IndentedStreamWriter SW)
		{
			// Writing Stream Class
			SW.WriteLine("internal class {0}", Stream.ClassName);
			SW.Block(() =>
			{
				// Version Field
				SW.WriteLine("public static readonly Version Version = new Version(\"{0}\");", Stream.Version);
				SW.WriteLine();

				#region Constructor
				SW.WriteLine($"public {Stream.ClassName}(int WriteBufferLength = 65536)");
				SW.Block(() =>
				{
					SW.WriteLine("B = new byte[WriteBufferLength];");
					SW.WriteLine("MS = new MemoryStream(B);");
					SW.WriteLine("BW = new BinaryWriter(MS, Encoding.Unicode);");
					SW.WriteLine();
					SW.WriteLine("T = new Thread(ReceiveData);");
				});
				SW.WriteLine();
				#endregion

				#region Fields Declaration
				SW.WriteLine("#region Fields");
				SW.WriteDesc("Amount of sleep time if no data exist (in millisecond)");
				SW.WriteLine("public int Sleep4Data = 100;");
				SW.WriteLine();

				SW.WriteLine("private BinaryReader BR;");
				SW.WriteLine("private TcpClient Client;");
				SW.WriteLine("private NetworkStream NS;");
				SW.WriteLine();

				SW.WriteLine("private readonly byte[] B;");
				SW.WriteLine("private readonly MemoryStream MS;");
				SW.WriteLine("private readonly BinaryWriter BW;");
				SW.WriteLine();

				SW.WriteLine("private readonly Thread T;");
				SW.WriteLine("private bool Closing, Running = true;");
				SW.WriteLine("#endregion");
				SW.WriteLine();
				#endregion

				#region Delegates
				SW.WriteLine("#region Delegates");
				SW.WriteLine("public delegate void dlg({0} Sender);", Stream.ClassName);
				foreach (var P in Stream.Packet)
				{
					string S = P.EventSignature();
					if (S != "") S = ", " + S;

					SW.WriteLine("public delegate void dlg{0}({1} Sender{2});", P.Name, Stream.ClassName, S);
				}

				SW.WriteLine("#endregion");
				SW.WriteLine();
				#endregion

				#region Events
				SW.WriteLine("#region Events");

				// Events
				SW.WriteLine("public event dlg OnClosed;");
				foreach (var P in Stream.Packet)
					SW.WriteLine("public event dlg{0} On{0};", P.Name);

				if (!UseNullPropagation)
				{
					SW.WriteLine();
					
					// Firing Methods
					AddFireMethod(SW, "Closed", "", "");
					foreach (var P in Stream.Packet)
						AddFireMethod(SW, P);
				}

				SW.WriteLine("#endregion");
				SW.WriteLine();
				#endregion

				// Basic Methods
				SW.WriteBulk(Resources.Methods);
				SW.WriteLine();

				#region Send Methods
				foreach (var P in Stream.Packet)
				{
					// Desc
					SW.WriteDesc(P.Desc);
					foreach (var D in P.Data)
						SW.WriteParameterDesc(D.Name, D.Desc);

					SW.WriteLine("public void Send{0}({1})", P.Name, P.SendSignature());
					SW.Block(() =>
					{
						SW.WriteLine("if (Closing) return;");
						SW.WriteLine();
						SW.WriteLine("WriteCode({0});", P.Code);

						foreach (var D in P.Data)
							if (D.isList)
							{
								SW.WriteLine();
								SW.WriteLine("BW.Write((ushort){0}.Count);", D.Name);
								SW.WriteLine("foreach (var X in {0})", D.Name);
								SW.Inside(() => SW.WriteLine("BW.Write(X);"));
								SW.WriteLine();
							}
							else
								SW.WriteLine("BW.Write({0});", D.Name);

						SW.WriteLine("Flush();");
					});
					SW.WriteLine();
				}
				#endregion

				#region ReceiveData
				SW.WriteLine("private void ReceiveData()");
				SW.Block(() =>
				{
					SW.WriteLine("while (Running)");
					SW.Block(() =>
					{
						SW.WriteLine("if (Client.Available <= 0)");
						SW.Block(() =>
						{
							SW.WriteLine("Thread.Sleep(Sleep4Data);");
							SW.WriteLine("continue;");
						});
						SW.WriteLine();

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
							SW.WriteLine(UseNullPropagation ? "OnClosed?.Invoke(this);" : "fireClosed();");
							SW.WriteLine("return; // Exit the thread");
						});

						foreach (var P in Stream.Packet)
						{
							SW.WriteLine();
							SW.WriteLine("case {0}: // {1}", P.Code, P.Name);
							SW.Block(() =>
							{
								var Args = "";
								foreach (var D in P.Data)
								{
									string
										VarName = "o" + D.Name,
										Deserializer =
											dicRead.ContainsKey(D.Type) ?
												string.Format("BR.{0}();", dicRead[D.Type]) :
												string.Format("new {0}(BR);", D.Type);

									if (UseNullPropagation || Args.Length > 0)
										Args += ", ";

									Args += VarName;

									if (D.isList)
									{
										SW.WriteLine();
										var CountVar = D.Name + "Count";
										SW.WriteLine("var {0} = BR.ReadUInt16();", CountVar);
										SW.WriteLine("var {0} = new {1}[{2}];", VarName, D.Type, CountVar);
										SW.WriteLine("for (ushort i = 0; i < {0}; i++)", CountVar);
										SW.Inside(() => SW.WriteLine("{0}[i] = {1}", VarName, Deserializer));
										SW.WriteLine();
									}
									else
										SW.WriteLine("var {0} = {1}", VarName, Deserializer);
								}

								SW.WriteLine();
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
				});
				#endregion
			});

			// Writing DataTypes
			foreach (var DT in DataTypes)
			{
				SW.WriteLine($"internal{(DT.Partial ? " partial" : "")} {(DT.isClass ? "class" : "struct")} {DT.Name}");
				SW.Block(() =>
				{
					string FieldType(DataType.Field F) => F.isList ? $"IList<{F.Type}>" : F.Type;

					// Declaring Fields
					foreach (var Field in DT.Fields)
						SW.WriteLine($"public {FieldType(Field)} {Field.Name};");

					SW.WriteLine();
					SW.WriteLine($"public {DT.Name}(BinaryReader BR)");
					SW.Block(() =>
					{
						foreach (var Field in DT.Fields)
						{
							var Deserializer = dicRead.ContainsKey(Field.Type)
								? string.Format($"BR.{dicRead[Field.Type]}();")
								: string.Format($"new {Field.Type}(BR);");

							if (Field.isList)
							{
								SW.WriteLine();
								var CountVar = Field.Name + "Count";
								SW.WriteLine($"var {CountVar} = BR.ReadUInt16();");
								SW.WriteLine($"{Field.Name} = new {Field.Type}[{CountVar}];");
								SW.WriteLine($"for (ushort i = 0; i < {CountVar}; i++)");
								SW.Inside(() => SW.WriteLine($"{Field.Name}[i] = {Deserializer}"));
								SW.WriteLine();
							}
							else
								SW.WriteLine($"{Field.Name} = {Deserializer}");
						}
					});
				});
			}

			if (DataTypes.Count <= 0) return;

			// Writing Extension Methods
			SW.WriteLine("internal static class BinaryWriterExt");
			SW.Block(() =>
			{
				foreach (var DT in DataTypes)
				{
					SW.WriteLine($"public static void Write(this BinaryWriter BW, {DT.Name} Obj)");
					SW.Block(() =>
					{
						foreach (var Field in DT.Fields)
						{
							var Name = $"Obj.{Field.Name}";
							if (Field.isList)
							{
								SW.WriteLine();
								SW.WriteLine($"BW.Write((ushort){Name}.Count);");
								SW.WriteLine($"foreach (var X in {Name})");
								SW.Inside(() => SW.WriteLine("BW.Write(X);"));
								SW.WriteLine();
							}
							else
								SW.WriteLine($"BW.Write({Name});");
						}
					});
				}
			});

		}

		private static void AddFireMethod(IndentedStreamWriter SW, string Name, string Signature, string Arguments)
			=> SW.WriteLine(
				"protected void fire{0}({1}) {{ if (On{0} != null) On{0}(this{3}{2}); }}",
				Name, Signature, Arguments, string.IsNullOrEmpty(Arguments) ? "" : ", ");

		private static void AddFireMethod(IndentedStreamWriter SW, StreamData.PacketData P)
			=> AddFireMethod(SW, P.Name, P.EventSignature(), P.Arguments());

	}
}
