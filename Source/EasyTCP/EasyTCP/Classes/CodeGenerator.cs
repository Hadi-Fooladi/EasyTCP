﻿using System.Collections.Generic;

namespace EasyTCP
{
	using Properties;

	internal class CodeGenerator : Config.EasyTCP
	{
		public CodeGenerator(string Filename) : base(Filename) { }

		private readonly Dictionary<string, string> dicRead = new Dictionary<string, string>
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
			SW.WriteLine("internal class {0}", ClassName);
			SW.Block(() =>
			{
				// Version Field
				SW.WriteLine("public static readonly Version Version = new Version({0}, {1});", Version.Major, Version.Minor);
				SW.WriteLine();

				#region Constructor
				SW.WriteLine("public {0}(TcpClient Client)", ClassName);
				SW.Block(() =>
				{
					SW.WriteLine("this.Client = Client;");
					SW.WriteLine("Client.NoDelay = true;");
					SW.WriteLine();

					SW.WriteLine("MS = new MemoryStream(B);");
					SW.WriteLine("this.BW = new BinaryWriter(MS);");
					SW.WriteLine();

					SW.WriteLine("NS = Client.GetStream();");
					SW.WriteLine("BR = new BinaryReader(NS);");
					SW.WriteLine("var BW = new BinaryWriter(NS);");
					SW.WriteLine();

					// Sending Version
					SW.WriteLine("BW.Write(Version.Major);");
					SW.WriteLine("BW.Write(Version.Minor);");
					SW.WriteLine();

					// Reading Version
					SW.WriteLine("var Major = BR.ReadInt32();");
					SW.WriteLine("BR.ReadInt32(); // Skip Minor");
					SW.WriteLine();

					// Checking Version
					SW.WriteLine("if (Major != Version.Major)");
					SW.Inside(() => SW.WriteLine("throw new Exception(\"Version Mismatch\");"));
					SW.WriteLine();

					// Starting the thread
					SW.WriteLine("T = new Thread(ReceiveData);");
					SW.WriteLine("T.Start();");
				});
				SW.WriteLine();
				#endregion

				#region Fields Declaration
				SW.WriteLine("#region Fields");
				SW.WriteLine("private readonly BinaryReader BR;");
				SW.WriteLine("private readonly TcpClient Client;");
				SW.WriteLine("private readonly NetworkStream NS;");
				SW.WriteLine();

				SW.WriteLine("private readonly MemoryStream MS;");
				SW.WriteLine("private readonly BinaryWriter BW;");
				SW.WriteLine("private readonly byte[] B = new byte[ushort.MaxValue];");
				SW.WriteLine();


				SW.WriteLine("private bool Closing;");
				SW.WriteLine("private readonly Thread T;");
				SW.WriteLine("#endregion");
				SW.WriteLine();
				#endregion

				#region Delegates
				SW.WriteLine("#region Delegates");
				SW.WriteLine("public delegate void dlg({0} Sender);", ClassName);
				foreach (var P in Packet)
					SW.WriteLine("public delegate void dlg{0}({1} Sender, {2});", P.Name, ClassName, P.Signature());
				SW.WriteLine("#endregion");
				SW.WriteLine();
				#endregion

				#region Events
				SW.WriteLine("#region Events");
				SW.WriteLine("public event dlg OnClosed;");
				foreach (var P in Packet)
					SW.WriteLine("public event dlg{0} On{0};", P.Name);
				SW.WriteLine("#endregion");
				SW.WriteLine();
				#endregion

				// Basic Methods
				SW.WriteBulk(Resources.Methods);
				SW.WriteLine();

				#region Send Methods
				foreach (var P in Packet)
				{
					// Desc
					SW.WriteDesc(P.Desc);
					foreach (var D in P.Data)
						SW.WriteParameterDesc(D.Name, D.Desc);

					SW.WriteLine("public void Send{0}({1})", P.Name, P.Signature());
					SW.Block(() =>
					{
						SW.WriteLine("WriteCode({0});", P.Code);

						var BlankLine = true;
						foreach (var D in P.Data)
							if (D.isList)
							{
								if (!BlankLine) SW.WriteLine();

								SW.WriteLine("BW.Write((ushort){0}.Count);", D.Name);
								SW.WriteLine("foreach (var X in {0})", D.Name);
								SW.Inside(() => SW.WriteLine("BW.Write(X);"));

								SW.WriteLine();
								BlankLine = true;
							}
							else
							{
								BlankLine = false;
								SW.WriteLine("BW.Write({0});", D.Name);
							}

						SW.WriteLine("Flush();");
					});
					SW.WriteLine();
				}
				#endregion

				#region ReceiveData
				SW.WriteLine("private void ReceiveData()");
				SW.Block(() =>
				{
					SW.WriteLine("for (;;)");
					SW.Block(() =>
					{
						SW.WriteLine("if (Client.Available <= 0)");
						SW.Block(() =>
						{
							SW.WriteLine("Thread.Sleep(100);");
							SW.WriteLine("continue;");
						});
						SW.WriteLine();

						SW.WriteLine("ushort");
						SW.Inside(() =>
						{
							SW.WriteLine("Code = BR.ReadUInt16(),");
							SW.WriteLine("Len = BR.ReadUInt16();");
						});
						SW.WriteLine();

						SW.WriteLine("switch (Code)");
						SW.WriteLine("{");
						SW.WriteLine("case 0xFFFF:");
						SW.Inside(() =>
						{
							SW.WriteLine("SendCloseRequest();");
							SW.WriteLine();

							SW.WriteLine("Client.Close();");
							SW.WriteLine("OnClosed?.Invoke(this);");
							SW.WriteLine("return; // Exit the thread");
						});

						foreach (var P in Packet)
						{
							SW.WriteLine();
							SW.WriteLine("case {0}: // {1}", P.Code, P.Name);
							SW.Block(() =>
							{
								var Args = "";
								var BlankLine = true;
								foreach (var D in P.Data)
								{
									string
										VarName = "o" + D.Name,
										Deserializer =
											dicRead.ContainsKey(D.Type) ?
												string.Format("BR.{0}();", dicRead[D.Type]) :
												string.Format("new {0}(BR);", D.Type);

									Args += ", " + VarName;

									if (D.isList)
									{
										if (!BlankLine) SW.WriteLine();

										var CountVar = D.Name + "Count";
										SW.WriteLine("var {0} = BR.ReadUInt16();", CountVar);
										SW.WriteLine("var {0} = new {1}[{2}];", VarName, D.Type, CountVar);
										SW.WriteLine("for (ushort i = 0; i < {0}; i++)", CountVar);
										SW.Inside(() => SW.WriteLine("{0}[i] = {1}", VarName, Deserializer));

										SW.WriteLine();
										BlankLine = true;
									}
									else
									{
										BlankLine = false;
										SW.WriteLine("var {0} = {1}", VarName, Deserializer);
									}
								}

								if (!BlankLine) SW.WriteLine();
								SW.WriteLine("On{0}?.Invoke(this{1});", P.Name, Args);
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
		}
	}
}
