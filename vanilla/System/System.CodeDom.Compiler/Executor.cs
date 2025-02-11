using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;

namespace System.CodeDom.Compiler;

/// <summary>Provides command execution functions for invoking compilers. This class cannot be inherited.</summary>
[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
public static class Executor
{
	private class ProcessResultReader
	{
		private StreamReader reader;

		private string file;

		public ProcessResultReader(StreamReader reader, string file)
		{
			this.reader = reader;
			this.file = file;
		}

		public void Read()
		{
			StreamWriter streamWriter = new StreamWriter(file);
			try
			{
				string value;
				while ((value = reader.ReadLine()) != null)
				{
					streamWriter.WriteLine(value);
				}
			}
			finally
			{
				streamWriter.Close();
			}
		}
	}

	/// <summary>Executes the command using the specified temporary files and waits for the call to return.</summary>
	/// <param name="cmd">The command to execute. </param>
	/// <param name="tempFiles">A <see cref="T:System.CodeDom.Compiler.TempFileCollection" /> with which to manage and store references to intermediate files generated during compilation. </param>
	public static void ExecWait(string cmd, TempFileCollection tempFiles)
	{
		string outputName = null;
		string errorName = null;
		ExecWaitWithCapture(cmd, Environment.CurrentDirectory, tempFiles, ref outputName, ref errorName);
	}

	/// <summary>Executes the specified command using the specified user token, current directory, and temporary files; then waits for the call to return, storing output and error information from the compiler in the specified strings.</summary>
	/// <returns>The return value from the compiler.</returns>
	/// <param name="userToken">The token to start the compiler process with. </param>
	/// <param name="cmd">The command to execute. </param>
	/// <param name="currentDir">The directory to start the process in. </param>
	/// <param name="tempFiles">A <see cref="T:System.CodeDom.Compiler.TempFileCollection" /> with which to manage and store references to intermediate files generated during compilation. </param>
	/// <param name="outputName">A reference to a string that will store the compiler's message output. </param>
	/// <param name="errorName">A reference to a string that will store the name of the error or errors encountered. </param>
	[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
	[SecurityPermission(SecurityAction.Assert, ControlPrincipal = true)]
	public static int ExecWaitWithCapture(IntPtr userToken, string cmd, string currentDir, TempFileCollection tempFiles, ref string outputName, ref string errorName)
	{
		using (WindowsIdentity.Impersonate(userToken))
		{
			return InternalExecWaitWithCapture(cmd, currentDir, tempFiles, ref outputName, ref errorName);
		}
	}

	/// <summary>Executes the specified command using the specified user token and temporary files, and waits for the call to return, storing output and error information from the compiler in the specified strings.</summary>
	/// <returns>The return value from the compiler.</returns>
	/// <param name="userToken">The token to start the compiler process with. </param>
	/// <param name="cmd">The command to execute. </param>
	/// <param name="tempFiles">A <see cref="T:System.CodeDom.Compiler.TempFileCollection" /> with which to manage and store references to intermediate files generated during compilation. </param>
	/// <param name="outputName">A reference to a string that will store the compiler's message output. </param>
	/// <param name="errorName">A reference to a string that will store the name of the error or errors encountered. </param>
	public static int ExecWaitWithCapture(IntPtr userToken, string cmd, TempFileCollection tempFiles, ref string outputName, ref string errorName)
	{
		return ExecWaitWithCapture(userToken, cmd, Environment.CurrentDirectory, tempFiles, ref outputName, ref errorName);
	}

	/// <summary>Executes the specified command using the specified current directory and temporary files, and waits for the call to return, storing output and error information from the compiler in the specified strings.</summary>
	/// <returns>The return value from the compiler.</returns>
	/// <param name="cmd">The command to execute. </param>
	/// <param name="currentDir">The current directory. </param>
	/// <param name="tempFiles">A <see cref="T:System.CodeDom.Compiler.TempFileCollection" /> with which to manage and store references to intermediate files generated during compilation. </param>
	/// <param name="outputName">A reference to a string that will store the compiler's message output. </param>
	/// <param name="errorName">A reference to a string that will store the name of the error or errors encountered. </param>
	[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
	public static int ExecWaitWithCapture(string cmd, string currentDir, TempFileCollection tempFiles, ref string outputName, ref string errorName)
	{
		return InternalExecWaitWithCapture(cmd, currentDir, tempFiles, ref outputName, ref errorName);
	}

	/// <summary>Executes the specified command using the specified temporary files and waits for the call to return, storing output and error information from the compiler in the specified strings.</summary>
	/// <returns>The return value from the compiler.</returns>
	/// <param name="cmd">The command to execute. </param>
	/// <param name="tempFiles">A <see cref="T:System.CodeDom.Compiler.TempFileCollection" /> with which to manage and store references to intermediate files generated during compilation. </param>
	/// <param name="outputName">A reference to a string that will store the compiler's message output. </param>
	/// <param name="errorName">A reference to a string that will store the name of the error or errors encountered. </param>
	[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
	public static int ExecWaitWithCapture(string cmd, TempFileCollection tempFiles, ref string outputName, ref string errorName)
	{
		return InternalExecWaitWithCapture(cmd, Environment.CurrentDirectory, tempFiles, ref outputName, ref errorName);
	}

	private static int InternalExecWaitWithCapture(string cmd, string currentDir, TempFileCollection tempFiles, ref string outputName, ref string errorName)
	{
		if (cmd == null || cmd.Length == 0)
		{
			throw new ExternalException(global::Locale.GetText("No command provided for execution."));
		}
		if (outputName == null)
		{
			outputName = tempFiles.AddExtension("out");
		}
		if (errorName == null)
		{
			errorName = tempFiles.AddExtension("err");
		}
		int num = -1;
		Process process = new Process();
		process.StartInfo.FileName = cmd;
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardError = true;
		process.StartInfo.WorkingDirectory = currentDir;
		try
		{
			process.Start();
			ProcessResultReader processResultReader = new ProcessResultReader(process.StandardOutput, outputName);
			Thread thread = new Thread(new ProcessResultReader(process.StandardError, errorName).Read);
			thread.Start();
			processResultReader.Read();
			thread.Join();
			process.WaitForExit();
		}
		finally
		{
			num = process.ExitCode;
			process.Close();
		}
		return num;
	}
}
