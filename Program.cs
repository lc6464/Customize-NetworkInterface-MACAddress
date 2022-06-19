// See https://aka.ms/new-console-template for more information
// Console.WriteLine("Hello, World!");

using var currentIdentity = WindowsIdentity.GetCurrent();
WindowsPrincipal principal = new(currentIdentity);
bool IsAdministrator = principal.IsInRole(WindowsBuiltInRole.Administrator);

FileInfo configFileInfo = new(Statics.configFile);
if (configFileInfo.Exists) { // 判断配置文件是否存在
	if (configFileInfo.Length != 4) {
		string moveTarget = $"{configFileInfo.FullName}_{DateTime.Now:s}.bak";
		Console.WriteLine("配置文件大小有误！\r\n已将原配置文件移动到 " + moveTarget);
		try {
			File.Move(configFileInfo.FullName, moveTarget, true);
		} catch (Exception e) {
			Console.WriteLine("很抱歉，移动配置文件失败！");
			Console.WriteLine();
			Console.WriteLine($"异常：{ e.Message }\r\n详细信息：");
			Console.WriteLine(e);
		}
	} else {
		try {
			string id = File.ReadAllText(configFileInfo.FullName, Statics.utf8);
			if (Statics.idRegex.IsMatch(id)) {
				try {
					using RegistryKey? driver = Statics.hklm.OpenSubKey(@$"{Statics.regPath}\{id}", true);
					if (driver == null) {
						Console.WriteLine("您的注册表存在问题或配置文件有误！\r\n"
							+ $@"无法找到注册表项 {Statics.regFullPath}\{id}");
					} else {
						try {
							string driverName = driver.GetValue("DriverDesc")?.ToString() ?? "名称获取失败";
							Console.Write("从配置文件中获取的网卡：");
							Console.WriteLine(driverName);
							Console.Write("原自定义 MAC 地址：");
							Console.WriteLine(driver.GetValue("NetworkAddress") ?? "不存在");
							Console.WriteLine("若需更换网卡请自行删除或移动配置文件！");
							Console.WriteLine();
							if (Statics.GetUserConfirm($"您是否确定需要自定义网卡 { driverName } 的 MAC 地址？")) {
								Console.Write("请输入目标 MAC 地址：");
								if (Console.ReadLine().TryExtractMacAddress(out string macAddress)) {
									if (!(IsAdministrator || Statics.GetUserConfirm("您当前未使用管理员权限运行此程序，极有可能写入注册表失败，是否仍要尝试？"))) {
										Console.WriteLine("已取消写入注册表。");
										Console.WriteLine($@"请自行将注册表项 {Statics.regFullPath}\{id}
的字符串值 NetworkAddress 的值设为 {macAddress} 后禁用网卡 {driverName} 后启用或重启电脑以使本次配置生效。");
										return;
									}

									try {
										driver.SetValue("NetworkAddress", macAddress);
										Console.WriteLine($@"已将注册表项 {Statics.regFullPath}\{id}
的字符串值 NetworkAddress 的值设为 {macAddress}");
										Console.WriteLine($"请禁用网卡 {driverName} 后启用或重启电脑以使本次配置生效。");
                                    } catch (System.Security.SecurityException e) {
										Console.WriteLine("当前用户权限不足，写入注册表失败！");
										Console.WriteLine($@"请自行将注册表项 {Statics.regFullPath}\{id}
的字符串值 NetworkAddress 的值设为 {macAddress} 后禁用网卡 {driverName} 后启用或重启电脑以使本次配置生效。");
										Console.WriteLine();
										Console.WriteLine($"异常：{e.Message}\r\n详细信息：");
										Console.WriteLine(e);
									} catch (Exception e) {
										Console.WriteLine("在写入自定义 MAC 地址时发生异常！");
										Console.WriteLine($@"请自行将注册表项 {Statics.regFullPath}\{id}
的字符串值 NetworkAddress 的值设为 {macAddress} 后禁用网卡 {driverName} 后启用或重启电脑以使本次配置生效。");
										Console.WriteLine();
										Console.WriteLine($"异常：{ e.Message }\r\n详细信息：");
										Console.WriteLine(e);
									}
								} else {
									Console.WriteLine("您输入的 MAC 地址有误！");
									Console.WriteLine("可用的格式有：\r\n\tXXXXXXXXXXXX\r\n\tXX:XX:XX:XX:XX:XX\r\n\tXX-XX-XX-XX-XX-XX");
                                }
							} else {
								if (Statics.GetUserConfirm("是否要进入模拟操作模式？", true)) {
									Console.Write("[模拟操作模式]\r\n请输入目标 MAC 地址：");
									if (Console.ReadLine().TryExtractMacAddress(out string macAddress)) {
										Console.WriteLine($@"已将注册表项 {Statics.regFullPath}\{id}
的字符串值 NetworkAddress 的值设为 {macAddress}");
										Console.WriteLine("由于当前是模拟操作模式，上述操作并未实际执行！");
									} else {
										Console.WriteLine("您输入的 MAC 地址有误！");
										Console.WriteLine("可用的格式有：\r\n\tXXXXXXXXXXXX\r\n\tXX:XX:XX:XX:XX:XX\r\n\tXX-XX-XX-XX-XX-XX");
									}
								}
							}
						} catch (Exception e) {
							Console.WriteLine("在读取自定义 MAC 地址时发生异常！");
							Console.WriteLine();
							Console.WriteLine($"异常：{ e.Message }\r\n详细信息：");
							Console.WriteLine(e);
						}
					}
				} catch (Exception e) {
					Console.WriteLine("打开注册表项失败！");
					Console.WriteLine();
					Console.WriteLine($"异常：{ e.Message }\r\n详细信息：");
					Console.WriteLine(e);
				}
			} else {
				string moveTarget = $"{configFileInfo.FullName}_{DateTime.Now:s}.bak";
				Console.WriteLine("配置文件格式有误！\r\n已将原配置文件移动到 " + moveTarget);
				try {
					File.Move(configFileInfo.FullName, moveTarget, true);
				} catch (Exception e) {
					Console.WriteLine("很抱歉，移动配置文件失败！");
					Console.WriteLine();
					Console.WriteLine($"异常：{ e.Message }\r\n详细信息：");
					Console.WriteLine(e);
				}
			}
		} catch (Exception e) {
			Console.WriteLine("读取配置文件失败！");
			Console.WriteLine();
			Console.WriteLine($"异常：{ e.Message }\r\n详细信息：");
			Console.WriteLine(e);
		}
	}
} else {
	try {
		using RegistryKey? netDrivers = Statics.hklm.OpenSubKey(Statics.regPath);
		if (netDrivers == null) {
			Console.WriteLine("您的注册表存在问题！\r\n"
				+ @"无法找到注册表项 " + Statics.regFullPath);
		} else {
			string[] driversID = netDrivers.GetSubKeyNames();
			foreach (string driverID in driversID) {
				if (Statics.idRegex.IsMatch(driverID)) {
					Console.Write(driverID);
					Console.Write(": ");
					try {
						using RegistryKey? driver = netDrivers.OpenSubKey(driverID);
						Console.WriteLine(driver?.GetValue("DriverDesc") ?? "获取名称失败");
					} catch {
						Console.WriteLine("获取信息失败");
					}
				}
			}
			Console.Write("\r\n请输入网卡ID：");
			string? id = Console.ReadLine();

			if (!string.IsNullOrWhiteSpace(id) && Statics.idRegex.IsMatch(id) && driversID.HasValue(id)) {
				try {
					File.WriteAllText(Statics.configFile, id, Statics.utf8);
					Console.WriteLine("写入配置文件成功！\r\n请重启程序！");
				} catch (Exception e) {
					Console.WriteLine("写入配置文件失败！");
					Console.WriteLine($"请自行向 {Statics.configFile} 文件中写入以下内容（不包含换行）：");
					Console.WriteLine(id);
					Console.WriteLine();
					Console.WriteLine($"异常：{ e.Message }\r\n详细信息：");
					Console.WriteLine(e);
				}
			} else {
				Console.WriteLine("您输入的网卡 ID 格式有误或不存在！");
			}
		}
	} catch (Exception e) {
		Console.WriteLine("打开注册表项失败！");
		Console.WriteLine();
		Console.WriteLine($"异常：{ e.Message }\r\n详细信息：");
		Console.WriteLine(e);
	}
}