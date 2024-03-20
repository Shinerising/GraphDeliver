# 设备状态实时传输工具

一款支持通过串口快速转发来自网络的多类型状态数据的轻量级软件。

[![.NET Build](https://github.com/Shinerising/GraphDeliver/actions/workflows/dotnet-build.yml/badge.svg)](https://github.com/Shinerising/ComTransfer/actions/workflows/dotnet-build.yml)
[![.NET Release](https://github.com/Shinerising/GraphDeliver/actions/workflows/dotnet-release.yml/badge.svg)](https://github.com/Shinerising/ComTransfer/actions/workflows/dotnet-release.yml)
[![GitHub Release Version](https://img.shields.io/github/v/release/Shinerising/GraphDeliver)](https://github.com/Shinerising/ComTransfer/releases)
![Code Count](https://tokei.rs/b1/github/Shinerising/GraphDeliver)

![](https://img.shields.io/badge/Windows-0078D6?style=for-the-badge&logo=windows)
![](https://img.shields.io/badge/Visual%20Studio-5C2D91?style=for-the-badge&logo=visualstudio)
![](https://img.shields.io/badge/.NET%20Framework-512BD4?style=for-the-badge&logo=dotnet)
![](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp)
![](https://img.shields.io/badge/XAML-0C54C2?style=for-the-badge&logo=xaml)

## 软件功能

- 采用`.NET Framework 4.0`开发，支持Windows XP及之后所有的Windows系统，同时支持32位和64位系统；
- 采用`.NET Framework`内置API实现串口操作和数据发送功能，工作稳定可靠；
- 可支持使用最高921600波特率进行传输作业，平均数据传输速度超过100KiB/s；
- 支持同时发送多个种类的动态实时数据，并可根据实际工作需要调整数据格式；
- 转发数据时使用Deflate算法进行压缩以减少数据传输时间；
- 支持同时接收多台计算机发送的动态数据并对冗余进行自动化筛选操作；
- 支持通信状态监视、数据统计、错误报告、日志记录等多样化功能；
- 轻量化开发，用户界面简单易于使用，软件启动速度和执行效率较好。
- 
## 界面说明

软件界面从上至下依次为：

- 串口信息：显示串口参数、启动状态、流状态指示等信息，最右侧为串口启动、关闭切换按钮；
- 程序设置：显示当前的数据发送间隔、超时断线计数等设置内容，最右侧为程序设置按钮；
- 网络信息：显示当前网络通信主机的地址信息和通信状态，绿色表示正常通信；
- 工作日志：显示程序的网络通信状态、错误报告等各项工作内容，右侧按钮为日志清除按钮；
- 传输记录：显示所有成功的数据传输记录，右侧按钮为记录清除按钮；

## 操作步骤

以下介绍常见工作流程的具体软件操作步骤：

### 程序设置

### 打开串口通信

## 注意事项

- 为实现较高的数据传输速度，建议使用能够支持较高波特率的通信设备（如NPort、USB虚拟串口等）；
- 受限于串口通信速度，请勿使用过高的数据发送频率。
