@echo off
rem Builds InfamousArabic.exe from InfamousArabic.cs with the C# compiler of the .NET Framework that ships with Windows.
cd /d "%~dp0"
"%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:winexe /optimize+ /codepage:65001 /out:InfamousArabic.exe /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll InfamousArabic.cs
